using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using DisasterServer;
using ExeNet;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Net;
using System.Numerics;

namespace DisasterServer.State
{
    public class Lobby : State
    {
        private bool _isCounting = false;
        private int _countdown = 10 * Ext.FRAMESPSEC;
        private object _cooldownLock = new();
        private int _timeout = 1 * Ext.FRAMESPSEC;
        private Random _rand = new();

        private Dictionary<ushort, int> _lastPackets = new();

        private ushort _voteKickTarget = ushort.MaxValue;
        private int _voteKickTimer = 0;
        private List<ushort> _voteKickVotes = new();
        private bool _voteKick = false;

        private bool _practice = false;
        private List<ushort> _practiceVotes = new();

        public override Session.State AsState()
        {
            return Session.State.LOBBY;
        }

        public override void PeerJoined(Server server, TcpSession session, Peer peer)
        {
            if (_isCounting)
            {
                _isCounting = false;

                // For safety
                lock (_cooldownLock)
                    _countdown = 10 * Ext.FRAMESPSEC;

                /* Send update since new player joined */
                MulticastState(server);
            }

            lock (_lastPackets)
                _lastPackets.Add(peer.ID, 0);

            peer.ExeChance = _rand.Next(2, 5);

            var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, (byte)peer.ExeChance);
            server.TCPSend(session, packet);

            packet = new TcpPacket(PacketType.SERVER_PLAYER_JOINED, peer.ID);
            server.TCPMulticast(packet, peer.ID);
        }

        public override void PeerLeft(Server server, TcpSession session, Peer peer)
        {
            lock (server.Peers)
            {
                if (server.Peers.Count <= 1)
                {
                    _isCounting = false;

                    // For safety
                    lock (_cooldownLock)
                        _countdown = 10 * Ext.FRAMESPSEC;

                    /* Send update since new player joined */
                    MulticastState(server);
                }

                lock (_lastPackets)
                    _lastPackets.Remove(peer.ID);

                Terminal.Log($"{peer.Nickname} (ID {peer.ID}) left.");
            }

            lock (server.Peers)
            {
                if (server.Peers.Count <= 0)
                {
                    lock (_voteKickVotes)
                    {
                        if (_voteKick)
                        {
                            Terminal.Log($"Vote kick failed for {peer.Nickname} (PID {peer.ID}) because everyone left.");

                            _voteKickVotes.Clear();
                            _voteKickTimer = 0;
                            _voteKickTarget = ushort.MaxValue;
                            _voteKick = false;
                        }
                    }

                    lock (_practiceVotes)
                    {
                        if (_practice)
                        {
                            Terminal.Log($"Practice failed because everyone left.");
                            _practiceVotes.Clear();
                            _practice = false;
                        }
                    }
                }
            }

            if (_voteKick)
            {
                lock (_voteKickVotes)
                {
                    if (_voteKickVotes.Contains(session.ID))
                        _voteKickVotes.Remove(session.ID);

                    if (_voteKickTarget == session.ID)
                    {
                        Terminal.Log($"Vote kick failed for {peer.Nickname} (PID {peer.ID}) because player left.");

                        SendMessage(server, $"\\kick vote failed~ (player left)");
                        _voteKickVotes.Clear();
                        _voteKickTimer = 0;
                        _voteKickTarget = ushort.MaxValue;
                        _voteKick = false;
                        return;
                    }

                    if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                        CheckVoteKick(server, false);
                }
            }

            if (_practice)
            {
                lock (_practiceVotes)
                {
                    if (_practiceVotes.Contains(session.ID))
                        _practiceVotes.Remove(session.ID);
                }
            }
        }

        public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            if (passtrough)
                server.Passtrough(reader, session);

            lock (_lastPackets)
                _lastPackets[session.ID] = 0;

            switch ((PacketType)type)
            {
                case PacketType.IDENTITY:
                    {
                        Ext.HandleIdentity(server, session, reader);
                        break;
                    }

                /* Player requests player list */
                case PacketType.CLIENT_LOBBY_PLAYERS_REQUEST:
                    {
                        lock (server.Peers)
                        {
                            foreach (var player in server.Peers)
                            {
                                if (player.Value.Pending)
                                    continue;

                                if (player.Key == session.ID)
                                    continue;

                                Terminal.LogDebug($"Sending {player.Value.Nickname}'s data to PID {session.ID}");

                                var pk = new TcpPacket(PacketType.SERVER_LOBBY_PLAYER);
                                pk.Write(player.Value.ID);
                                pk.Write(player.Value.Player.IsReady);
                                pk.Write(player.Value.Nickname[..Math.Min(player.Value.Nickname.Length, 15)]);
                                pk.Write(player.Value.Icon);
                                pk.Write(player.Value.Pet);

                                server.TCPSend(session, pk);
                            }
                        }

                        server.TCPSend(session, new TcpPacket(PacketType.SERVER_LOBBY_CORRECT));
                        SendMessage(server, session, "|type .help for command list~");
                        SendMessage(server, session, "\\warning: i won't pay for the domain name in 23-04-2024~");
                        SendMessage(server, session, "\\contact to ~the/slavonic~fox\\' discord dm for more information~");
                        break;
                    }

                /* Chat message */
                case PacketType.CLIENT_CHAT_MESSAGE:
                    {
                        var key = reader.ReadUInt16();
                        var msg = reader.ReadStringNull();

                        lock (server.Peers)
                        {
                            switch (msg)
                            {
                                case ".y":
                                case ".yes":
                                    lock (_voteKickVotes)
                                    {
                                        if (_voteKickTimer <= 0)
                                            break;

                                        if (!_voteKickVotes.Contains(session.ID))
                                            _voteKickVotes.Add(session.ID);
                                        else
                                            break;

                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");
                                            Terminal.Log($"{server.Peers[session.ID].Nickname} voted yes");
                                        }

                                        if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                                            CheckVoteKick(server, false);
                                    }

                                    break;

                                case ".n":
                                case ".no":
                                    lock (_voteKickVotes)
                                    {
                                        if (_voteKickTimer <= 0)
                                            break;

                                        if (_voteKickVotes.Contains(session.ID))
                                            _voteKickVotes.Remove(session.ID);

                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} voted \\no~");
                                            Terminal.Log($"{server.Peers[session.ID].Nickname} voted no");
                                        }

                                        if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                                            CheckVoteKick(server, false);
                                    }
                                    break;

                                case ".help":
                                case ".h":
                                    SendMessage(server, session, $"~---|list of commands:~---");
                                    SendMessage(server, session, "@.practice~ (.p) - practice mode vote");
                                    SendMessage(server, session, "@.mute~ (.m) - toggle chat messages");
                                    SendMessage(server, session, "@.votekick~ (.vk) - kick vote a player");
                                    SendMessage(server, session, "@.info~ (.i) - display a server info");
                                    SendMessage(server, session, "@.randomnotready~ (.rnr) - display a name of random not ready");
                                    SendMessage(server, session, "@.rules~ (.r) - display a server rules");
                                    SendMessage(server, session, $"~----------------------");
                                    break;


                                case ".info":
                                case ".i":
                                    SendMessage(server, session, $"~----/disaster\\server:~----");
                                    SendMessage(server, session, "@original by:~ team exe empire");
                                    SendMessage(server, session, "@rebuilt by:~ theslavonicfox");
                                    SendMessage(server, session, "@with a goal of~ public server rebirth");
                                    SendMessage(server, session, "@\\warning: the server build is not stable");
                                    SendMessage(server, session, $"~----------------------");
                                    break;

                                case ".rules":
                                case ".r":
                                    SendMessage(server, session, $"~--------|rules:~--------");
                                    SendMessage(server, session, "@this server is for fair play)~");
                                    SendMessage(server, session, "@don't cross-team with each-other~");
                                    SendMessage(server, session, "@consider using original game or same mod~");
                                    SendMessage(server, session, "@don't use no-delay or speedhack~");
                                    SendMessage(server, session, "@don't lie~");
                                    SendMessage(server, session, $"~----------------------");
                                    break;

                                case ".randomnotready":
                                case ".rnr":
                                    var notReadyPeers = server.Peers.Values
                                        .Where(p => !p.Player.IsReady && !p.Waiting)
                                        .ToList();
                                    if (notReadyPeers.Count > 0)
                                    {
                                        var randomPeer = notReadyPeers[_rand.Next(notReadyPeers.Count)];
                                        SendMessage(server, session, $"random player not ready: {randomPeer.Nickname} (pid: {randomPeer.ID})");
                                    }
                                    else
                                    {
                                        SendMessage(server, session, "all players are ready.");
                                    }
                                    break;

                                case ".practice":
                                case ".p":
                                    if (_practice)
                                    {
                                        lock (_practiceVotes)
                                        {
                                            if (_practiceVotes.Contains(session.ID))
                                                break;

                                            lock (server.Peers)
                                            {
                                                SendMessage(server, $"{server.Peers[session.ID].Nickname} wants to `practice~");
                                                Terminal.Log($"{server.Peers[session.ID].Nickname} wants to practice");

                                                _practiceVotes.Add(session.ID);

                                                if (_practiceVotes.Count >= server.Peers.Count)
                                                    server.SetState(new CharacterSelect(new FartZone()));
                                            }
                                        }
                                        break;
                                    }

                                    lock (_practiceVotes)
                                    {
                                        lock (server.Peers)
                                        {
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} wants to `practice~");
                                            Terminal.Log($"{server.Peers[session.ID].Nickname} wants to practice");

                                            _practiceVotes.Add(session.ID);
                                        }
                                    }

                                    Terminal.Log($"{server.Peers[session.ID].Nickname} started practice vote");
                                    SendMessage(server, $"~----------------------");
                                    SendMessage(server, $"\\`practice~ vote started by /{server.Peers[key].Nickname}~");
                                    SendMessage(server, $"type `.p~ for practice room");
                                    SendMessage(server, $"~----------------------");

                                    _practice = true;
                                    break;

                                default:
                                    foreach (var peer in server.Peers.Values)
                                    {
                                        if (peer.Waiting)
                                            continue;

                                        if (peer.ID != key)
                                            continue;

                                        Terminal.Log($"[{peer.Nickname}]: {msg}");
                                    }
                                    break;
                            }
                        }
                        break;
                    }

                /* New ready state (key Z) */
                case PacketType.CLIENT_LOBBY_READY_STATE:
                    {
                        var ready = reader.ReadBoolean();

                        lock (server.Peers)
                        {
                            if (!server.Peers.ContainsKey(session.ID))
                                break;

                            var peer = server.Peers[session.ID];
                            peer.Player.IsReady = ready;

                            var pk = new TcpPacket(PacketType.SERVER_LOBBY_READY_STATE);
                            pk.Write(peer.ID);
                            pk.Write(ready);
                            server.TCPMulticast(pk, session.ID);

                            CheckReadyPeers(server);
                        }
                        break;
                    }

                case PacketType.CLIENT_LOBBY_VOTEKICK:
                    {
                        var id = reader.ReadUInt16();

                        lock (server.Peers)
                        {
                            lock (_voteKickVotes)
                            {
                                if (id == _voteKickTarget)
                                {
                                    if (!_voteKickVotes.Contains(session.ID))
                                    {
                                        _voteKickVotes.Add(session.ID);

                                        SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");

                                        if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                                            CheckVoteKick(server, false);
                                    }
                                }
                                else if (_voteKick)
                                    SendMessage(server, session, "\\kick vote is already in process!");
                                else
                                {
                                    VoteKickStart(server, session.ID, id);
                                    SendMessage(server, $"{server.Peers[session.ID].Nickname} voted @yes~");
                                }
                            }
                        }

                        break;
                    }
            }
        }

        /* Lobby has no UDP messages */
        public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
        {
        }

        public override void Init(Server server)
        {
            lock (server.Peers)
            {
                foreach (var peer in server.Peers.Values)
                {
                    if (peer.Waiting)
                    {
                        var pak = new TcpPacket(PacketType.SERVER_IDENTITY_RESPONSE);
                        pak.Write(true);
                        pak.Write(peer.ID);
                        server.TCPSend(server.GetSession(peer.ID), pak);

                        peer.Waiting = false;
                    }
                    else
                    {
                        var pk = new TcpPacket(PacketType.SERVER_GAME_BACK_TO_LOBBY);
                        server.TCPSend(server.GetSession(peer.ID), pk);
                    }

                    lock (_lastPackets)
                        _lastPackets.Add(peer.ID, 0);

                    peer.Player = new();

                    if (peer.ExeChance >= 99)
                        peer.ExeChance = 99;

                    var packet = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, (byte)peer.ExeChance);
                    server.TCPSend(server.GetSession(peer.ID), packet);
                }
            }
        }

        public override void Tick(Server server)
        {
            if (_isCounting)
            {
                // For safety
                lock (_cooldownLock)
                {
                    _countdown--;

                    if (_countdown <= 0)
                    {
                        CheckVoteKick(server, true);
                        server.SetState<MapVote>();
                    }
                    else
                    {
                        if (_countdown % Ext.FRAMESPSEC == 0)
                            MulticastState(server);
                    }
                }
            }

            DoVoteKick(server);
            DoTimeout(server);
        }

        private void DoTimeout(Server server)
        {
            if (_timeout-- > 0)
                return;

            lock (server.Peers)
            {
                lock (_lastPackets)
                {
                    foreach (var peer in server.Peers.Values)
                    {
                        if (peer.Waiting)
                        {
                            _lastPackets[peer.ID] = 0;
                            continue;
                        }

                        if (!_lastPackets.ContainsKey(peer.ID))
                            continue;

                        if (peer.Player.IsReady)
                        {
                            _lastPackets[peer.ID] = 0;
                            continue;
                        }

                        if (_lastPackets[peer.ID] >= 25 * Ext.FRAMESPSEC)
                        {
                            server.DisconnectWithReason(server.GetSession(peer.ID), "AFK or Timeout");
                            continue;
                        }

                        _lastPackets[peer.ID] += Ext.FRAMESPSEC;
                    }
                }
            }

            _timeout = 1 * Ext.FRAMESPSEC;
        }

        private void CheckReadyPeers(Server server)
        {
            lock (server.Peers)
            {
                var totalReady = 0;

                foreach (var pr in server.Peers.Values)
                {
                    if (pr.Player.IsReady)
                        totalReady++;
                }

                if (totalReady >= server.Peers.Count && totalReady > 1)
                {
                    _isCounting = true;

                    MulticastState(server);
                }
                else if (_isCounting)
                {
                    _isCounting = false;

                    // For safety
                    lock (_cooldownLock)
                        _countdown = 10 * Ext.FRAMESPSEC;

                    MulticastState(server);
                }
            }
        }

        private void MulticastState(Server server)
        {
            var packet = new TcpPacket
            (
                PacketType.SERVER_LOBBY_COUNTDOWN,
                _isCounting,
                (byte)(_countdown / Ext.FRAMESPSEC)
            );

            server.TCPMulticast(packet);
        }

        private void DoVoteKick(Server server)
        {
            lock (_voteKickVotes)
            {
                if (_voteKickTimer > 0)
                {
                    _voteKickTimer--;
                    if (_voteKickTimer <= 0)
                    {
                        CheckVoteKick(server, true);
                    }
                }
            }
        }

        private void CheckVoteKick(Server server, bool ignore)
        {
            if (_voteKickTimer <= 0 && !ignore)
                return;

            if (!_voteKick)
                return;

            int totalFor = _voteKickVotes.Count;
            int totalAgainst = 0;

            lock (server.Peers)
            {
                if (!server.Peers.ContainsKey(_voteKickTarget))
                {
                    Terminal.Log($"Vote kick failed for PID {_voteKickTarget} because player left.");

                    SendMessage(server, $"\\kick vote failed~ (player left)");
                    _voteKickVotes.Clear();
                    _voteKickTimer = 0;
                    _voteKickTarget = ushort.MaxValue;
                    _voteKick = false;
                    return;
                }

                foreach (var peer in server.Peers)
                {
                    if (peer.Value.Waiting)
                        continue;

                    bool has = false;

                    foreach (var peer2 in _voteKickVotes)
                    {
                        if (peer.Key == peer2)
                        {
                            has = true;
                            break;
                        }
                    }

                    if (!has)
                        totalAgainst++;
                }
            }

            if (totalFor >= totalAgainst)
            {
                Terminal.Log($"Vote kick succeeded for {server.Peers[_voteKickTarget].Nickname} (PID {_voteKickTarget})");

                var session = server.GetSession(_voteKickTarget);
                KickList.Add((session.RemoteEndPoint! as IPEndPoint).Address.ToString()!);

                server.DisconnectWithReason(session, "Vote kick.");
                SendMessage(server, $"@kick vote success~ (@{totalFor}~ vs \\{totalAgainst}~)");
                _voteKickVotes.Clear();
                _voteKickTimer = 0;
                _voteKickTarget = ushort.MaxValue;
                _voteKick = false;
            }
            else
            {
                Terminal.Log($"Vote kick failed for {server.Peers[_voteKickTarget].Nickname} (PID {_voteKickTarget})");

                SendMessage(server, $"\\kick vote failed~ (@{totalFor}~ vs \\{totalAgainst}~)");
                _voteKickVotes.Clear();
                _voteKickTimer = 0;
                _voteKickTarget = ushort.MaxValue;
                _voteKick = false;
            }
        }

        private void VoteKickStart(Server server, ushort voter, ushort id)
        {
            _voteKickTarget = id;
            _voteKickVotes.Clear();
            _voteKickVotes.Add(voter);
            _voteKickTimer = Ext.FRAMESPSEC * 15;
            _voteKick = true;

            SendMessage(server, $"~----------------------");
            SendMessage(server, $"\\kick vote started for /{server.Peers[id].Nickname}~");
            SendMessage(server, $"type @.y~ or \\.n~");
            SendMessage(server, $"~----------------------");

            Terminal.Log($"Vote kick started for {server.Peers[id].Nickname} (PID {id})");
        }

        private void SendMessage(Server server, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPMulticast(pack);
        }

        private void SendMessage(Server server, ushort id, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPSend(server.GetSession(id), pack);
        }

        private void SendMessage(Server server, TcpSession session, string text)
        {
            var pack = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, (ushort)0);
            pack.Write(text);
            server.TCPSend(session, pack);
        }
    }
}

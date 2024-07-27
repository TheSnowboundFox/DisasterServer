using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.State
{
  public class Lobby : DisasterServer.State.State
  {
    public static readonly Type[] Maps = new Type[]
    {
      typeof (HideAndSeek2),
      typeof (RavineMist),
      typeof (DotDotDot),
      typeof (DesertTown),
      typeof (YouCantRun),
      typeof (LimpCity),
      typeof (NotPerfect),
      typeof (KindAndFair),
      typeof (Act9),
      typeof (NastyParadise),
      typeof (PricelessFreedom),
      typeof (VolcanoValley),
      typeof (GreenHill),
      typeof (MajinForest),
      typeof (AngelIsland),
      typeof (TortureCave),
      typeof (DarkTower),
      typeof (HauntingDream),
      typeof (FartZone)
    };
    private bool _isCounting;
    public List<Map> list_of_maps = [
    new HideAndSeek2(),
    new RavineMist(),
    new DotDotDot(),
    new DesertTown(),
    new YouCantRun(),
    new LimpCity(),
    new NotPerfect(),
    new KindAndFair(),
    new Act9(),
    new NastyParadise(),
    new PricelessFreedom(),
    new VolcanoValley(),
    new GreenHill(),
    new MajinForest(),
    new AngelIsland(),
    new TortureCave(),
    new DarkTower(),
    new HauntingDream(),
    new FartZone()
    ];
    private int _countdown = 300;
    private object _cooldownLock = new object();
    private int _timeout = 60;
    private Random _rand = new Random();
    private Dictionary<ushort, int> _lastPackets = new Dictionary<ushort, int>();
    private ushort _voteKickTarget = ushort.MaxValue;
    private int _voteKickTimer;
    private List<ushort> _voteKickVotes = new List<ushort>();
    private bool _voteKick;
    private bool _practice;
    private List<ushort> _practiceVotes = new List<ushort>();
    public override DisasterServer.Session.State AsState() => DisasterServer.Session.State.LOBBY;

    public override void PeerJoined(Server server, TcpSession session, Peer peer)
    {
      if (this._isCounting)
      {
        this._isCounting = false;
        lock (this._cooldownLock)
          this._countdown = 300;
        this.MulticastState(server);
      }
      lock (this._lastPackets)
        this._lastPackets.Add(peer.ID, 0);
      peer.ExeChance = this._rand.Next(2, 5);
      TcpPacket packet1 = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, new object[1]
      {
        (object) (byte) peer.ExeChance
      });
      server.TCPSend(session, packet1);
      TcpPacket packet2 = new TcpPacket(PacketType.SERVER_PLAYER_JOINED, new object[1]
      {
        (object) peer.ID
      });
      server.TCPMulticast(packet2, new ushort?(peer.ID));
    }

    public override void PeerLeft(Server server, TcpSession session, Peer peer)
    {
      lock (server.Peers)
      {
        if (server.Peers.Count <= 1)
        {
          this._isCounting = false;
          lock (this._cooldownLock)
            this._countdown = 300;
          this.MulticastState(server);
        }
        lock (this._lastPackets)
          this._lastPackets.Remove(peer.ID);
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 2);
        interpolatedStringHandler.AppendFormatted(peer.Nickname);
        interpolatedStringHandler.AppendLiteral(" (ID ");
        interpolatedStringHandler.AppendFormatted<ushort>(peer.ID);
        interpolatedStringHandler.AppendLiteral(") left.");
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
      lock (server.Peers)
      {
        if (server.Peers.Count <= 0)
        {
          lock (this._voteKickVotes)
          {
            if (this._voteKick)
            {
              DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(51, 2);
              interpolatedStringHandler.AppendLiteral("Vote kick failed for ");
              interpolatedStringHandler.AppendFormatted(peer.Nickname);
              interpolatedStringHandler.AppendLiteral(" (PID ");
              interpolatedStringHandler.AppendFormatted<ushort>(peer.ID);
              interpolatedStringHandler.AppendLiteral(") because everyone left.");
              Terminal.Log(interpolatedStringHandler.ToStringAndClear());
              this._voteKickVotes.Clear();
              this._voteKickTimer = 0;
              this._voteKickTarget = ushort.MaxValue;
              this._voteKick = false;
            }
          }
          lock (this._practiceVotes)
          {
            if (this._practice)
            {
              Terminal.Log("Practice failed because everyone left.");
              this._practiceVotes.Clear();
              this._practice = false;
            }
          }
        }
      }
      if (this._voteKick)
      {
        lock (this._voteKickVotes)
        {
          if (this._voteKickVotes.Contains(session.ID))
            this._voteKickVotes.Remove(session.ID);
          if ((int) this._voteKickTarget == (int) session.ID)
          {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
            interpolatedStringHandler.AppendLiteral("Vote kick failed for ");
            interpolatedStringHandler.AppendFormatted(peer.Nickname);
            interpolatedStringHandler.AppendLiteral(" (PID ");
            interpolatedStringHandler.AppendFormatted<ushort>(peer.ID);
            interpolatedStringHandler.AppendLiteral(") because player left.");
            Terminal.Log(interpolatedStringHandler.ToStringAndClear());
            this.SendMessage(server, "\\голосование провалено~ (игрок уехал)");
            this._voteKickVotes.Clear();
            this._voteKickTimer = 0;
            this._voteKickTarget = ushort.MaxValue;
            this._voteKick = false;
            return;
          }
          if (this._voteKickVotes.Count >= server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => (int) e.Key != (int) this._voteKickTarget)))
            this.CheckVoteKick(server, false);
        }
      }
      if (!this._practice)
        return;
      lock (this._practiceVotes)
      {
        if (!this._practiceVotes.Contains(session.ID))
          return;
        this._practiceVotes.Remove(session.ID);
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
                        SendMessage(server, session, "|приветствую на '/ледяная ~звезда|'~");
                        SendMessage(server, session, "|напиши @.help| для списка комманд~");
                        if(server.Peers.Count >= 6){
                          SendMessage(server, session, "\\внимание: сервер может быть перегруженным~");
                          SendMessage(server, session, $"\\вы - {server.Peers.Count}-ый игрок из 14~");
                        }
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
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} голосует @за~");
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
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} голосует \\против~");
                                            Terminal.Log($"{server.Peers[session.ID].Nickname} voted no");
                                        }

                                        if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                                            CheckVoteKick(server, false);
                                    }
                                    break;

                                case ".help":
                                case ".h":
                                    SendMessage(server, session, $"~---------|список комманд:~---------");
                                    SendMessage(server, session, "@.practice~ (.p) - голосовать за практику");
                                    SendMessage(server, session, "@.mute~ (.m) - выключить чат (для себя)");
                                    SendMessage(server, session, "@.votekick~ (.vk) - предложить выгнать игрока");
                                    SendMessage(server, session, "@.info~ (.i) - информация о сервере");
                                    SendMessage(server, session, "@.help~ (.h) - список комманд");
                                    SendMessage(server, session, "@.randomnotready~ (.rnr) - случайный неготовый");
                                    SendMessage(server, session, $"~------------------------------------");
                                    break;


                                case ".info":
                                case ".i":
                                    SendMessage(server, session, $"~---------/disaster\\сервер:~---------");
                                    SendMessage(server, session, "@оригинал от:~ team exe empire");
                                    SendMessage(server, session, "@пересобрано:~ foxthelsav-ом");
                                    SendMessage(server, session, "@с целью~ восстановления публичных серверов");
                                    SendMessage(server, session, "@\\внимание: сборка сервера нестабильна");
                                    SendMessage(server, session, $"~---------------------------------");
                                    break;

                                case ".randomnotready":
                                case ".rnr":
                                    var notReadyPeers = server.Peers.Values
                                        .Where(p => !p.Player.IsReady && !p.Waiting)
                                        .ToList();
                                    if (notReadyPeers.Count > 0)
                                    {
                                        var randomPeer = notReadyPeers[_rand.Next(notReadyPeers.Count)];
                                        SendMessage(server, session, $"неготовый игрок: {randomPeer.Nickname} (pid: {randomPeer.ID})");
                                    }
                                    else
                                    {
                                        SendMessage(server, session, "все готовы.");
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
                                                SendMessage(server, $"{server.Peers[session.ID].Nickname} хочет `попрактиковаться~");
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
                                            SendMessage(server, $"{server.Peers[session.ID].Nickname} хочет `попрактиковаться~");
                                            Terminal.Log($"{server.Peers[session.ID].Nickname} wants to practice");

                                            _practiceVotes.Add(session.ID);
                                        }
                                    }

                                    Terminal.Log($"{server.Peers[session.ID].Nickname} started practice vote");
                                    SendMessage(server, $"~----------------------------");
                                    SendMessage(server, $"\\`голосование за практику~ начато /{server.Peers[key].Nickname}~-ом~");
                                    SendMessage(server, $"напиши `.p~ для захода на тренировочную карту");
                                    SendMessage(server, $"~----------------------------");

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

                                        SendMessage(server, $"{server.Peers[session.ID].Nickname} голосует @за~");

                                        if (_voteKickVotes.Count >= server.Peers.Count(e => e.Key != _voteKickTarget))
                                            CheckVoteKick(server, false);
                                    }
                                }
                                else if (_voteKick)
                                    SendMessage(server, session, "\\голосование в процессе!");
                                else
                                {
                                    VoteKickStart(server, session.ID, id);
                                    SendMessage(server, $"{server.Peers[session.ID].Nickname} голосует @за~");
                                }
                            }
                        }

                        break;
                    }
            }
        }


    public override void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data)
    {
    }

    public override void Init(Server server)
    {
      lock (server.Peers)
      {
        foreach (Peer peer in server.Peers.Values)
        {
          if (peer.Waiting)
          {
            TcpPacket packet = new TcpPacket(PacketType.SERVER_IDENTITY_RESPONSE);
            packet.Write(true);
            packet.Write(peer.ID);
            server.TCPSend((TcpSession) server.GetSession(peer.ID), packet);
            peer.Waiting = false;
          }
          else
          {
            TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_BACK_TO_LOBBY);
            server.TCPSend((TcpSession) server.GetSession(peer.ID), packet);
          }
          lock (this._lastPackets)
            this._lastPackets.Add(peer.ID, 0);
          peer.Player = new Player();
          if (peer.ExeChance >= 99)
            peer.ExeChance = 99;
          TcpPacket packet1 = new TcpPacket(PacketType.SERVER_LOBBY_EXE_CHANCE, new object[1]
          {
            (object) (byte) peer.ExeChance
          });
          server.TCPSend((TcpSession) server.GetSession(peer.ID), packet1);
        }
      }
    }

    public override void Tick(Server server)
    {
      if (this._isCounting)
      {
        lock (this._cooldownLock)
        {
          --this._countdown;
          if (this._countdown <= 0)
          {
            this.CheckVoteKick(server, true);
            server.SetState(new CharacterSelect(list_of_maps[this._rand.Next(0, 18)]));
          }
          else if (this._countdown % 60 == 0)
            this.MulticastState(server);
        }
      }
      this.DoVoteKick(server);
      this.DoTimeout(server);
    }

    private void DoTimeout(Server server)
    {
      if (this._timeout-- > 0)
        return;
      lock (server.Peers)
      {
        lock (this._lastPackets)
        {
          foreach (Peer peer in server.Peers.Values)
          {
            if (peer.Waiting)
              this._lastPackets[peer.ID] = 0;
            else if (this._lastPackets.ContainsKey(peer.ID))
            {
              if (peer.Player.IsReady)
                this._lastPackets[peer.ID] = 0;
              else if (this._lastPackets[peer.ID] >= 1500)
                server.DisconnectWithReason((TcpSession) server.GetSession(peer.ID), "AFK or Timeout");
              else
                this._lastPackets[peer.ID] += 60;
            }
          }
        }
      }
      this._timeout = 60;
    }

    private void CheckReadyPeers(Server server)
    {
      lock (server.Peers)
      {
        int num = 0;
        foreach (Peer peer in server.Peers.Values)
        {
          if (peer.Player.IsReady)
            ++num;
        }
        if (num >= server.Peers.Count && num > 1)
        {
          this._isCounting = true;
          this.MulticastState(server);
        }
        else
        {
          if (!this._isCounting)
            return;
          this._isCounting = false;
          lock (this._cooldownLock)
            this._countdown = 300;
          this.MulticastState(server);
        }
      }
    }

    private void MulticastState(Server server)
    {
      TcpPacket packet = new TcpPacket(PacketType.SERVER_LOBBY_COUNTDOWN, new object[2]
      {
        (object) this._isCounting,
        (object) (byte) (this._countdown / 60)
      });
      server.TCPMulticast(packet);
    }

    private void DoVoteKick(Server server)
    {
      lock (this._voteKickVotes)
      {
        if (this._voteKickTimer <= 0)
          return;
        --this._voteKickTimer;
        if (this._voteKickTimer > 0)
          return;
        this.CheckVoteKick(server, true);
      }
    }

    private void CheckVoteKick(Server server, bool ignore)
    {
      if (this._voteKickTimer <= 0 && !ignore || !this._voteKick)
        return;
      int count = this._voteKickVotes.Count;
      int num = 0;
      DefaultInterpolatedStringHandler interpolatedStringHandler;
      lock (server.Peers)
      {
        if (!server.Peers.ContainsKey(this._voteKickTarget))
        {
          interpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
          interpolatedStringHandler.AppendLiteral("Vote kick failed for PID ");
          interpolatedStringHandler.AppendFormatted<ushort>(this._voteKickTarget);
          interpolatedStringHandler.AppendLiteral(" because player left.");
          Terminal.Log(interpolatedStringHandler.ToStringAndClear());
          this.SendMessage(server, "\\голосование провалено~ (игрок уехал)");
          this._voteKickVotes.Clear();
          this._voteKickTimer = 0;
          this._voteKickTarget = ushort.MaxValue;
          this._voteKick = false;
          return;
        }
        foreach (KeyValuePair<ushort, Peer> peer in server.Peers)
        {
          if (!peer.Value.Waiting)
          {
            bool flag = false;
            foreach (ushort voteKickVote in this._voteKickVotes)
            {
              if ((int) peer.Key == (int) voteKickVote)
              {
                flag = true;
                break;
              }
            }
            if (!flag)
              ++num;
          }
        }
      }
      if (count >= num)
      {
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
        interpolatedStringHandler.AppendLiteral("Vote kick succeeded for ");
        interpolatedStringHandler.AppendFormatted(server.Peers[this._voteKickTarget].Nickname);
        interpolatedStringHandler.AppendLiteral(" (PID ");
        interpolatedStringHandler.AppendFormatted<ushort>(this._voteKickTarget);
        interpolatedStringHandler.AppendLiteral(")");
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
        SharedServerSession session = server.GetSession(this._voteKickTarget);
        KickList.Add((session.RemoteEndPoint as IPEndPoint).Address.ToString());
        server.DisconnectWithReason((TcpSession) session, "Vote kick.");
        Server server1 = server;
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
        interpolatedStringHandler.AppendLiteral("@голосование успешно~ (@");
        interpolatedStringHandler.AppendFormatted<int>(count);
        interpolatedStringHandler.AppendLiteral("~ и \\");
        interpolatedStringHandler.AppendFormatted<int>(num);
        interpolatedStringHandler.AppendLiteral("~)");
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        this.SendMessage(server1, stringAndClear);
        this._voteKickVotes.Clear();
        this._voteKickTimer = 0;
        this._voteKickTarget = ushort.MaxValue;
        this._voteKick = false;
      }
      else
      {
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 2);
        interpolatedStringHandler.AppendLiteral("Vote kick failed for ");
        interpolatedStringHandler.AppendFormatted(server.Peers[this._voteKickTarget].Nickname);
        interpolatedStringHandler.AppendLiteral(" (PID ");
        interpolatedStringHandler.AppendFormatted<ushort>(this._voteKickTarget);
        interpolatedStringHandler.AppendLiteral(")");
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
        Server server2 = server;
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
        interpolatedStringHandler.AppendLiteral("\\голосование провалено~ (@");
        interpolatedStringHandler.AppendFormatted<int>(count);
        interpolatedStringHandler.AppendLiteral("~ против \\");
        interpolatedStringHandler.AppendFormatted<int>(num);
        interpolatedStringHandler.AppendLiteral("~)");
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        this.SendMessage(server2, stringAndClear);
        this._voteKickVotes.Clear();
        this._voteKickTimer = 0;
        this._voteKickTarget = ushort.MaxValue;
        this._voteKick = false;
      }
    }

    private void VoteKickStart(Server server, ushort voter, ushort id)
    {
      this._voteKickTarget = id;
      this._voteKickVotes.Clear();
      this._voteKickVotes.Add(voter);
      this._voteKickTimer = 900;
      this._voteKick = true;
      this.SendMessage(server, "~----------------------");
      this.SendMessage(server, "\\голосование начато для /" + server.Peers[id].Nickname + "~");
      this.SendMessage(server, "напиши @.y~ или \\.n~");
      this.SendMessage(server, "~----------------------");
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
      interpolatedStringHandler.AppendLiteral("Vote kick started for ");
      interpolatedStringHandler.AppendFormatted(server.Peers[id].Nickname);
      interpolatedStringHandler.AppendLiteral(" (PID ");
      interpolatedStringHandler.AppendFormatted<ushort>(id);
      interpolatedStringHandler.AppendLiteral(")");
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
    }

    private void SendMessage(Server server, string text)
    {
      TcpPacket packet = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, new object[1]
      {
        (object) (ushort) 0
      });
      packet.Write(text);
      server.TCPMulticast(packet);
    }

    private void SendMessage(Server server, TcpSession session, string text)
    {
      TcpPacket packet = new TcpPacket(PacketType.CLIENT_CHAT_MESSAGE, new object[1]
      {
        (object) (ushort) 0
      });
      packet.Write(text);
      server.TCPSend(session, packet);
    }
  }
}

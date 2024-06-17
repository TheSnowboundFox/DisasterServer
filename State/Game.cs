using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.State
{
  public class Game : DisasterServer.State.State
  {
    public Dictionary<ushort, IPEndPoint> IPEndPoints = new Dictionary<ushort, IPEndPoint>();
    private bool _waiting = true;
    private Map _map;
    private readonly ushort _exeId;
    private int _endTimer = -1;
    private int _demonCount;
    private string _game_mode = Options.Get<string>("game_mode");
    private int _timeout = 60;
    private bool _initMap;
    private Dictionary<ushort, int> _lastPackets = new Dictionary<ushort, int>();
    private Dictionary<ushort, int> _packetTimeouts = new Dictionary<ushort, int>();
    private Dictionary<ushort, RevivalData> _reviveTimer = new Dictionary<ushort, RevivalData>();

    public Game(Map map, ushort exe)
    {
      this._map = map;
      this._map.Game = this;
      this._exeId = exe;
    }

    public override DisasterServer.Session.State AsState() => DisasterServer.Session.State.GAME;

    public override void PeerJoined(Server server, TcpSession session, Peer peer)
    {
    }

    public override void PeerLeft(Server server, TcpSession session, Peer peer)
    {
      lock (this.IPEndPoints)
        this.IPEndPoints.Remove(peer.ID);
      lock (this._lastPackets)
        this._lastPackets.Remove(peer.ID);
      lock (server.Peers)
      {
        if (peer.Player.RevivalTimes >= 2)
          --this._demonCount;
        this.CheckEscapedAndAlive(server);
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) <= 1)
          server.SetState<Lobby>();
      }
      this._map.PeerLeft(server, session, peer);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      Terminal.LogDebug("HandlePlayers()");
      this.HandlePlayers(server, session, reader);
      Terminal.LogDebug("HandleMap()");
      reader.BaseStream.Seek(0L, SeekOrigin.Begin);
      this._map.PeerTCPMessage(server, session, reader);
    }

    public override void PeerUDPMessage(Server server, IPEndPoint endpoint, ref byte[] data)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler;
      try
      {
        if (data.Length == 0)
        {
          interpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
          interpolatedStringHandler.AppendLiteral("Length is 0 from ");
          interpolatedStringHandler.AppendFormatted<IPEndPoint>(endpoint);
          Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        }
        else
        {
          FastBitReader fastBitReader = new FastBitReader();
          ushort num1 = fastBitReader.ReadUShort(ref data);
          PacketType type = (PacketType) fastBitReader.ReadByte(ref data);
          fastBitReader.Position = 3;
          this._lastPackets[num1] = 0;
          switch (type)
          {
            case PacketType.CLIENT_PLAYER_DATA:
              if (!this._waiting)
              {
                ushort key = fastBitReader.ReadUShort(ref data);
                float X = fastBitReader.ReadFloat(ref data);
                float num3 = fastBitReader.ReadFloat(ref data);
                lock (server.Peers)
                {
                  Peer peer;
                  if (server.Peers.TryGetValue(key, out peer))
                  {
                    if (peer != null)
                    {
                      if (peer.Player.Character == Character.Exe && peer.Player.ExeCharacter == ExeCharacter.Original)
                      {
                        int num4 = (int) fastBitReader.ReadByte(ref data);
                        int num5 = (int) fastBitReader.ReadUShort(ref data);
                        int num6 = (int) fastBitReader.ReadByte(ref data);
                        int num7 = (int) fastBitReader.ReadChar(ref data);
                        int num8 = (int) fastBitReader.ReadByte(ref data);
                        int num9 = (int) fastBitReader.ReadByte(ref data);
                        int num10 = (int) fastBitReader.ReadByte(ref data);
                        peer.Player.Invisible = fastBitReader.ReadBoolean(ref data);
                      }
                      peer.Player.X = X;
                      peer.Player.Y = num3;
                    }
                  }
                }
                fastBitReader.Position = 3;
                UdpPacket packet = new UdpPacket(type);
                while (fastBitReader.Position < data.Length)
                  packet.Write(fastBitReader.ReadByte(ref data));
                server.UDPMulticast(ref this.IPEndPoints, packet, endpoint);
                break;
              }
              break;
            case PacketType.CLIENT_PING:
              if (!this.IPEndPoints.Any<KeyValuePair<ushort, IPEndPoint>>((Func<KeyValuePair<ushort, IPEndPoint>, bool>) (e => e.Value.ToString() == endpoint.ToString())) && !this._waiting)
              {
                SharedServerSession session = server.GetSession(num1);
                if (session != null)
                {
                  Terminal.LogDebug("ANTI SERG BOM BOM");
                  server.DisconnectWithReason((TcpSession) session, "invalid session");
                  break;
                }
                break;
              }
              interpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
              interpolatedStringHandler.AppendLiteral("Ping-pong with ");
              interpolatedStringHandler.AppendFormatted<IPEndPoint>(endpoint);
              interpolatedStringHandler.AppendLiteral(" (PID ");
              interpolatedStringHandler.AppendFormatted<ushort>(num1);
              interpolatedStringHandler.AppendLiteral(")");
              Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
              UdpPacket packet1 = new UdpPacket(PacketType.SERVER_PONG);
              ulong num11 = fastBitReader.ReadULong(ref data);
              ushort num12 = fastBitReader.ReadUShort(ref data);
              packet1.Write(num11);
              server.UDPSend(endpoint, packet1);
              UdpPacket packet2 = new UdpPacket(PacketType.SERVER_GAME_PING);
              packet2.Write(num1);
              packet2.Write(num12);
              server.UDPMulticast(ref this.IPEndPoints, packet2, endpoint);
              break;
          }
          if (this._waiting)
          {
            lock (this.IPEndPoints)
            {
              if (!this.IPEndPoints.ContainsKey(num1))
              {
                interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 2);
                interpolatedStringHandler.AppendLiteral("Received from ");
                interpolatedStringHandler.AppendFormatted<IPEndPoint>(endpoint);
                interpolatedStringHandler.AppendLiteral(" (PID ");
                interpolatedStringHandler.AppendFormatted<ushort>(num1);
                interpolatedStringHandler.AppendLiteral(")");
                Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
                this.IPEndPoints.Add(num1, endpoint);
                lock (this._packetTimeouts)
                  this._packetTimeouts[num1] = -1;
              }
              lock (server.Peers)
              {
                if (this.IPEndPoints.Count < server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)))
                  return;
                lock (this._map)
                {
                  if (!this._initMap)
                  {
                    this._map.Init(server);
                    this._initMap = true;
                  }
                }
                Terminal.Log("Got packets from all players.");
                this._waiting = false;
              }
            }
          }
          else
          {
            fastBitReader.Position = 0;
            this._map.PeerUDPMessage(server, endpoint, data);
          }
        }
      }
      catch (Exception ex)
      {
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 2);
        interpolatedStringHandler.AppendLiteral("PeerUDPMessage() failed for ");
        interpolatedStringHandler.AppendFormatted<IPEndPoint>(endpoint);
        interpolatedStringHandler.AppendLiteral(": ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public override void UDPSocketError(IPEndPoint endpoint, SocketError error)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 2);
      interpolatedStringHandler.AppendLiteral("Removing ");
      interpolatedStringHandler.AppendFormatted<IPEndPoint>(endpoint);
      interpolatedStringHandler.AppendLiteral(": ");
      interpolatedStringHandler.AppendFormatted<SocketError>(error);
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
      lock (this.IPEndPoints)
        this.IPEndPoints.Remove(this.IPEndPoints.FirstOrDefault<KeyValuePair<ushort, IPEndPoint>>((Func<KeyValuePair<ushort, IPEndPoint>, bool>) (kvp => kvp.Value == endpoint)).Key);
      base.UDPSocketError(endpoint, error);
    }

    public override void Init(Server server)
    {
      lock (server.Peers)
      {
        foreach (Peer peer in server.Peers.Values)
        {
          if (!peer.Waiting)
          {
            lock (this._lastPackets)
              this._lastPackets.Add(peer.ID, 0);
            lock (this._packetTimeouts)
              this._packetTimeouts.Add(peer.ID, 1080);
            lock (this._reviveTimer)
              this._reviveTimer.Add(peer.ID, new RevivalData());
          }
        }
      }
      TcpPacket packet = new TcpPacket(PacketType.SERVER_LOBBY_GAME_START);
      server.TCPMulticast(packet);
      Terminal.Log("Waiting for players...");
    }

    public override void Tick(Server server)
    {
      if (this._endTimer > 0)
        --this._endTimer;
      else if (this._endTimer == 0)
        server.SetState<Lobby>();
      else if (this._waiting)
      {
        lock (this._packetTimeouts)
        {
          foreach (KeyValuePair<ushort, int> packetTimeout in this._packetTimeouts)
          {
            if (packetTimeout.Value != -1 && this._packetTimeouts[packetTimeout.Key]-- <= 0)
              server.DisconnectWithReason((TcpSession) server.GetSession(packetTimeout.Key), "UDP packets didnt arrive in time");
          }
        }
      }
      else
      {
        lock (this._reviveTimer)
        {
          foreach (KeyValuePair<ushort, RevivalData> keyValuePair in this._reviveTimer)
          {
            if (keyValuePair.Value.Progress > 0.0)
            {
              keyValuePair.Value.Progress -= 0.004;
              if (keyValuePair.Value.Progress <= 0.0)
              {
                keyValuePair.Value.DeathNote.Clear();
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, new object[2]
                {
                  (object) false,
                  (object) keyValuePair.Key
                }));
              }
              server.UDPMulticast(ref this.IPEndPoints, new UdpPacket(PacketType.SERVER_REVIVAL_PROGRESS, new object[2]
              {
                (object) keyValuePair.Key,
                (object) keyValuePair.Value.Progress
              }));
            }
            else
              keyValuePair.Value.Progress = 0.0;
          }
        }
        this.DoTimeout(server);
        this.UpdateDeathTimers(server);
        this._map.Tick(server);
        if (this._map.Timer > 0)
          return;
        this.EndGame(server, 2);
      }
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
            if (!peer.Waiting && this._lastPackets.ContainsKey(peer.ID))
            {
              if (peer.Player.HasEscaped || !peer.Player.IsAlive)
                this._lastPackets[peer.ID] = 0;
              else if (this._lastPackets[peer.ID] >= 240)
                server.DisconnectWithReason((TcpSession) server.GetSession(peer.ID), "AFK or Timeout");
              else
                this._lastPackets[peer.ID] += 60;
            }
          }
        }
      }
      this._timeout = 60;
    }

    private void HandlePlayers(Server server, TcpSession session, BinaryReader reader)
    {
      int num1 = reader.ReadBoolean() ? 1 : 0;
      byte num2 = reader.ReadByte();
      if (num1 != 0)
        server.Passtrough(reader, session);
      PacketType packetType = (PacketType) num2;
      if ((uint) packetType <= 75U)
      {
        if (packetType != PacketType.IDENTITY)
        {
          if (packetType != PacketType.CLIENT_ERECTOR_BALLS)
            return;
          float num3 = reader.ReadSingle();
          float num4 = reader.ReadSingle();
          server.TCPMulticast(new TcpPacket(PacketType.CLIENT_ERECTOR_BALLS, new object[2]
          {
            (object) num3,
            (object) num4
          }));
        }
        else
          Ext.HandleIdentity(server, session, reader);
      }
      else
      {
        switch (packetType)
        {
          case PacketType.CLIENT_REVIVAL_PROGRESS:
            ushort num5 = reader.ReadUInt16();
            byte num6 = reader.ReadByte();
            lock (server.Peers)
            {
              if (server.Peers[num5].Player.IsAlive)
                break;
              if (server.Peers[num5].Player.RevivalTimes >= 2)
                break;
            }
            lock (this._reviveTimer)
            {
              if (this._reviveTimer[num5].Progress <= 0.0)
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, new object[2]
                {
                  (object) true,
                  (object) num5
                }));
              if (!this._reviveTimer[num5].DeathNote.Contains(session.ID))
                this._reviveTimer[num5].DeathNote.Add(session.ID);
              this._reviveTimer[num5].Progress += 0.013 + 0.004 * (double) num6;
              if (this._reviveTimer[num5].Progress > 1.0)
              {
                foreach (ushort id in this._reviveTimer[num5].DeathNote)
                  server.TCPSend((TcpSession) server.GetSession(id), new TcpPacket(PacketType.SERVER_REVIVAL_RINGSUB));
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, new object[2]
                {
                  (object) false,
                  (object) num5
                }));
                server.TCPSend((TcpSession) server.GetSession(num5), new TcpPacket(PacketType.SERVER_REVIVAL_REVIVED));
                this._reviveTimer[num5] = new RevivalData();
                break;
              }
              server.UDPMulticast(ref this.IPEndPoints, new UdpPacket(PacketType.SERVER_REVIVAL_PROGRESS, new object[2]
              {
                (object) num5,
                (object) this._reviveTimer[num5].Progress
              }));
              break;
            }
          case PacketType.CLIENT_PLAYER_DEATH_STATE:
            if (this._endTimer >= 0)
              break;
            lock (server.Peers)
            {
              if (!server.Peers.ContainsKey(session.ID) || server.Peers[session.ID].Player.RevivalTimes >= 2)
                break;
              Peer peer = server.Peers[session.ID];
              peer.Player.IsAlive = !reader.ReadBoolean();
              peer.Player.RevivalTimes = (int) reader.ReadByte();
              server.TCPMulticast(new TcpPacket(PacketType.SERVER_PLAYER_DEATH_STATE, new object[3]
              {
                (object) session.ID,
                (object) peer.Player.IsAlive,
                (object) (byte) peer.Player.RevivalTimes
              }));
              lock (this._reviveTimer)
                this._reviveTimer[peer.ID] = new RevivalData();
              server.TCPMulticast(new TcpPacket(PacketType.SERVER_REVIVAL_STATUS, new object[2]
              {
                (object) false,
                (object) session.ID
              }));
              if (!peer.Player.IsAlive)
              {
                Terminal.Log(peer.Nickname + " died.");
                if (peer.Player.DiedBefore || this._map.Timer <= 7200)
                {
                  TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);
                  if (this._demonCount >= (int) ((double) (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) / 2.0))
                  {
                    packet.Write(0);
                  }
                  else
                  {
                    peer.Player.RevivalTimes = 2;
                    ++this._demonCount;
                    Terminal.Log(peer.Nickname + " was demonized!");
                    packet.Write(1);
                  }
                  server.TCPSend(session, packet);
                  peer.Player.DeadTimer = -1f;
                }
                if (peer.Player.RevivalTimes == 0)
                  peer.Player.DeadTimer = 1800f;
                peer.Player.DiedBefore = true;
              }
              else
                peer.Player.DeadTimer = -1f;
              this.CheckEscapedAndAlive(server);
              break;
            }
          case PacketType.CLIENT_PLAYER_ESCAPED:
            if (this._endTimer >= 0)
              break;
            lock (server.Peers)
            {
              if (!server.Peers.ContainsKey(session.ID))
                break;
              Peer peer = server.Peers[session.ID];
              peer.Player.HasEscaped = true;
              TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_PLAYER_ESCAPED);
              packet.Write(peer.ID);
              server.TCPMulticast(packet);
              this.CheckEscapedAndAlive(server);
              Terminal.Log(peer.Nickname + " has escaped!");
              break;
            }
        }
      }
    }

    private void UpdateDeathTimers(Server server)
    {
      lock (server.Peers)
      {
        foreach (Peer peer in (IEnumerable<Peer>) server.Peers.Values.OrderBy<Peer, float>((Func<Peer, float>) (e => e.Player.DeadTimer)))
        {
          if (!peer.Waiting)
          {
            if (peer.Player.IsAlive || peer.Player.HasEscaped)
              peer.Player.DeadTimer = -1f;
            else if ((double) peer.Player.DeadTimer != -1.0)
            {
              if ((int) peer.Player.DeadTimer % 60 == 0)
              {
                TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_TICK);
                packet.Write(peer.ID);
                packet.Write((byte) ((double) peer.Player.DeadTimer / 60.0));
                server.TCPMulticast(packet);
              }
              peer.Player.DeadTimer -= Ext.Dist((double) peer.Player.X, (double) peer.Player.Y, (double) server.Peers[this._exeId].Player.X, (double) server.Peers[this._exeId].Player.Y) >= 240.0 ? 1f : 0.5f;
              if ((double) peer.Player.DeadTimer <= 0.0 || this._map.Timer <= 7200)
              {
                TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_DEATHTIMER_END);
                if (this._demonCount >= (int) ((double) (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) / 2.0))
                {
                  packet.Write(0);
                }
                else
                {
                  ++this._demonCount;
                  peer.Player.RevivalTimes = 2;
                  Terminal.Log(peer.Nickname + " was demonized!");
                  packet.Write(1);
                }
                server.TCPSend((TcpSession) server.GetSession(peer.ID), packet);
                peer.Player.DeadTimer = -1f;
              }
            }
          }
        }
      }
    }

    private void CheckEscapedAndAlive(Server server)
    {
      lock (server.Peers)
      {
        if (this._endTimer >= 0)
          return;
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) <= 0)
          server.SetState<Lobby>();
        else if (!server.Peers.ContainsKey(this._exeId))
        {
          this.EndGame(server, 1);
        }
        else
        {
          int num1 = 0;
          int num2 = 0;
          foreach (KeyValuePair<ushort, Peer> peer in server.Peers)
          {
            if (!peer.Value.Waiting && (int) peer.Key != (int) this._exeId)
            {
              if (peer.Value.Player.IsAlive)
                ++num1;
              if (peer.Value.Player.HasEscaped)
                ++num2;
            }
          }
          if (num1 == 0 && num2 == 0)
          {
            this.EndGame(server, 0);
          }
          else
          {
            if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - num1 + num2 < server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)))
              return;
            if (num2 == 0)
              this.EndGame(server, 0);
            else
              this.EndGame(server, 1);
          }
        }
      }
    }

    public void EndGame(Server server, int type)
    {
      if (this._endTimer >= 0)
        return;
      switch (type)
      {
        case 0:
          this._endTimer = 300;
          Terminal.Log("Exe wins!");
          TcpPacket packet1 = new TcpPacket(PacketType.SERVER_GAME_EXE_WINS);
          server.TCPMulticast(packet1);
          break;
        case 1:
          this._endTimer = 300;
          Terminal.Log("Survivors win!");
          TcpPacket packet2 = new TcpPacket(PacketType.SERVER_GAME_SURVIVOR_WIN);
          server.TCPMulticast(packet2);
          break;
        case 2:
          this._endTimer = 300;
          Terminal.Log("Time over!");
          TcpPacket packet3 = new TcpPacket(PacketType.SERVER_GAME_TIME_OVER);
          server.TCPMulticast(packet3);
          TcpPacket packet4 = new TcpPacket(PacketType.SERVER_GAME_TIME_SYNC);
          packet4.Write((ushort) 0);
          server.TCPMulticast(packet4);
          break;
      }
    }
  }
}

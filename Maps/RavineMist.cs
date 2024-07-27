// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.RavineMist
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Entities;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.Maps
{
  public class RavineMist : Map
  {
    private readonly Vector2[] _slimeSpawnPoints = new Vector2[11]
    {
      new Vector2(1901, 392),
      new Vector2(2193, 392),
      new Vector2(2468, 392),
      new Vector2(1188, 860),
      new Vector2(2577, 1952),
      new Vector2(2564, 2264),
      new Vector2(2782, 2264),
      new Vector2(1441, 2264),
      new Vector2(884, 2264),
      new Vector2(988, 2004),
      new Vector2(915, 2004)
    };
    private readonly Vector2[] _shardSpawnPoints = new Vector2[12]
    {
      new Vector2(862, 248),
      new Vector2(3078, 248),
      new Vector2(292, 558),
      new Vector2(2918, 558),
      new Vector2(1100, 820),
      new Vector2(980, 1188),
      new Vector2(1870, 1252),
      new Vector2(2180, 1508),
      new Vector2(2920, 2216),
      new Vector2(282, 2228),
      new Vector2(1318, 1916),
      new Vector2(3010, 1766)
    };
    private Dictionary<ushort, byte> _playersShardCount = new Dictionary<ushort, byte>();
    private Random _rand = new Random();
    private int _ringLoc;

    public override void Init(Server server)
    {
      IEnumerable<Vector2> vector2s = ((IEnumerable<Vector2>) this._shardSpawnPoints).OrderBy<Vector2, int>((Func<Vector2, int>) (e => this._rand.Next())).Take<Vector2>(7);
      lock (this.Entities)
      {
        foreach (Vector2 vector2 in vector2s)
          this.Spawn<RMZShard>(server, new RMZShard(vector2.X, vector2.Y));
        foreach (Vector2 slimeSpawnPoint in this._slimeSpawnPoints)
          this.Spawn<RMZSlimeSpawner>(server, new RMZSlimeSpawner(slimeSpawnPoint.X, slimeSpawnPoint.Y));
      }
      lock (server.Peers)
      {
        foreach (KeyValuePair<ushort, Peer> peer in server.Peers)
        {
          if (!peer.Value.Waiting)
            this._playersShardCount.Add(peer.Key, (byte) 0);
        }
      }
      this._ringLoc = this._rand.Next((int) byte.MaxValue);
      this.SetTime(server, 180);
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    public override void PeerLeft(Server server, TcpSession session, Peer peer)
    {
      lock (this._playersShardCount)
      {
        lock (server.Peers)
        {
          lock (this.Entities)
          {
            for (int index = 0; index < (int) this._playersShardCount[session.ID]; ++index)
              this.Spawn<RMZShard>(server, new RMZShard((int) ((double) peer.Player.X + (double) this._rand.Next(-8, 8)), (int) peer.Player.Y, true));
          }
          this._playersShardCount.Remove(session.ID);
        }
      }
      this.SendRingState(server);
      base.PeerLeft(server, session, peer);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      switch ((PacketType) reader.ReadByte())
      {
        case PacketType.CLIENT_RMZSLIME_HIT:
          lock (this.Entities)
          {
            byte id = reader.ReadByte();
            ushort pid = reader.ReadUInt16();
            bool isProjectile = reader.ReadBoolean();
            DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(14, 1);
            interpolatedStringHandler1.AppendLiteral("Killing slime ");
            interpolatedStringHandler1.AppendFormatted<byte>(id);
            Terminal.Log(interpolatedStringHandler1.ToStringAndClear());
            RMZSlimeSpawner[] ofType = this.FindOfType<RMZSlimeSpawner>();
            if (ofType != null)
            {
              RMZSlimeSpawner rmzSlimeSpawner = ((IEnumerable<RMZSlimeSpawner>) ofType).Where<RMZSlimeSpawner>((Func<RMZSlimeSpawner, bool>) (e => e.ID == (int) id)).FirstOrDefault<RMZSlimeSpawner>();
              if (rmzSlimeSpawner != null)
              {
                Peer killer;
                lock (server.Peers)
                  killer = server.Peers.Values.Where<Peer>((Func<Peer, bool>) (e => (int) e.ID == (int) pid && !e.Waiting)).FirstOrDefault<Peer>();
                if (killer != null)
                {
                  rmzSlimeSpawner.KillSlime(server, (Map) this, killer, isProjectile);
                  DefaultInterpolatedStringHandler interpolatedStringHandler2 = new DefaultInterpolatedStringHandler(13, 1);
                  interpolatedStringHandler2.AppendLiteral("Killed slime ");
                  interpolatedStringHandler2.AppendFormatted<byte>(id);
                  Terminal.Log(interpolatedStringHandler2.ToStringAndClear());
                  break;
                }
                break;
              }
              break;
            }
            break;
          }
        case PacketType.CLIENT_RMZSHARD_COLLECT:
          lock (this.Entities)
          {
            byte gid = reader.ReadByte();
            RMZShard[] ofType = this.FindOfType<RMZShard>();
            if (ofType == null)
              return;
            RMZShard rmzShard = ((IEnumerable<RMZShard>) ofType).FirstOrDefault<RMZShard>((Func<RMZShard, bool>) (e => (int) e.ID == (int) gid));
            if (rmzShard == null)
              return;
            lock (this._shardSpawnPoints)
              this._playersShardCount[session.ID]++;
            server.TCPMulticast(new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, new object[3]
            {
              (object) (byte) 2,
              (object) rmzShard.ID,
              (object) session.ID
            }));
            this.Destroy(server, (Entity) rmzShard);
            this.SendRingState(server);
            break;
          }
        case PacketType.CLIENT_PLAYER_DEATH_STATE:
          lock (this.Entities)
          {
            int num1 = !reader.ReadBoolean() ? 1 : 0;
            int num2 = (int) reader.ReadByte();
            if (num1 == 0)
            {
              lock (this._playersShardCount)
              {
                lock (server.Peers)
                {
                  for (int index = 0; index < (int) this._playersShardCount[session.ID]; ++index)
                    this.Spawn<RMZShard>(server, new RMZShard((int) ((double) server.Peers[session.ID].Player.X + (double) this._rand.Next(-8, 8)), (int) server.Peers[session.ID].Player.Y, true));
                  this._playersShardCount[session.ID] = (byte) 0;
                }
              }
              this.SendRingState(server);
              break;
            }
            break;
          }
      }
      base.PeerTCPMessage(server, session, reader);
    }

    public override void PeerUDPMessage(Server server, IPEndPoint endpoint, byte[] data)
    {
      base.PeerUDPMessage(server, endpoint, data);
    }

    protected override void DoBigRingTimer(Server server)
    {
      if (this.Timer - 3600 <= 0 && !this.BigRingSpawned)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
        packet.Write(false);
        packet.Write((byte) this._ringLoc);
        server.TCPMulticast(packet);
        this.BigRingSpawned = true;
      }
      if (this.Timer - this.RingActivateTime > 0 || this.BigRingReady)
        return;
      lock (this.Entities)
      {
        int num = 7 - this.Entities.Count<Entity>((Func<Entity, bool>) (e => e is RMZShard));
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
        packet.Write(num >= 6);
        packet.Write((byte) this._ringLoc);
        server.TCPMulticast(packet);
        this.BigRingSpawned = true;
      }
    }

    public void SendRingState(Server server)
    {
      lock (this.Entities)
      {
        int num = 7 - this.Entities.Count<Entity>((Func<Entity, bool>) (e => e is RMZShard));
        server.TCPMulticast(new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, new object[2]
        {
          (object) (byte) 3,
          (object) (byte) num
        }));
        if (this.Timer - this.RingActivateTime > 0 || this.BigRingReady)
          return;
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
        packet.Write(num >= 6);
        packet.Write((byte) this._ringLoc);
        server.TCPMulticast(packet);
        this.BigRingSpawned = true;
      }
    }

    protected override int GetRingSpawnCount() => 27;
  }
}

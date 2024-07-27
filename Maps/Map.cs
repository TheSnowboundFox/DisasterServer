using DisasterServer.Data;
using DisasterServer.Entities;
using DisasterServer.Session;
using DisasterServer.State;
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
  public abstract class Map
  {
    public Game Game;
    public int Timer = 7200;
    public List<Entity> Entities = new List<Entity>();
    public bool BigRingSpawned;
    public bool BigRingReady;
    public ushort RingIDs = 1;
    public ushort BRingsIDs = 1;
    public byte ExellerCloneIDs = 1;
    protected int RingActivateTime = 3000;
    private float _ringCoff;
    private int _ringTimer = -240;
    private Random _rand = new Random();
    private bool[] _ringSpawns;
    private DateTime lastExellerSpawnCloneTime = DateTime.MinValue;
    private readonly TimeSpan ExellerSpawnCloneCooldown = TimeSpan.FromSeconds(4);
    private DateTime lastErectorBRingSpawnTime = DateTime.MinValue;
    private readonly TimeSpan ErectorBRingSpawnCooldown = TimeSpan.FromSeconds(41);
    public virtual void Init(Server server)
    {
      this._ringCoff = this.GetRingTime();
      lock (server.Peers)
      {
        if (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) > 3)
          --this._ringCoff;
      }
      this._ringSpawns = new bool[this.GetRingSpawnCount()];
      TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_PLAYERS_READY);
      server.TCPMulticast(packet);
    }

    public virtual void Tick(Server server)
    {
      if (this.Timer % 60 == 0)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_TIME_SYNC);
        packet.Write((ushort) this.Timer);
        server.TCPMulticast(packet);
      }
      this.DoRingTimer(server);
      this.DoBigRingTimer(server);
      if (this.Timer > 0)
        --this.Timer;
      lock (this.Entities)
      {
        for (int index = 0; index < this.Entities.Count; ++index)
        {
          UdpPacket packet = this.Entities[index].Tick(server, this.Game, this);
          if (packet != null)
            server.UDPMulticast(ref this.Game.IPEndPoints, packet);
        }
      }
    }

    public virtual void PeerLeft(Server server, TcpSession session, Peer peer)
    {
    }

    public virtual void PeerUDPMessage(Server server, IPEndPoint endpoint, byte[] data)
    {
    }

    public virtual void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.BaseStream.Seek(0L, SeekOrigin.Begin);
      reader.ReadBoolean();
            switch ((PacketType)reader.ReadByte())
            {
                case PacketType.CLIENT_ETRACKER:
                    EggmanTracker eggmanTracker1 = new EggmanTracker();
                    eggmanTracker1.X = (int)reader.ReadUInt16();
                    eggmanTracker1.Y = (int)reader.ReadUInt16();
                    EggmanTracker entity1 = eggmanTracker1;
                    this.Spawn<EggmanTracker>(server, entity1);
                    break;
                case PacketType.CLIENT_ETRACKER_ACTIVATED:
                    lock (this.Entities)
                    {
                        byte id = reader.ReadByte();
                        ushort num = reader.ReadUInt16();
                        EggmanTracker[] ofType = this.FindOfType<EggmanTracker>();
                        if (ofType == null)
                            break;
                        EggmanTracker eggmanTracker2 = ((IEnumerable<EggmanTracker>)ofType).Where<EggmanTracker>((Func<EggmanTracker, bool>)(e => (int)e.ID == (int)id)).FirstOrDefault<EggmanTracker>();
                        if (eggmanTracker2 == null)
                            break;
                        eggmanTracker2.ActivatorID = num;
                        this.Destroy(server, (Entity)eggmanTracker2);
                        break;
                    }
                case PacketType.CLIENT_TPROJECTILE:
                    if (this.FindOfType<TailsProjectile>().Length != 0)
                        break;
                    TailsProjectile tailsProjectile = new TailsProjectile();
                    tailsProjectile.OwnerID = session.ID;
                    tailsProjectile.X = (int)reader.ReadUInt16();
                    tailsProjectile.Y = (int)reader.ReadUInt16();
                    tailsProjectile.Direction = reader.ReadSByte();
                    tailsProjectile.Damage = reader.ReadByte();
                    tailsProjectile.IsExe = reader.ReadBoolean();
                    tailsProjectile.Charge = reader.ReadByte();
                    TailsProjectile entity2 = tailsProjectile;
                    this.Spawn<TailsProjectile>(server, entity2);
                    break;
                case PacketType.CLIENT_TPROJECTILE_HIT:
                    this.Destroy<TailsProjectile>(server);
                    break;
                case PacketType.CLIENT_ERECTOR_BRING_SPAWN:
                    if (DateTime.Now - lastErectorBRingSpawnTime < ErectorBRingSpawnCooldown)
                    {
                        break;
                    }

                    lastErectorBRingSpawnTime = DateTime.Now;

                    float num1 = reader.ReadSingle();
                    float num2 = reader.ReadSingle();
                    BlackRing blackRing1 = this.Spawn<BlackRing>(server, false);
                    blackRing1.ID = this.BRingsIDs++;
                    server.TCPMulticast(new TcpPacket(PacketType.SERVER_ERECTOR_BRING_SPAWN, new object[3]
                    {
            (object) blackRing1.ID,
            (object) num1,
            (object) num2
                    }));
                    break;
                case PacketType.CLIENT_EXELLER_SPAWN_CLONE:
                    if (DateTime.Now - lastExellerSpawnCloneTime < ExellerSpawnCloneCooldown)
                    {
                        break;
                    }

                    lastExellerSpawnCloneTime = DateTime.Now;

                    if (this.FindOfType<ExellerClone>().Length > 1) {
                    break; }
          ushort x = reader.ReadUInt16();
          ushort y = reader.ReadUInt16();
          sbyte dir = reader.ReadSByte();
          this.Spawn<ExellerClone>(server, new ExellerClone(session.ID, this.ExellerCloneIDs++, (int) x, (int) y, dir));
          break;
        case PacketType.CLIENT_EXELLER_TELEPORT_CLONE:
          byte uid1 = reader.ReadByte();
          ExellerClone[] ofType1 = this.FindOfType<ExellerClone>();
          if (ofType1 == null)
            break;
          ExellerClone exellerClone = ((IEnumerable<ExellerClone>) ofType1).Where<ExellerClone>((Func<ExellerClone, bool>) (e => (int) e.ID == (int) uid1)).FirstOrDefault<ExellerClone>();
          if (exellerClone == null)
            break;
          this.Destroy(server, (Entity) exellerClone);
          break;
        case PacketType.CLIENT_RING_COLLECTED:
          lock (this.Entities)
          {
            int num3 = (int) reader.ReadByte();
            ushort uid2 = reader.ReadUInt16();
            Ring[] ofType2 = this.FindOfType<Ring>();
            if (ofType2 == null)
              break;
            Ring ring = ((IEnumerable<Ring>) ofType2).Where<Ring>((Func<Ring, bool>) (e => (int) e.ID == (int) uid2)).FirstOrDefault<Ring>();
            if (ring == null)
              break;
            TcpPacket packet = new TcpPacket(PacketType.SERVER_RING_COLLECTED, new object[1]
            {
              (object) ring.IsRedRing
            });
            server.TCPSend(session, packet);
            this.Destroy(server, (Entity) ring);
            break;
          }
        case PacketType.CLIENT_BRING_COLLECTED:
          lock (this.Entities)
          {
            ushort uid3 = reader.ReadUInt16();
            BlackRing[] ofType3 = this.FindOfType<BlackRing>();
            if (ofType3 == null)
              break;
            BlackRing blackRing2 = ((IEnumerable<BlackRing>) ofType3).Where<BlackRing>((Func<BlackRing, bool>) (e => (int) e.ID == (int) uid3)).FirstOrDefault<BlackRing>();
            if (blackRing2 == null)
              break;
            TcpPacket packet = new TcpPacket(PacketType.SERVER_BRING_COLLECTED);
            server.TCPSend(session, packet);
            this.Destroy(server, (Entity) blackRing2);
            break;
          }
            case PacketType.CLIENT_CREAM_SPAWN_RINGS:
                ushort num4 = reader.ReadUInt16();
                ushort num5 = reader.ReadUInt16();
                if (reader.ReadBoolean())
                {
                    for (int index = 0; index < 2; ++index)
                    {
                        Server server1 = server;
                        CreamRing entity3 = new CreamRing();
                        entity3.X = (int)((double)num4 + Math.Sin(5.0 * Math.PI / 2.0 - (double)index * Math.PI) * 26.0) - 1;
                        entity3.Y = (int)((double)num5 + Math.Cos(5.0 * Math.PI / 2.0 - (double)index * Math.PI) * 26.0);
                        entity3.IsRedRing = true;
                        this.Spawn<CreamRing>(server1, entity3);
                    }
                    break;
                }
                for (int index = 0; index < 3; ++index)
                {
                    Server server2 = server;
                    CreamRing entity4 = new CreamRing();
                    entity4.X = (int)((double)num4 + Math.Sin(5.0 * Math.PI / 2.0 + (double)index * (Math.PI / 2.0)) * 26.0);
                    entity4.Y = (int)((double)num5 + Math.Cos(5.0 * Math.PI / 2.0 + (double)index * (Math.PI / 2.0)) * 26.0);
                    entity4.IsRedRing = false;
                    this.Spawn<CreamRing>(server2, entity4);
                }
                break;
            }
    }

    public void SetTime(Server server, int seconds)
    {
      this.Timer = seconds * 60 + this.GetPlayerOffset(server) * 60;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
      interpolatedStringHandler.AppendLiteral("Timer is set to ");
      interpolatedStringHandler.AppendFormatted<int>(this.Timer);
      interpolatedStringHandler.AppendLiteral(" frames");
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
    }

    public void ActivateRingAfter(int afterSeconds)
    {
      this.RingActivateTime = 3600 - afterSeconds * 60;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
      interpolatedStringHandler.AppendLiteral("Ring activate time is set to ");
      interpolatedStringHandler.AppendFormatted<int>(this.Timer);
      interpolatedStringHandler.AppendLiteral(" frames");
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
    }

    public void FreeRingID(byte iid)
    {
      lock (this._ringSpawns)
        this._ringSpawns[(int) iid] = false;
    }

    public bool GetFreeRingID(out byte iid)
    {
      lock (this._ringSpawns)
      {
        if (((IEnumerable<bool>) this._ringSpawns).Where<bool>((Func<bool, bool>) (e => !e)).Count<bool>() <= 0)
        {
          iid = (byte) 0;
          return false;
        }
        byte index;
        do
        {
          index = (byte) this._rand.Next(this._ringSpawns.Length);
        }
        while (this._ringSpawns[(int) index]);
        this._ringSpawns[(int) index] = true;
        iid = index;
        return true;
      }
    }

    public T? Spawn<T>(Server server, bool callSpawn = true) where T : Entity
    {
      T ofType = Ext.CreateOfType<T>();
      if ((object) ofType == null)
        return default (T);
      TcpPacket packet = (TcpPacket) null;
      lock (this.Entities)
      {
        this.Entities.Add((Entity) ofType);
        if (callSpawn)
          packet = ofType.Spawn(server, this.Game, this);
      }
      if (packet != null)
        server.TCPMulticast(packet);
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
      interpolatedStringHandler.AppendLiteral("Entity ");
      interpolatedStringHandler.AppendFormatted<T>(ofType);
      interpolatedStringHandler.AppendLiteral(" spawned.");
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
      return ofType;
    }

    public T Spawn<T>(Server server, T entity, bool callSpawn = true) where T : Entity
    {
      TcpPacket packet = (TcpPacket) null;
      lock (this.Entities)
      {
        this.Entities.Add((Entity) entity);
        if (callSpawn)
          packet = entity.Spawn(server, this.Game, this);
      }
      if (packet != null)
        server.TCPMulticast(packet);
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(16, 1);
      interpolatedStringHandler.AppendLiteral("Entity ");
      interpolatedStringHandler.AppendFormatted<T>(entity);
      interpolatedStringHandler.AppendLiteral(" spawned.");
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
      return entity;
    }

    public void Destroy(Server server, Entity entity)
    {
      TcpPacket packet;
      lock (this.Entities)
      {
        this.Entities.Remove(entity);
        packet = entity.Destroy(server, this.Game, this);
      }
      if (packet != null)
        server.TCPMulticast(packet);
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
      interpolatedStringHandler.AppendLiteral("Entity ");
      interpolatedStringHandler.AppendFormatted<Entity>(entity);
      interpolatedStringHandler.AppendLiteral(" destroyed.");
      Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
    }

    public void Destroy<T>(Server server) where T : Entity
    {
      lock (this.Entities)
      {
        T[] ofType = this.FindOfType<T>();
        if (ofType == null)
          return;
        foreach (T obj in ofType)
        {
          this.Destroy(server, (Entity) obj);
          DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
          interpolatedStringHandler.AppendLiteral("Entity ");
          interpolatedStringHandler.AppendFormatted<T>(obj);
          interpolatedStringHandler.AppendLiteral(" destroyed.");
          Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        }
      }
    }

    public T[]? FindOfType<T>() where T : Entity
    {
      lock (this.Entities)
      {
        Entity[] array = this.Entities.Where<Entity>((Func<Entity, bool>) (e => e is T)).ToArray<Entity>();
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
        interpolatedStringHandler.AppendLiteral("Entity search found ");
        interpolatedStringHandler.AppendFormatted<int>(array.Length);
        interpolatedStringHandler.AppendLiteral(" entities of type ");
        interpolatedStringHandler.AppendFormatted(typeof (T).FullName);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        return Array.ConvertAll<Entity, T>(array, (Converter<Entity, T>) (e => (T) e));
      }
    }

    private void DoRingTimer(Server server)
    {
      if ((double) this._ringTimer >= (double) this._ringCoff * 60.0)
      {
        this._ringTimer = 0;
        byte iid;
        if (!this.GetFreeRingID(out iid))
          return;
        this.Spawn<Ring>(server, new Ring() { IID = iid });
      }
      ++this._ringTimer;
    }

    protected virtual void DoBigRingTimer(Server server)
    {
      if (this.Timer - 3600 <= 0 && !this.BigRingSpawned)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
        packet.Write(false);
        packet.Write((byte) this._rand.Next((int) byte.MaxValue));
        server.TCPMulticast(packet);
        this.BigRingSpawned = true;
      }
      if (this.Timer - this.RingActivateTime > 0 || this.BigRingReady)
        return;
      TcpPacket packet1 = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
      packet1.Write(true);
      packet1.Write((byte) this._rand.Next((int) byte.MaxValue));
      server.TCPMulticast(packet1);
      this.BigRingSpawned = true;
    }

    public virtual bool CanSpawnRedRings() => true;

    protected virtual int GetPlayerOffset(Server server)
    {
      lock (server.Peers)
        return (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) * 20;
    }

    protected virtual float GetRingTime() => 5f;

    protected abstract int GetRingSpawnCount();
  }
}
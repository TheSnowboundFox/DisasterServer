// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.RMZSlimeSpawner
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using ExeNet;
using System;

#nullable enable
namespace DisasterServer.Entities
{
  internal class RMZSlimeSpawner : Entity
  {
    public int ID = -1;
    public bool HasSlime;
    public RMZSlime Slime;
    public const int SPAWN_INTERVAL = 900;
    private int _timer = 900;
    private Random _rand = new Random();
    private static byte _slimeIds;
    private object _lock = new object();

    public RMZSlimeSpawner(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      lock (this._lock)
        this._timer += this._rand.Next(2, 17) * 60;
      return (TcpPacket) null;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      lock (this._lock)
      {
        if (this.HasSlime || this._timer-- > 0)
          return (UdpPacket) null;
        Map map1 = map;
        Server server1 = server;
        RMZSlime entity = new RMZSlime();
        entity.X = this.X;
        entity.Y = this.Y;
        entity.SpawnX = this.X;
        entity.SpawnY = this.Y;
        entity.ID = RMZSlimeSpawner._slimeIds++;
        this.Slime = map1.Spawn<RMZSlime>(server1, entity);
        this.HasSlime = true;
        this.ID = (int) this.Slime.ID;
      }
      return (UdpPacket) null;
    }

    public void KillSlime(Server server, Map map, Peer killer, bool isProjectile)
    {
      lock (this._lock)
      {
        if (!this.HasSlime)
          return;
        switch (this.Slime.State)
        {
          case 2:
          case 3:
            if (!isProjectile)
            {
              SharedServerSession session = server.GetSession(killer.ID);
              server.TCPSend((TcpSession) session, new TcpPacket(PacketType.SERVER_RMZSLIME_RINGBONUS, new object[1]
              {
                (object) false
              }));
              break;
            }
            break;
          case 4:
          case 5:
            if (!isProjectile)
            {
              SharedServerSession session = server.GetSession(killer.ID);
              server.TCPSend((TcpSession) session, new TcpPacket(PacketType.SERVER_RMZSLIME_RINGBONUS, new object[1]
              {
                (object) true
              }));
              break;
            }
            break;
        }
        map.Destroy(server, (Entity) this.Slime);
        this.HasSlime = false;
        this._timer = 900 + this._rand.Next(2, 17) * 60;
        this.ID = -1;
      }
    }
  }
}

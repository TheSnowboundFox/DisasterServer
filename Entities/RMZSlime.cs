// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.RMZSlime
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System;

#nullable enable
namespace DisasterServer.Entities
{
  internal class RMZSlime : Entity
  {
    public byte State;
    public byte ID;
    public int SpawnX;
    public int SpawnY;
    private float _x;
    private bool _state;
    private byte _ring;
    private Random _rand = new Random();

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      int num = this._rand.Next(100);
      this._ring = num < 0 || num >= 50 ? (num < 40 || num >= 90 ? (byte) 2 : (byte) 1) : (byte) 0;
      switch (this._ring)
      {
        case 0:
          this.State = (byte) 0;
          break;
        case 1:
          this.State = (byte) 2;
          break;
        case 2:
          this.State = (byte) 4;
          break;
      }
      return new TcpPacket(PacketType.SERVER_RMZSLIME_STATE, new object[5]
      {
        (object) (byte) 0,
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) this.State
      });
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_RMZSLIME_STATE, new object[2]
      {
        (object) (byte) 2,
        (object) this.ID
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      this.UpdateState();
      return new UdpPacket(PacketType.SERVER_RMZSLIME_STATE, new object[5]
      {
        (object) (byte) 1,
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) this.State
      });
    }

    private void UpdateState()
    {
      if (this._state)
        --this._x;
      else
        ++this._x;
      this.X = (int) (ushort) ((double) this.SpawnX + (double) this._x);
      if ((double) this._x > 100.0)
      {
        switch (this._ring)
        {
          case 0:
            this.State = (byte) 1;
            break;
          case 1:
            this.State = (byte) 3;
            break;
          case 2:
            this.State = (byte) 5;
            break;
        }
        this._state = true;
      }
      else
      {
        if ((double) this._x >= -100.0)
          return;
        switch (this._ring)
        {
          case 0:
            this.State = (byte) 0;
            break;
          case 1:
            this.State = (byte) 2;
            break;
          case 2:
            this.State = (byte) 4;
            break;
        }
        this._state = false;
      }
    }
  }
}

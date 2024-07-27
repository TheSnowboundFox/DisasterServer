// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.PFLift
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class PFLift : Entity
  {
    public byte ID;
    public ushort ActivatorID;
    private bool _activated;
    private float _y;
    private float _startY;
    private float _endY;
    private int _timer;
    private float _speed;

    public PFLift(byte id, float starty, float endY)
    {
      this.ID = id;
      this._startY = starty;
      this._endY = endY;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (!this._activated)
      {
        if (this._timer > 0)
        {
          --this._timer;
          if (this._timer == 0)
            server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, new object[3]
            {
              (object) (byte) 3,
              (object) this.ID,
              (object) (ushort) this._startY
            }));
        }
        return (UdpPacket) null;
      }
      if ((double) this._y > (double) this._endY)
      {
        if ((double) this._speed < 7.0)
          this._speed += 0.052f;
        this._y -= this._speed;
      }
      else
      {
        server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, new object[4]
        {
          (object) (byte) 2,
          (object) this.ID,
          (object) this.ActivatorID,
          (object) (ushort) this._y
        }));
        this._timer = 90;
        this._activated = false;
        this.ActivatorID = (ushort) 0;
      }
      return new UdpPacket(PacketType.SERVER_PFLIFT_STATE, new object[4]
      {
        (object) (byte) 1,
        (object) this.ID,
        (object) this.ActivatorID,
        (object) (ushort) this._y
      });
    }

    public void Activate(Server server, ushort id)
    {
      if (this._activated || this._timer > 0)
        return;
      this.ActivatorID = id;
      this._timer = 0;
      this._speed = 0.0f;
      this._y = this._startY;
      this._activated = true;
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_PFLIFT_STATE, new object[3]
      {
        (object) (byte) 0,
        (object) this.ID,
        (object) this.ActivatorID
      }));
    }
  }
}

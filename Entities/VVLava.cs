// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.VVLava
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System;

#nullable enable
namespace DisasterServer.Entities
{
  public class VVLava : Entity
  {
    public float StartY;
    public float TravelDistance;
    public byte ID;
    private byte _state;
    private int _timer = 1200;
    private float _y;
    private float _accel;
    private static Random _rand = new Random();

    public VVLava(byte id, float startY, float dist)
    {
      this.ID = id;
      this.StartY = startY;
      this._y = startY;
      this.TravelDistance = dist;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this._timer += VVLava._rand.Next(2, 5) * 60;
      return (TcpPacket) null;
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      switch (this._state)
      {
        case 0:
          this._y = this.StartY + MathF.Sin((float) this._timer / 25f) * 6f;
          if (this._timer-- <= 0)
          {
            this._state = (byte) 1;
            break;
          }
          break;
        case 1:
          if ((double) this._y < (double) this.StartY + 20.0)
          {
            this._y += 0.15f;
            break;
          }
          this._state = (byte) 2;
          break;
        case 2:
          if ((double) this._y > (double) this.StartY - (double) this.TravelDistance)
          {
            if ((double) this._accel < 5.0)
              this._accel += 0.08f;
            this._y -= this._accel;
            break;
          }
          this._state = (byte) 3;
          this._timer = 300;
          this._accel = 0.0f;
          break;
        case 3:
          if (this._timer-- <= 0)
            this._state = (byte) 4;
          this._y = (float) ((double) this.StartY - (double) this.TravelDistance + (double) MathF.Sin((float) this._timer / 25f) * 6.0);
          break;
        case 4:
          if ((double) this.StartY > (double) this._y)
          {
            if ((double) this._accel < 5.0)
              this._accel += 0.08f;
            this._y += this._accel;
            break;
          }
          this._state = (byte) 0;
          this._timer = 1200;
          this._accel = 0.0f;
          break;
      }
      return new UdpPacket(PacketType.SERVER_VVLCOLUMN_STATE, new object[3]
      {
        (object) this.ID,
        (object) this._state,
        (object) this._y
      });
    }
  }
}

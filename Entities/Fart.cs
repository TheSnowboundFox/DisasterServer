// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.Fart
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
  public class Fart : Entity
  {
    private float _xspd;
    private float _x;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this.X = 1616;
      this.Y = 2608;
      this._x = (float) this.X;
      return (TcpPacket) null;
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      this._x += this._xspd;
      this._x = Math.Clamp(this._x, 1282f, 2944f);
      this._xspd -= MathF.Min(MathF.Abs(this._xspd), 3f / 16f) * (float) MathF.Sign(this._xspd);
      return new UdpPacket(PacketType.SERVER_FART_STATE, new object[2]
      {
        (object) (ushort) this._x,
        (object) (ushort) this.Y
      });
    }

    public void Push(sbyte force) => this._xspd = (float) force;
  }
}

// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.YCRSmokeController
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.Entities
{
  internal class YCRSmokeController : Entity
  {
    public int _timer;
    public bool _activated;
    public byte _id;
    private Random _rand = new Random();

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer > 360)
      {
        this._timer = 0;
        this._activated = !this._activated;
        this._id = !this._activated ? (byte) 0 : (byte) this._rand.Next(7);
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 2);
        interpolatedStringHandler.AppendFormatted<byte>(this._id);
        interpolatedStringHandler.AppendLiteral(": state ");
        interpolatedStringHandler.AppendFormatted<bool>(this._activated);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        TcpPacket packet = new TcpPacket(PacketType.SERVER_YCRSMOKE_STATE);
        packet.Write(this._activated);
        packet.Write(this._id);
        server.TCPMulticast(packet);
      }
      if (this._activated && this._timer == 60)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_YCRSMOKE_READY);
        packet.Write(this._id);
        server.TCPMulticast(packet);
      }
      ++this._timer;
      return (UdpPacket) null;
    }
  }
}

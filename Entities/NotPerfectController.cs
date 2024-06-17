// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.NotPerfectController
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
  internal class NotPerfectController : Entity
  {
    private byte _stage;
    private int _timer;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (map.BigRingSpawned)
      {
        if (this._timer == 120)
        {
          TcpPacket packet = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
          packet.Write(false);
          packet.Write((byte) 0);
          packet.Write((byte) 0);
          server.TCPMulticast(packet);
        }
        if (this._timer >= 300)
        {
          ++this._stage;
          this._timer = 0;
          TcpPacket packet = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
          packet.Write(true);
          packet.Write((byte) ((uint) this._stage % 4U));
          packet.Write((byte) (Math.Max((int) this._stage - 1, 0) % 4));
          server.TCPMulticast(packet);
        }
      }
      else
      {
        if (this._timer == 900)
        {
          TcpPacket packet = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
          packet.Write(false);
          packet.Write((byte) 0);
          packet.Write((byte) 0);
          server.TCPMulticast(packet);
        }
        if (this._timer >= 1200)
        {
          ++this._stage;
          this._timer = 0;
          TcpPacket packet = new TcpPacket(PacketType.SERVER_NPCONTROLLER_STATE);
          packet.Write(true);
          packet.Write((byte) ((uint) this._stage % 4U));
          packet.Write((byte) (Math.Max((int) this._stage - 1, 0) % 4));
          server.TCPMulticast(packet);
        }
      }
      ++this._timer;
      return (UdpPacket) null;
    }
  }
}

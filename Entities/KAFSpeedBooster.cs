// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.KAFSpeedBooster
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class KAFSpeedBooster : Entity
  {
    public byte ID;
    private int _timer;
    private bool _activated;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE, new object[2]
      {
        (object) (byte) 0,
        (object) this.ID
      });
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer > 0)
        --this._timer;
      else if (this._timer == 0)
      {
        this._activated = false;
        TcpPacket packet = new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE);
        packet.Write((byte) 1);
        packet.Write(this.ID);
        server.TCPMulticast(packet);
        this._timer = -1;
      }
      return (UdpPacket) null;
    }

    public void Activate(Server server, ushort nid, bool isProjectile)
    {
      if (this._activated)
        return;
      TcpPacket packet = new TcpPacket(PacketType.SERVER_KAFMONITOR_STATE);
      packet.Write((byte) 2);
      packet.Write(this.ID);
      packet.Write(isProjectile ? 0 : (int) nid);
      server.TCPMulticast(packet);
      this._activated = true;
      this._timer = 1800;
    }
  }
}

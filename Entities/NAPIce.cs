// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.NAPIce
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public class NAPIce : Entity
  {
    public byte ID;
    private bool _activated = true;
    private int _timer;

    public NAPIce(byte id) => this.ID = id;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (!this._activated)
        return (UdpPacket) null;
      if (this._timer-- <= 0)
      {
        this._activated = false;
        this._timer = 900;
        server.TCPMulticast(new TcpPacket(PacketType.SERVER_NAPICE_STATE, new object[2]
        {
          (object) (byte) 1,
          (object) this.ID
        }));
      }
      return (UdpPacket) null;
    }

    public void Activate(Server server)
    {
      if (this._activated)
        return;
      this._timer = 900;
      this._activated = true;
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_NAPICE_STATE, new object[2]
      {
        (object) (byte) 0,
        (object) this.ID
      }));
    }
  }
}

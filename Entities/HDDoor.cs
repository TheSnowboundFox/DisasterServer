// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.HDDoor
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public class HDDoor : Entity
  {
    public bool _state;
    private int _timer;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer > 0)
      {
        --this._timer;
        if (this._timer == 0)
          server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, new object[2]
          {
            (object) (byte) 1,
            (object) true
          }));
      }
      return (UdpPacket) null;
    }

    public void Toggle(Server server)
    {
      if (this._timer > 0)
        return;
      this._state = !this._state;
      this._timer = 600;
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, new object[2]
      {
        (object) (byte) 0,
        (object) this._state
      }));
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_HDDOOR_STATE, new object[2]
      {
        (object) (byte) 1,
        (object) false
      }));
    }
  }
}

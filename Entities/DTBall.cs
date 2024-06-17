// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.DTBall
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public class DTBall : Entity
  {
    private double _state;
    private bool _side = true;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._side)
      {
        this._state += 0.014999999664723873;
        if (this._state >= 1.0)
          this._side = false;
      }
      else
      {
        this._state -= 0.014999999664723873;
        if (this._state <= -1.0)
          this._side = true;
      }
      return new UdpPacket(PacketType.SERVER_DTBALL_STATE, new object[1]
      {
        (object) (float) this._state
      });
    }
  }
}

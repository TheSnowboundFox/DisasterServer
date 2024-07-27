// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.MovingSpikeController
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class MovingSpikeController : Entity
  {
    private int _timer = 120;
    private int _frame;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer-- <= 0)
      {
        ++this._frame;
        if (this._frame > 5)
          this._frame = 0;
        this._timer = this._frame == 0 || this._frame == 2 ? 120 : 0;
        TcpPacket packet = new TcpPacket(PacketType.SERVER_MOVINGSPIKE_STATE, new object[1]
        {
          (object) (byte) this._frame
        });
        server.TCPMulticast(packet);
      }
      return (UdpPacket) null;
    }
  }
}

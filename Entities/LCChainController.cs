// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.LCChainController
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class LCChainController : Entity
  {
    private int _timer;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer == 480)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
        packet.Write(0);
        server.TCPMulticast(packet);
      }
      if (this._timer == 600)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
        packet.Write(1);
        server.TCPMulticast(packet);
      }
      if (this._timer >= 720)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_LCCHAIN_STATE);
        packet.Write(2);
        server.TCPMulticast(packet);
        this._timer = 0;
      }
      ++this._timer;
      return (UdpPacket) null;
    }
  }
}

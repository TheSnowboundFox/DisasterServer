// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.BlackRing
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public class BlackRing : Entity
  {
    public ushort ID = 1;

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_BRING_STATE, new object[2]
      {
        (object) (byte) 1,
        (object) this.ID
      });
    }

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this.ID = map.BRingsIDs++;
      return new TcpPacket(PacketType.SERVER_BRING_STATE, new object[2]
      {
        (object) (byte) 0,
        (object) this.ID
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket) null;
  }
}

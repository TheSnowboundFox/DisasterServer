// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.EggmanTracker
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class EggmanTracker : Entity
  {
    public static byte TrackerIDs;
    public ushort ActivatorID;
    public byte ID;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this.ID = EggmanTracker.TrackerIDs++;
      return new TcpPacket(PacketType.SERVER_ETRACKER_STATE, new object[4]
      {
        (object) (byte) 0,
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_ETRACKER_STATE, new object[3]
      {
        (object) (byte) 1,
        (object) this.ID,
        (object) this.ActivatorID
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket) null;
  }
}

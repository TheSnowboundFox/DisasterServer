// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.Ring
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System;

#nullable enable
namespace DisasterServer.Entities
{
  public class Ring : Entity
  {
    public ushort ID = 1;
    public byte IID;
    public bool IsRedRing;
    private static readonly Random _rand = new Random();

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this.ID = map.RingIDs++;
      this.IsRedRing = map.CanSpawnRedRings() && Ring._rand.Next(100) <= 10;
      return new TcpPacket(PacketType.SERVER_RING_STATE, new object[4]
      {
        (object) (byte) 0,
        (object) this.IID,
        (object) this.ID,
        (object) this.IsRedRing
      });
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      map.FreeRingID(this.IID);
      return new TcpPacket(PacketType.SERVER_RING_STATE, new object[3]
      {
        (object) (byte) 1,
        (object) this.IID,
        (object) this.ID
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket) null;
  }
}

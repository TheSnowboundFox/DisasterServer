// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.VVVase
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
  public class VVVase : Entity
  {
    public byte ID;
    public byte Type = (byte) VVVase._rand.Next(0, 4);
    public ushort DestroyerID;
    private static readonly Random _rand = new Random();

    public VVVase(byte id) => this.ID = id;

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_VVVASE_STATE, new object[3]
      {
        (object) this.ID,
        (object) this.Type,
        (object) this.DestroyerID
      });
    }

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket) null;
  }
}

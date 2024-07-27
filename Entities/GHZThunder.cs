// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.GHZThunder
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
  public class GHZThunder : Entity
  {
    private int _timer;
    private Random _rand = new Random();

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this._timer = 60 * this._rand.Next(15, 20);
      return (TcpPacket) null;
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer == 120)
      {
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GHZTHUNDER_STATE, new object[1]
        {
          (object) (byte) 0
        });
        server.TCPMulticast(packet);
      }
      if (this._timer <= 0)
      {
        this._timer = 60 * this._rand.Next(15, 20);
        TcpPacket packet = new TcpPacket(PacketType.SERVER_GHZTHUNDER_STATE, new object[1]
        {
          (object) (byte) 1
        });
        server.TCPMulticast(packet);
      }
      --this._timer;
      return (UdpPacket) null;
    }
  }
}

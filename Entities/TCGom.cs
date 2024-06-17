// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.TCGom
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
  internal class TCGom : Entity
  {
    private int _timer = 240;
    private int _id;
    private bool _state;
    private Random _rand = new Random();

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer-- <= 0)
      {
        this._timer = 240;
        this._state = !this._state;
        if (this._state)
          this._id = this._rand.Next(7);
        server.TCPMulticast(new TcpPacket(PacketType.SERVER_TCGOM_STATE, new object[2]
        {
          (object) (byte) this._id,
          (object) this._state
        }));
      }
      return (UdpPacket) null;
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.Act9Wall
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class Act9Wall : Entity
  {
    private byte _id;
    private ushort _tx;
    private ushort _ty;
    private int _startTime;

    public Act9Wall(byte id, ushort tx, ushort ty)
    {
      this._id = id;
      this._tx = tx;
      this._ty = ty;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      this._startTime = map.Timer;
      return (TcpPacket) null;
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      this.X = (int) ((double) this._tx * ((double) (this._startTime - map.Timer) / (double) this._startTime));
      this.Y = (int) ((double) this._ty * ((double) (this._startTime - map.Timer) / (double) this._startTime));
      return new UdpPacket(PacketType.SERVER_ACT9WALL_STATE, new object[3]
      {
        (object) this._id,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }
  }
}

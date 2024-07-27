using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class TailsProjectile : Entity
  {
    public ushort OwnerID;
    public sbyte Direction;
    public bool IsExe;
    public byte Charge;
    public byte Damage;
    private int _timer = 300;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      UdpPacket packet = new UdpPacket(PacketType.SERVER_TPROJECTILE_STATE, new object[]
      {
        (object) (byte) 1,
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) this.OwnerID,
        (object) this.Direction,
        (object) this.Damage,
        (object) this.IsExe,
        (object) this.Charge
      });
      server.UDPMulticast(ref game.IPEndPoints, packet);
      return new TcpPacket(PacketType.SERVER_TPROJECTILE_STATE, new object[]
      {
        (object) (byte) 0,
        (object) (ushort) this.X,
        (object) (ushort) this.Y,
        (object) this.OwnerID,
        (object) this.Direction,
        (object) this.Damage,
        (object) this.IsExe,
        (object) this.Charge
      });
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_TPROJECTILE_STATE, new object[1]
      {
        (object) (byte) 2
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      this.X += (int) this.Direction * 12;
      if (this.X <= 0 || this._timer-- <= 0)
      {
        map.Destroy(server, (Entity) this);
        return (UdpPacket) null;
      }
      return new UdpPacket(PacketType.SERVER_TPROJECTILE_STATE, new object[3]
      {
        (object) (byte) 1,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }
  }
}

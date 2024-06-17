using System;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

namespace DisasterServer.Entities
{
    public class CreamRing : Ring
    {
        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            this.IID = byte.MaxValue;
            this.ID = map.RingIDs++;
            return new TcpPacket(PacketType.SERVER_RING_STATE, new object[6]
            {
                (object) (byte) 2,
                (object) (ushort) this.X,
                (object) (ushort) this.Y,
                (object) this.IID,
                (object) this.ID,
                (object) this.IsRedRing
            });
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket(PacketType.SERVER_RING_STATE, new object[3]
            {
                (object) (byte) 1,
                (object) this.IID,
                (object) this.ID
            });
        }

        public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket)null;
    }
}

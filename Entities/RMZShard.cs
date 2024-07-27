// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.RMZShard
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public class RMZShard : Entity
  {
    private static byte GID;
    public byte ID = RMZShard.GID++;
    private bool _isSpawned;

    public RMZShard(int x, int y, bool spawned = false)
    {
      this.X = x;
      this.Y = y;
      this._isSpawned = spawned;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_RMZSHARD_STATE, new object[4]
      {
        (object) (byte) (this._isSpawned ? 1 : 0),
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map) => (UdpPacket) null;
  }
}

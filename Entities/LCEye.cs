// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.LCEye
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  internal class LCEye : Entity
  {
    public byte ID;
    public ushort UseID;
    public bool Used;
    public int Charge = 100;
    public byte Target;
    private int _cooldown;
    private int _timer;

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._cooldown > 0)
      {
        --this._cooldown;
        return (UdpPacket) null;
      }
      if (this._timer++ >= 60)
      {
        this._timer = 0;
        if (this.Used && this.Charge > 0)
        {
          this.Charge -= 20;
          if (this.Charge < 20)
          {
            this._cooldown = 120;
            this.Used = false;
          }
          this.SendState(server);
        }
        else if (!this.Used && this.Charge < 100)
        {
          this.Charge += 10;
          this.SendState(server);
        }
      }
      return (UdpPacket) null;
    }

    public void SendState(Server server)
    {
      TcpPacket packet = new TcpPacket(PacketType.SERVER_LCEYE_STATE);
      packet.Write(this.ID);
      packet.Write(this.Used);
      packet.Write(this.UseID);
      packet.Write(this.Target);
      packet.Write((byte) this.Charge);
      server.TCPMulticast(packet);
    }
  }
}

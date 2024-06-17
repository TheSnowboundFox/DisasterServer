// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.DTAss
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System.Collections.Generic;

#nullable enable
namespace DisasterServer.Entities
{
  public class DTAss : Entity
  {
    public static byte SID;
    public byte ID;
    private bool _state;
    private double _accel;
    private double _y;
    private int _sY;
    private int _timer = -60;

    public DTAss(int x, int y)
    {
      this.X = x;
      this.Y = y;
      this._y = (double) y;
      this._sY = y;
      this.ID = DTAss.SID++;
    }

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Spawn(Server server, Game game, Map map)
    {
      return new TcpPacket(PacketType.SERVER_DTASS_STATE, new object[4]
      {
        (object) (byte) 0,
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (this._timer == 0)
        server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, new object[4]
        {
          (object) (byte) 0,
          (object) this.ID,
          (object) (ushort) this.X,
          (object) (ushort) this.Y
        }));
      if (this._timer > -60)
        --this._timer;
      if (this._timer <= -60 && !this._state)
      {
        lock (server.Peers)
        {
          foreach (KeyValuePair<ushort, Peer> peer in server.Peers)
          {
            if (!peer.Value.Waiting && !peer.Value.Player.Invisible)
            {
              float num = peer.Value.Player.Y - (float) this.Y;
              if ((double) num > 0.0 && (double) num <= 336.0 && (double) peer.Value.Player.X >= (double) this.X && (double) peer.Value.Player.X <= (double) (this.X + 80))
              {
                server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, new object[2]
                {
                  (object) (byte) 2,
                  (object) this.ID
                }));
                this._state = true;
                break;
              }
            }
          }
        }
      }
      if (!this._state)
        return (UdpPacket) null;
      this._accel += 0.164;
      this._y += this._accel;
      this.Y = (int) this._y;
      return new UdpPacket(PacketType.SERVER_DTASS_STATE, new object[3]
      {
        (object) this.ID,
        (object) (ushort) this.X,
        (object) (ushort) this.Y
      });
    }

    public void Dectivate(Server server)
    {
      this._state = false;
      this._y = (double) this._sY;
      this.Y = this._sY;
      this._timer = 1500;
      this._accel = 0.0;
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_DTASS_STATE, new object[2]
      {
        (object) (byte) 1,
        (object) this.ID
      }));
    }
  }
}

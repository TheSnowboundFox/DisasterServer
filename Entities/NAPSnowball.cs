// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.NAPSnowball
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;
using System;
using System.Runtime.CompilerServices;

#nullable enable
namespace DisasterServer.Entities
{
  public class NAPSnowball : Entity
  {
    public byte ID;
    private bool _active;
    private double _frame;
    private byte _stage;
    private sbyte _dir;
    private double _stateProg;
    private double _accel;
    private float[] _waypoints;
    private float[] _waypointsSpeeds;
    private const int NUM_FRAMES = 32;
    private const int ROLL_START = 16;

    public NAPSnowball(byte nid, byte waypointCount, sbyte dir)
    {
      this._accel = 0.0;
      this._active = false;
      this._frame = 0.0;
      this._stage = (byte) 0;
      this._stateProg = 0.0;
      this._dir = dir;
      this.ID = nid;
      this._waypoints = new float[(int) waypointCount];
      Array.Fill<float>(this._waypoints, 0.05f);
      this._waypointsSpeeds = new float[(int) waypointCount];
      Array.Fill<float>(this._waypointsSpeeds, 0.35f);
    }

    public override TcpPacket? Spawn(Server server, Game game, Map map) => (TcpPacket) null;

    public override TcpPacket? Destroy(Server server, Game game, Map map) => (TcpPacket) null;

    public override UdpPacket? Tick(Server server, Game game, Map map)
    {
      if (!this._active)
        return (UdpPacket) null;
      if (this._accel > 1.0)
      {
        this._frame += (double) this._waypointsSpeeds[(int) this._stage];
        if (this._frame >= 32.0)
          this._frame = 16.0;
        this._stateProg += (double) this._waypoints[(int) this._stage];
      }
      else
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
        interpolatedStringHandler.AppendFormatted<double>(this._accel);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        this._accel += 0.016;
        this._frame += this._accel * 0.44999998807907104;
        this._stateProg += this._accel * 0.05000000074505806;
      }
      if (this._stateProg > 1.0)
      {
        this._stateProg = 0.0;
        ++this._stage;
        if ((int) this._stage >= this._waypoints.Length - 1)
        {
          this._active = false;
          this._stage = (byte) 0;
          this._frame = 0.0;
          this._stateProg = 0.0;
          server.TCPMulticast(new TcpPacket(PacketType.SERVER_NAPBALL_STATE, new object[2]
          {
            (object) (byte) 2,
            (object) this.ID
          }));
          return (UdpPacket) null;
        }
      }
      return new UdpPacket(PacketType.SERVER_NAPBALL_STATE, new object[5]
      {
        (object) (byte) 1,
        (object) this.ID,
        (object) this._stage,
        (object) (byte) this._frame,
        (object) this._stateProg
      });
    }

    public void Activate(Server server)
    {
      if (this._active)
        return;
      this._accel = 0.0;
      this._stage = (byte) 0;
      this._frame = 0.0;
      this._stateProg = 0.0;
      this._active = true;
      server.TCPMulticast(new TcpPacket(PacketType.SERVER_NAPBALL_STATE, new object[3]
      {
        (object) (byte) 0,
        (object) this.ID,
        (object) this._dir
      }));
    }

    public void SetWaypointMoveSpeed(byte index, float speed)
    {
      this._waypoints[(int) index] = speed;
    }

    public void SetWaypointAnimSpeed(byte index, float speed)
    {
      this._waypointsSpeeds[(int) index] = speed;
    }
  }
}

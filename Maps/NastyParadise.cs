// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.NastyParadise
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Entities;
using DisasterServer.Session;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace DisasterServer.Maps
{
  public class NastyParadise : Map
  {
    private int _timer;

    public override void Init(Server server)
    {
      for (byte id = 0; id < (byte) 10; ++id)
        this.Spawn<NAPIce>(server, new NAPIce(id));
      NAPSnowball napSnowball1 = this.Spawn<NAPSnowball>(server, new NAPSnowball((byte) 0, (byte) 10, (sbyte) 1));
      for (byte index = 0; index < (byte) 4; ++index)
      {
        napSnowball1.SetWaypointMoveSpeed((byte) (5U + (uint) index), (float) (0.05000000074505806 + 0.05000000074505806 * ((double) index / 4.0)));
        napSnowball1.SetWaypointAnimSpeed((byte) (5U + (uint) index), (float) (0.34999999403953552 + 0.25 * ((double) index / 4.0)));
      }
      NAPSnowball napSnowball2 = this.Spawn<NAPSnowball>(server, new NAPSnowball((byte) 1, (byte) 8, (sbyte) -1));
      for (byte index = 0; index < (byte) 5; ++index)
      {
        napSnowball2.SetWaypointMoveSpeed((byte) (2U + (uint) index), (float) (0.05000000074505806 + 0.05000000074505806 * ((double) index / 5.0)));
        napSnowball2.SetWaypointAnimSpeed((byte) (2U + (uint) index), (float) (0.34999999403953552 + 0.25 * ((double) index / 5.0)));
      }
      NAPSnowball napSnowball3 = this.Spawn<NAPSnowball>(server, new NAPSnowball((byte) 2, (byte) 11, (sbyte) 1));
      for (byte index = 0; index < (byte) 5; ++index)
      {
        napSnowball3.SetWaypointMoveSpeed((byte) (5U + (uint) index), (float) (0.05000000074505806 + 0.05000000074505806 * ((double) index / 5.0)));
        napSnowball3.SetWaypointAnimSpeed((byte) (5U + (uint) index), (float) (0.34999999403953552 + 0.25 * ((double) index / 5.0)));
      }
      NAPSnowball napSnowball4 = this.Spawn<NAPSnowball>(server, new NAPSnowball((byte) 3, (byte) 9, (sbyte) 1));
      for (byte index = 0; index < (byte) 2; ++index)
      {
        napSnowball4.SetWaypointMoveSpeed((byte) (6U + (uint) index), (float) (0.05000000074505806 + 0.05000000074505806 * ((double) index / 2.0)));
        napSnowball4.SetWaypointAnimSpeed((byte) (6U + (uint) index), (float) (0.34999999403953552 + 0.25 * ((double) index / 2.0)));
      }
      this.Spawn<NAPSnowball>(server, new NAPSnowball((byte) 4, (byte) 5, (sbyte) -1));
      this.SetTime(server, 155);
      base.Init(server);
    }

    public override void Tick(Server server)
    {
      ++this._timer;
      if (this._timer >= 1200)
      {
        this._timer = 0;
        foreach (NAPSnowball napSnowball in this.FindOfType<NAPSnowball>())
          napSnowball.Activate(server);
      }
      base.Tick(server);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 85)
      {
        byte id = reader.ReadByte();
        NAPIce[] ofType = this.FindOfType<NAPIce>();
        if (ofType != null)
          ((IEnumerable<NAPIce>) ofType).Where<NAPIce>((Func<NAPIce, bool>) (e => (int) e.ID == (int) id)).FirstOrDefault<NAPIce>()?.Activate(server);
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetRingSpawnCount() => 26;
  }
}

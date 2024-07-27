// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.Act9
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Entities;
using DisasterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace DisasterServer.Maps
{
  public class Act9 : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 130);
      this.Spawn<Act9Wall>(server, new Act9Wall((byte) 0, (ushort) 0, (ushort) 1025));
      this.Spawn<Act9Wall>(server, new Act9Wall((byte) 1, (ushort) 1663, (ushort) 0));
      this.Spawn<Act9Wall>(server, new Act9Wall((byte) 2, (ushort) 1663, (ushort) 0));
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    protected override int GetPlayerOffset(Server server)
    {
      lock (server.Peers)
        return (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) * 10;
    }

    protected override int GetRingSpawnCount() => 38;

    protected override float GetRingTime() => 3f;

    public override bool CanSpawnRedRings() => true;
  }
}

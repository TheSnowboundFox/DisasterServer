// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.MajinForest
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace DisasterServer.Maps
{
  public class MajinForest : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 155);
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    protected override int GetPlayerOffset(Server server)
    {
      lock (server.Peers)
        return (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) * 10;
    }

    protected override int GetRingSpawnCount() => 20;
  }
}

// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.PricelessFreedom
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Data;
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
  public class PricelessFreedom : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 155);
      this.Spawn<PFLift>(server, new PFLift((byte) 0, 1669f, 1016f));
      this.Spawn<PFLift>(server, new PFLift((byte) 1, 1069f, 704f));
      this.Spawn<PFLift>(server, new PFLift((byte) 2, 829f, 400f));
      this.Spawn<PFLift>(server, new PFLift((byte) 3, 1070f, 544f));
      for (int index = 0; index < 29; ++index)
        this.Spawn<BlackRing>(server);
      base.Init(server);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 87)
      {
        byte id = reader.ReadByte();
        PFLift[] ofType = this.FindOfType<PFLift>();
        if (ofType != null)
          ((IEnumerable<PFLift>) ofType).Where<PFLift>((Func<PFLift, bool>) (e => (int) e.ID == (int) id)).FirstOrDefault<PFLift>()?.Activate(server, session.ID);
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetPlayerOffset(Server server)
    {
      lock (server.Peers)
        return (server.Peers.Count<KeyValuePair<ushort, Peer>>((Func<KeyValuePair<ushort, Peer>, bool>) (e => !e.Value.Waiting)) - 1) * 10;
    }

    protected override int GetRingSpawnCount() => 38;
  }
}

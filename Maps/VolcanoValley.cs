// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.VolcanoValley
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
  public class VolcanoValley : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 180);
      this.Spawn<VVLava>(server, new VVLava((byte) 0, 736f, 130f));
      this.Spawn<VVLava>(server, new VVLava((byte) 1, 1388f, 130f));
      this.Spawn<VVLava>(server, new VVLava((byte) 2, 1524f, 130f));
      this.Spawn<VVLava>(server, new VVLava((byte) 3, 1084f, 130f));
      for (byte id = 0; id < (byte) 14; ++id)
        this.Spawn<VVVase>(server, new VVVase(id));
      base.Init(server);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 88)
      {
        byte nid = reader.ReadByte();
        VVVase[] ofType = this.FindOfType<VVVase>();
        if (ofType == null)
          return;
        VVVase vvVase = ((IEnumerable<VVVase>) ofType).Where<VVVase>((Func<VVVase, bool>) (e => (int) e.ID == (int) nid)).FirstOrDefault<VVVase>();
        if (vvVase != null)
        {
          vvVase.DestroyerID = session.ID;
          this.Destroy(server, (Entity) vvVase);
        }
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetRingSpawnCount() => 27;
  }
}

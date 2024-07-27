// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.DarkTower
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
  public class DarkTower : Map
  {
    public override void Init(Server server)
    {
      this.Spawn<DTTailsDoll>(server);
      this.Spawn<DTBall>(server);
      this.Spawn<DTAss>(server, new DTAss(1744, 224));
      this.Spawn<DTAss>(server, new DTAss(1840, 224));
      this.Spawn<DTAss>(server, new DTAss(1936, 224));
      this.Spawn<DTAss>(server, new DTAss(2032, 224));
      this.Spawn<DTAss>(server, new DTAss(2128, 224));
      this.Spawn<DTAss>(server, new DTAss(1824, 784));
      this.Spawn<DTAss>(server, new DTAss(1920, 784));
      this.Spawn<DTAss>(server, new DTAss(2016, 784));
      this.Spawn<DTAss>(server, new DTAss(2112, 784));
      this.Spawn<DTAss>(server, new DTAss(2208, 784));
      this.Spawn<DTAss>(server, new DTAss(2464, 1384));
      this.Spawn<DTAss>(server, new DTAss(2592, 1384));
      this.Spawn<DTAss>(server, new DTAss(3032, 64));
      this.Spawn<DTAss>(server, new DTAss(3088, 64));
      this.SetTime(server, 205);
      base.Init(server);
    }

    protected override int GetRingSpawnCount() => 31;

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 91)
      {
        byte id = reader.ReadByte();
        DTAss[] ofType = this.FindOfType<DTAss>();
        if (ofType != null)
          ((IEnumerable<DTAss>) ofType).FirstOrDefault<DTAss>((Func<DTAss, bool>) (e => (int) e.ID == (int) id))?.Dectivate(server);
      }
      base.PeerTCPMessage(server, session, reader);
    }
  }
}

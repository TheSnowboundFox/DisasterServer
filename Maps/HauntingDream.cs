// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.HauntingDream
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Entities;
using DisasterServer.Session;
using ExeNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable enable
namespace DisasterServer.Maps
{
  public class HauntingDream : Map
  {
    public override void Init(Server server)
    {
      this.Spawn<HDDoor>(server);
      this.SetTime(server, 205);
      base.Init(server);
    }

    protected override int GetRingSpawnCount() => 31;

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 92)
      {
        HDDoor[] ofType = this.FindOfType<HDDoor>();
        if (ofType != null)
          ((IEnumerable<HDDoor>) ofType).FirstOrDefault<HDDoor>()?.Toggle(server);
      }
      base.PeerTCPMessage(server, session, reader);
    }
  }
}

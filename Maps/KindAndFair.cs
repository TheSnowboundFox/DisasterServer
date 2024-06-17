// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.KindAndFair
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Entities;
using DisasterServer.Session;
using ExeNet;
using System.IO;

#nullable enable
namespace DisasterServer.Maps
{
  public class KindAndFair : Map
  {
    public override void Init(Server server)
    {
      for (int index = 0; index < 11; ++index)
        this.Spawn<KAFSpeedBooster>(server, new KAFSpeedBooster()
        {
          ID = (byte) index
        });
      this.SetTime(server, 180);
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 82)
      {
        byte index = reader.ReadByte();
        bool isProjectile = reader.ReadBoolean();
        KAFSpeedBooster[] ofType = this.FindOfType<KAFSpeedBooster>();
        if (ofType == null)
          return;
        if ((int) index < ofType.Length)
        {
          KAFSpeedBooster kafSpeedBooster = ofType[(int) index];
          lock (server.Peers)
            kafSpeedBooster.Activate(server, server.Peers[session.ID].ID, isProjectile);
        }
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetRingSpawnCount() => 31;
  }
}

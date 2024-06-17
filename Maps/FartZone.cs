// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.FartZone
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
  internal class FartZone : Map
  {
    public override void Init(Server server)
    {
      this.SetTime(server, 256);
      this.Spawn<Fart>(server);
      this.Spawn<MovingSpikeController>(server);
      this.Spawn<DTBall>(server);
      for (int index = 0; index < 3; ++index)
        this.Spawn<BlackRing>(server);
      base.Init(server);
    }

    public override void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
    {
      reader.ReadBoolean();
      if (reader.ReadByte() == (byte) 93)
      {
        sbyte force = reader.ReadSByte();
        this.FindOfType<Fart>()?[0].Push(force);
      }
      base.PeerTCPMessage(server, session, reader);
    }

    protected override int GetRingSpawnCount() => 15;

    protected override float GetRingTime() => 1f;
  }
}

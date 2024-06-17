// Decompiled with JetBrains decompiler
// Type: DisasterServer.Maps.NotPerfect
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Entities;
using DisasterServer.Session;

#nullable enable
namespace DisasterServer.Maps
{
  public class NotPerfect : Map
  {
    public override void Init(Server server)
    {
      this.Spawn<NotPerfectController>(server);
      this.SetTime(server, 155);
      base.Init(server);
    }

    public override void Tick(Server server) => base.Tick(server);

    protected override int GetRingSpawnCount() => 59;

    protected override float GetRingTime() => 3f;
  }
}

// Decompiled with JetBrains decompiler
// Type: DisasterServer.Entities.Entity
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\DisasterServerWin\DisasterServer.dll

using DisasterServer.Maps;
using DisasterServer.Session;
using DisasterServer.State;

#nullable enable
namespace DisasterServer.Entities
{
  public abstract class Entity
  {
    public int X;
    public int Y;

    public abstract TcpPacket? Spawn(Server server, Game game, Map map);

    public abstract UdpPacket? Tick(Server server, Game game, Map map);

    public abstract TcpPacket? Destroy(Server server, Game game, Map map);
  }
}

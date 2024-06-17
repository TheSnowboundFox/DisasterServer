// Decompiled with JetBrains decompiler
// Type: DisasterServer.Session.StatServerSession
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using ExeNet;
using System.Net.Sockets;

#nullable enable
namespace DisasterServer.Session
{
  public class StatServerSession : TcpSession
  {
    public StatServerSession(TcpServer server, TcpClient client)
      : base(server, client)
    {
    }

    protected override void OnConnected()
    {
      byte[] data = new byte[64];
      data[0] = (byte) 7;

      this.Send(data);
      base.OnConnected();
    }
  }
}

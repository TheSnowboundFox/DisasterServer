// Decompiled with JetBrains decompiler
// Type: DisasterServer.Session.SharedServer
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using ExeNet;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

#nullable enable
namespace DisasterServer.Session
{
  public class SharedServer : TcpServer
  {
    protected Server _server;

    public SharedServer(Server server, int port)
      : base(IPAddress.Any, port)
    {
      this._server = server;
    }

    protected override void OnReady()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      interpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
      interpolatedStringHandler.AppendLiteral("Server started on TCP port ");
      interpolatedStringHandler.AppendFormatted<int>(this.Port);
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      base.OnReady();
    }

    protected override void OnSocketError(SocketError error)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
      interpolatedStringHandler.AppendLiteral("Caught SocketError: ");
      interpolatedStringHandler.AppendFormatted<SocketError>(error);
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      base.OnSocketError(error);
    }

    protected override void OnError(string message)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      Terminal.Log("Caught Error: " + message);
      base.OnError(message);
    }

    protected override TcpSession CreateSession(TcpClient client)
    {
      return (TcpSession) new SharedServerSession(this._server, client);
    }
  }
}

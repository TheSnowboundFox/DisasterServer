using ExeNet;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

#nullable enable
namespace DisasterServer.Session
{
  public class MulticastServer : UdpServer
  {
    protected Server _server;

    public MulticastServer(Server server, int port)
      : base(port)
    {
      this._server = server;
    }

    protected override void OnReady()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
      interpolatedStringHandler.AppendLiteral("Server started on UDP port ");
      interpolatedStringHandler.AppendFormatted<int>(this.Port);
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      base.OnReady();
    }

    protected override void OnSocketError(IPEndPoint endpoint, SocketError error)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
      interpolatedStringHandler.AppendLiteral("Caught SocketError: ");
      interpolatedStringHandler.AppendFormatted<SocketError>(error);
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      this._server.State.UDPSocketError(endpoint, error);
      base.OnSocketError(endpoint, error);
    }

    protected override void OnError(IPEndPoint? endpoint, string message)
    {
      Terminal.Log("Caught Error: " + message);
      base.OnError(endpoint, message);
    }

    protected override void OnData(IPEndPoint sender, ref byte[] data)
    {
      if (data.Length > 128)
      {
        Terminal.Log("UDP overload (data.Length > 128)");
      }
      else
      {
        this._server.State.PeerUDPMessage(this._server, sender, ref data);
        base.OnData(sender, ref data);
      }
    }
  }
}

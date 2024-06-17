// Decompiled with JetBrains decompiler
// Type: DisasterServer.State.State
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.Session;
using ExeNet;
using System.IO;
using System.Net;
using System.Net.Sockets;

#nullable enable
namespace DisasterServer.State
{
  public abstract class State
  {
    public abstract void PeerJoined(Server server, TcpSession session, Peer peer);

    public abstract void PeerLeft(Server server, TcpSession session, Peer peer);

    public abstract void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader);

    public abstract void PeerUDPMessage(Server server, IPEndPoint IPEndPoint, ref byte[] data);

    public virtual void UDPSocketError(IPEndPoint endpoint, SocketError error)
    {
    }

    public virtual void TCPSocketError(TcpSession session, SocketError error)
    {
    }

    public abstract void Init(Server server);

    public abstract void Tick(Server server);

    public abstract DisasterServer.Session.State AsState();
  }
}

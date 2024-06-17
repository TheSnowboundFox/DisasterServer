// Decompiled with JetBrains decompiler
// Type: DisasterServer.Session.SharedServerSession
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer;
using DisasterServer.Data;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

#nullable enable
namespace DisasterServer.Session
{
  public class SharedServerSession : TcpSession
  {
    private DisasterServer.Session.Server _server;
    private List<byte> _header = new List<byte>();
    private List<byte> _data = new List<byte>();
    private int _length = -1;
    private bool _start;
    private byte[] _headerData = new byte[5]
    {
      (byte) 104,
      (byte) 80,
      (byte) 75,
      (byte) 84,
      (byte) 0
    };

    public SharedServerSession(DisasterServer.Session.Server server, TcpClient client)
      : base((TcpServer) server.SharedServer, client)
    {
      this._server = server;
    }

    protected override void OnConnected()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler1.ToStringAndClear();
      lock (this._server.Peers)
      {
        if (KickList.Check((this.RemoteEndPoint as IPEndPoint).Address.ToString()))
        {
          this._server.DisconnectWithReason((TcpSession) this, "Kicked by server.");
          return;
        }
        if (BanList.Check((this.RemoteEndPoint as IPEndPoint).Address.ToString()))
        {
            this._server.DisconnectWithReason((TcpSession)this, "You were banned from this server.");
            return;
        }
        if (this._server.Peers.Count >= 7)
        {
          this._server.DisconnectWithReason((TcpSession) this, "Server is full. (7/7)");
          return;
        }
        Peer peer = new Peer()
        {
          EndPoint = this.RemoteEndPoint,
          Player = new Player(),
          ID = this.ID,
          Pending = true,
          Waiting = this._server.State.AsState() != 0
        };
        this._server.Peers.Add(this.ID, peer);
        if (!peer.Waiting)
          this._server.State.PeerJoined(this._server, (TcpSession) this, peer);
        DefaultInterpolatedStringHandler interpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 2);
        interpolatedStringHandler2.AppendFormatted<EndPoint>(this.RemoteEndPoint);
        interpolatedStringHandler2.AppendLiteral(" (ID ");
        interpolatedStringHandler2.AppendFormatted<ushort>(this.ID);
        interpolatedStringHandler2.AppendLiteral(") connected.");
        Terminal.Log(interpolatedStringHandler2.ToStringAndClear());
      }
      base.OnConnected();
    }

    protected override void OnDisconnected()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler1 = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler1.ToStringAndClear();
      lock (this._server.Peers)
      {
        Terminal.LogDebug("Disconnect Stage 1");
        Peer peer;
        this._server.Peers.Remove(this.ID, out peer);
        if (peer == null)
          return;
        Terminal.LogDebug("Disconnect Stage 2");
        if (peer.Waiting)
        {
          DefaultInterpolatedStringHandler interpolatedStringHandler2 = new DefaultInterpolatedStringHandler(30, 2);
          interpolatedStringHandler2.AppendFormatted<EndPoint>(peer?.EndPoint);
          interpolatedStringHandler2.AppendLiteral(" (waiting) (ID ");
          interpolatedStringHandler2.AppendFormatted<ushort?>(peer?.ID);
          interpolatedStringHandler2.AppendLiteral(") disconnected.");
          Terminal.Log(interpolatedStringHandler2.ToStringAndClear());
          return;
        }
        Terminal.LogDebug("Disconnect Stage 3");
        this._server.TCPMulticast(new TcpPacket(PacketType.SERVER_PLAYER_LEFT, new object[1]
        {
          (object) peer.ID
        }), new ushort?(this.ID));
        Terminal.LogDebug("Disconnect Stage 4");
        this._server.State.PeerLeft(this._server, (TcpSession) this, peer);
        DefaultInterpolatedStringHandler interpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 2);
        interpolatedStringHandler3.AppendFormatted<EndPoint>(peer?.EndPoint);
        interpolatedStringHandler3.AppendLiteral(" (ID ");
        interpolatedStringHandler3.AppendFormatted<ushort?>(peer?.ID);
        interpolatedStringHandler3.AppendLiteral(") disconnected.");
        Terminal.Log(interpolatedStringHandler3.ToStringAndClear());
      }
      base.OnDisconnected();
    }

    protected override void OnData(byte[] buffer, int length)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      this.ProcessBytes(buffer, length);
      base.OnData(buffer, length);
    }

    protected override void Timeouted()
    {
      this._server.DisconnectWithReason((TcpSession) this, "Connection timeout");
      base.Timeouted();
    }

    private void ProcessBytes(byte[] buffer, int length)
    {
      using (MemoryStream memoryStream = new MemoryStream(buffer, 0, length))
      {
        while (memoryStream.Position < memoryStream.Length)
        {
          byte num = (byte) memoryStream.ReadByte();
          if (this._start)
          {
            this._start = false;
            this._length = (int) num;
            this._data.Clear();
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
            interpolatedStringHandler.AppendLiteral("Packet start ");
            interpolatedStringHandler.AppendFormatted<int>(this._length);
            Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
          }
          else
          {
            this._data.Add(num);
            if (this._data.Count >= this._length && this._length != -1)
            {
              byte[] array = this._data.ToArray();
              using (MemoryStream input = new MemoryStream(array))
              {
                using (BinaryReader reader = new BinaryReader((Stream) input))
                {
                  Terminal.LogDebug("Packet recv " + BitConverter.ToString(array));
                  try
                  {
                    if (array.Length > 256)
                    {
                      Terminal.Log("TCP overload (data.Length > 256)");
                      this._server.DisconnectWithReason((TcpSession) this, "Packet overload > 256");
                    }
                    else
                      this._server.State.PeerTCPMessage(this._server, (TcpSession) this, reader);
                  }
                  catch (Exception ex)
                  {
                    this.OnError(ex.Message);
                  }
                  this._length = -1;
                  this._data.Clear();
                }
              }
            }
          }
          this._header.Add(num);
          if (this._header.Count >= 6)
            this._header.RemoveAt(0);
          if (this._header.SequenceEqual<byte>((IEnumerable<byte>) this._headerData))
            this._start = true;
        }
        if (this._data.Count >= this._length || this._length == -1)
          return;
        Terminal.LogDebug("Packet split, waiting for part to arrive.");
      }
    }

    protected override void OnSocketError(SocketError error)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
      interpolatedStringHandler.AppendLiteral("Caught SocketError: ");
      interpolatedStringHandler.AppendFormatted<SocketError>(error);
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      this._server.State.TCPSocketError((TcpSession) this, error);
      base.OnSocketError(error);
    }

    protected override void OnError(string message)
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      Terminal.Log("Caught Error: " + message);
      base.OnError(message);
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: DisasterServer.Session.Server
// Assembly: DisasterServer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 543F9D20-F969-4EBF-B20C-D2589F4E3C54
// Assembly location: C:\Users\User\Downloads\disasterlauncherwindows\server\DisasterServer.dll

using DisasterServer.Data;
using DisasterServer.State;
using ExeNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace DisasterServer.Session
{
  public class Server
  {
    private const int TCP_PORT = 7606;
    private const int UDP_PORT = 8606;
    public bool IsRunning;
    public DisasterServer.State.State State = (DisasterServer.State.State) new Lobby();
    public Dictionary<ushort, Peer> Peers = new Dictionary<ushort, Peer>();
    public int LastMap = -1;
    public MulticastServer MulticastServer;
    public SharedServer SharedServer;
    private Thread? _thread;
    private int _hbTimer;

    public Server()
    {
      this.MulticastServer = new MulticastServer(this, 8606);
      this.SharedServer = new SharedServer(this, 7606);
    }

    public void StartAsync()
    {
      if (!this.SharedServer.Start())
        throw new Exception("Failed to start SharedServer (TCP)");
      if (!this.MulticastServer.Start())
        throw new Exception("Failed to start MulticastServer (UCP)");
      this.IsRunning = true;
      this._thread = new Thread((ThreadStart) (() =>
      {
        while (this.IsRunning)
        {
          this.DoHeartbeat();
          this.Tick();
          Thread.Sleep(15);
        }
      }));
      this._thread.Priority = ThreadPriority.AboveNormal;
      Thread thread = this._thread;
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
      string stringAndClear = interpolatedStringHandler.ToStringAndClear();
      thread.Name = stringAndClear;
      this._thread.Start();
    }

    public void Tick() => this.State.Tick(this);

    public SharedServerSession? GetSession(ushort id)
    {
      return (SharedServerSession) this.SharedServer.GetSession(id);
    }

    public void TCPSend(TcpSession? session, TcpPacket packet)
    {
      if (session == null)
        return;
      try
      {
        byte[] array = packet.ToArray();
        session.Send(array, packet.Length);
      }
      catch (Exception ex)
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
        interpolatedStringHandler.AppendLiteral("TCPSend() Exception: ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public void TCPMulticast(TcpPacket packet, ushort? except = null)
    {
      try
      {
        byte[] array = packet.ToArray();
        lock (this.Peers)
        {
          foreach (KeyValuePair<ushort, Peer> peer in this.Peers)
          {
            int key = (int) peer.Key;
            ushort? nullable1 = except;
            int? nullable2 = nullable1.HasValue ? new int?((int) nullable1.GetValueOrDefault()) : new int?();
            int valueOrDefault = nullable2.GetValueOrDefault();
            if (!(key == valueOrDefault & nullable2.HasValue))
              this.GetSession(peer.Value.ID)?.Send(array, packet.Length);
          }
        }
      }
      catch (Exception ex)
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
        interpolatedStringHandler.AppendLiteral("TCPMulticast() Exception: ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public void UDPSend(IPEndPoint IPEndPoint, UdpPacket packet)
    {
      try
      {
        byte[] array = packet.ToArray();
        this.MulticastServer.Send(IPEndPoint, ref array, packet.Length);
      }
      catch (InvalidOperationException ex)
      {
      }
      catch (Exception ex)
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(21, 1);
        interpolatedStringHandler.AppendLiteral("UDPSend() Exception: ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public void UDPMulticast(ref List<IPEndPoint> IPEndPoints, UdpPacket packet, IPEndPoint? except = null)
    {
      try
      {
        byte[] array = packet.ToArray();
        lock (IPEndPoints)
        {
          foreach (IPEndPoint endpoint in IPEndPoints)
          {
            if (endpoint != except)
              this.MulticastServer.Send(endpoint, ref array, packet.Length);
          }
        }
      }
      catch (InvalidOperationException ex)
      {
      }
      catch (Exception ex)
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
        interpolatedStringHandler.AppendLiteral("UDPMulticast() Exception: ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public void UDPMulticast(
      ref Dictionary<ushort, IPEndPoint> IPEndPoints,
      UdpPacket packet,
      IPEndPoint? except = null)
    {
      try
      {
        byte[] array = packet.ToArray();
        lock (IPEndPoints)
        {
          foreach (KeyValuePair<ushort, IPEndPoint> keyValuePair in IPEndPoints)
          {
            if (keyValuePair.Value != except)
              this.MulticastServer.Send(keyValuePair.Value, ref array, packet.Length);
          }
        }
      }
      catch (InvalidOperationException ex)
      {
      }
      catch (Exception ex)
      {
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 1);
        interpolatedStringHandler.AppendLiteral("UDPMulticast() Exception: ");
        interpolatedStringHandler.AppendFormatted<Exception>(ex);
        Terminal.Log(interpolatedStringHandler.ToStringAndClear());
      }
    }

    public void Passtrough(BinaryReader reader, TcpSession sender)
    {
      Terminal.LogDebug("Passtrough()");
      long position = reader.BaseStream.Position;
      reader.BaseStream.Seek(0L, SeekOrigin.Begin);
      int num = (int) reader.ReadByte();
      TcpPacket packet = new TcpPacket((PacketType) reader.ReadByte());
      while (reader.BaseStream.Position < reader.BaseStream.Length)
        packet.Write(reader.ReadByte());
      this.TCPMulticast(packet, new ushort?(sender.ID));
      reader.BaseStream.Seek(position, SeekOrigin.Begin);
      Terminal.LogDebug("Passtrough end()");
    }

    public void DisconnectWithReason(TcpSession? session, string reason)
    {
      Task.Run((Action) (() =>
      {
        if (session == null || !session.IsRunning)
          return;
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
        interpolatedStringHandler.AppendLiteral("Disconnecting cuz following balls (");
        interpolatedStringHandler.AppendFormatted<ushort>(session.ID);
        interpolatedStringHandler.AppendLiteral("): ");
        interpolatedStringHandler.AppendFormatted(reason);
        Terminal.LogDebug(interpolatedStringHandler.ToStringAndClear());
        interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
        string stringAndClear = interpolatedStringHandler.ToStringAndClear();
        try
        {
          EndPoint remoteEndPoint = session.RemoteEndPoint;
          ushort id = session.ID;
          lock (this.Peers)
          {
            if (!this.Peers.ContainsKey(id))
            {
              interpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 2);
              interpolatedStringHandler.AppendLiteral("(ID ");
              interpolatedStringHandler.AppendFormatted<ushort>(id);
              interpolatedStringHandler.AppendLiteral(") disconnect: ");
              interpolatedStringHandler.AppendFormatted(reason);
              Terminal.Log(interpolatedStringHandler.ToStringAndClear());
            }
            else
            {
              Peer peer = this.Peers[id];
              interpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 3);
              interpolatedStringHandler.AppendFormatted(peer.Nickname);
              interpolatedStringHandler.AppendLiteral(" (ID ");
              interpolatedStringHandler.AppendFormatted<ushort>(peer.ID);
              interpolatedStringHandler.AppendLiteral(") disconnect: ");
              interpolatedStringHandler.AppendFormatted(reason);
              Terminal.Log(interpolatedStringHandler.ToStringAndClear());
            }
          }
          TcpPacket packet = new TcpPacket(PacketType.SERVER_PLAYER_FORCE_DISCONNECT);
          packet.Write(reason);
          this.TCPSend(session, packet);
          session.Disconnect();
        }
        catch (Exception ex)
        {
          interpolatedStringHandler = new DefaultInterpolatedStringHandler(19, 1);
          interpolatedStringHandler.AppendLiteral("Disconnect failed: ");
          interpolatedStringHandler.AppendFormatted<Exception>(ex);
          Terminal.Log(interpolatedStringHandler.ToStringAndClear());
        }
      }));
    }

    public void SetState<T>() where T : DisasterServer.State.State
    {
      object instance = Activator.CreateInstance(typeof (T));
      if (instance == null)
        return;
      this.State = (DisasterServer.State.State) instance;
      this.State.Init(this);
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
      interpolatedStringHandler.AppendLiteral("Server state is ");
      interpolatedStringHandler.AppendFormatted<DisasterServer.State.State>(this.State);
      interpolatedStringHandler.AppendLiteral(" now");
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
    }

    public void SetState<T>(T value) where T : DisasterServer.State.State
    {
      this.State = (DisasterServer.State.State) value;
      this.State.Init(this);
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(20, 1);
      interpolatedStringHandler.AppendLiteral("Server state is ");
      interpolatedStringHandler.AppendFormatted<DisasterServer.State.State>(this.State);
      interpolatedStringHandler.AppendLiteral(" now");
      Terminal.Log(interpolatedStringHandler.ToStringAndClear());
    }

    private void DoHeartbeat()
    {
      lock (this.Peers)
      {
        if (this.Peers.Count <= 0)
          return;
      }
      if (this._hbTimer++ < 120)
        return;
      this.TCPMulticast(new TcpPacket(PacketType.SERVER_HEARTBEAT));
      Terminal.LogDebug("Server heartbeated.");
      this._hbTimer = 0;
    }
  }
}

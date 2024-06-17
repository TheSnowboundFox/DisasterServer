// Decompiled with JetBrains decompiler
// Type: ExeNet.TcpSession
// Assembly: ExeNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7AC46939-1C6E-4A4A-A8C2-645E334BBB85
// Assembly location: C:\Users\User\Desktop\ExeNet.dll

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

#nullable enable
namespace ExeNet
{
  public class TcpSession : IDisposable
  {
    public ushort ID;
    private byte[] _readBuffer = Array.Empty<byte>();

    public int ReadBufferSize { get; private set; } = 96;

    public bool IsRunning { get; private set; }

    public EndPoint? RemoteEndPoint
    {
      get
      {
        try
        {
          return this.Client == null || this.Client.Client == null || !this.Client.Connected ? (EndPoint) null : this.Client.Client.RemoteEndPoint;
        }
        catch
        {
          return (EndPoint) null;
        }
      }
    }

    protected TcpClient Client { get; private set; }

    protected TcpServer Server { get; private set; }

    public TcpSession(TcpServer server, TcpClient client)
    {
      this.Server = server;
      this.Client = client;
      this.ID = server.RequestID();
      this.Client.SendTimeout = 3000;
      this.Client.ReceiveTimeout = 2000;
      this.Client.NoDelay = true;
    }

    public void Start()
    {
      this._readBuffer = new byte[this.ReadBufferSize];
      this.IsRunning = true;
      this.OnConnected();
      if (!this.IsRunning)
        return;
      this.Client.Client.BeginReceive(this._readBuffer, 0, this._readBuffer.Length, SocketFlags.None, new AsyncCallback(this.DoReceive), (object) null);
    }

    public void Send(byte[] data) => this.Send(data, data.Length);

    public void Send(byte[] data, int length)
    {
      if (!this.IsRunning || !this.Client.Connected)
        return;
      this.Client.Client.BeginSend(data, 0, length, SocketFlags.None, new AsyncCallback(this.DoSend), (object) null);
    }

    public void Disconnect()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(47, 1);
      interpolatedStringHandler.AppendLiteral("[TcpSession.cs] CleanUp call from Disconnect (");
      interpolatedStringHandler.AppendFormatted<bool>(this.IsRunning);
      interpolatedStringHandler.AppendLiteral(")");
      Console.WriteLine(interpolatedStringHandler.ToStringAndClear());
      this.CleanUp();
    }

    private void CleanUp()
    {
      try
      {
        Console.WriteLine("[TcpSession.cs] CleanUp()");
        if (!this.IsRunning)
          return;
        this.IsRunning = false;
        if (this.Client.Connected)
          this.Client.Close();
        Console.WriteLine("[TcpSession.cs] Client is closed");
        this.OnDisconnected();
        lock (this.Server.Sessions)
        {
          if (this.Server.Sessions.Contains(this))
            this.Server.Sessions.Remove(this);
        }
        Console.WriteLine("[TcpSession.cs] Disconnect called and removed from the list.");
      }
      catch (ObjectDisposedException ex)
      {
        Console.WriteLine("[TcpSession.cs] already disposed");
      }
    }

    private void DoSend(IAsyncResult result)
    {
      try
      {
        if (!this.IsRunning)
          return;
        SocketError errorCode;
        int num = this.Client.Client.EndSend(result, out errorCode);
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 1);
        interpolatedStringHandler.AppendLiteral("[TcpSession.cs] DoSend() SocketError Code: ");
        interpolatedStringHandler.AppendFormatted<SocketError>(errorCode);
        Console.WriteLine(interpolatedStringHandler.ToStringAndClear());
        switch (errorCode)
        {
          case SocketError.Success:
            if (num > 0)
              break;
            this.CleanUp();
            break;
          case SocketError.NetworkReset:
          case SocketError.ConnectionAborted:
          case SocketError.ConnectionReset:
          case SocketError.Shutdown:
          case SocketError.ConnectionRefused:
            this.CleanUp();
            break;
          case SocketError.TimedOut:
            break;
          default:
            if (num <= 0)
            {
              this.CleanUp();
              break;
            }
            this.OnSocketError(errorCode);
            break;
        }
      }
      catch (Exception ex)
      {
        this.OnError(ex.Message);
      }
    }

    private void DoReceive(IAsyncResult result)
    {
      try
      {
        if (!this.IsRunning || this.Client == null)
          return;
        SocketError errorCode;
        int length = this.Client.Client.EndReceive(result, out errorCode);
        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
        interpolatedStringHandler.AppendLiteral("[TcpSession.cs] DoReceive() SocketError Code: ");
        interpolatedStringHandler.AppendFormatted<SocketError>(errorCode);
        Console.WriteLine(interpolatedStringHandler.ToStringAndClear());
        switch (errorCode)
        {
          case SocketError.Success:
            if (length <= 0)
            {
              this.CleanUp();
              return;
            }
            goto case SocketError.TimedOut;
          case SocketError.NetworkReset:
          case SocketError.ConnectionAborted:
          case SocketError.ConnectionReset:
          case SocketError.Shutdown:
          case SocketError.ConnectionRefused:
            this.CleanUp();
            return;
          case SocketError.TimedOut:
            this.OnData(this._readBuffer, length);
            break;
          default:
            if (length <= 0)
            {
              this.CleanUp();
              goto case SocketError.TimedOut;
            }
            else
            {
              this.OnSocketError(errorCode);
              this.CleanUp();
              return;
            }
        }
      }
      catch (Exception ex)
      {
        this.OnError(ex.Message);
      }
      if (!this.IsRunning)
        return;
      try
      {
        this.Client.Client.BeginReceive(this._readBuffer, 0, this._readBuffer.Length, SocketFlags.None, new AsyncCallback(this.DoReceive), (object) null);
      }
      catch (Exception ex)
      {
        this.OnError(ex.Message);
      }
    }

    protected virtual void OnConnected()
    {
    }

    protected virtual void OnData(byte[] data, int length)
    {
    }

    protected virtual void OnSocketError(SocketError error)
    {
    }

    protected virtual void OnError(string message)
    {
    }

    protected virtual void Timeouted()
    {
    }

    protected virtual void OnDisconnected()
    {
    }

    public void Dispose()
    {
      DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(44, 1);
      interpolatedStringHandler.AppendLiteral("[TcpSession.cs] CleanUp call from Dispose (");
      interpolatedStringHandler.AppendFormatted<bool>(this.IsRunning);
      interpolatedStringHandler.AppendLiteral(")");
      Console.WriteLine(interpolatedStringHandler.ToStringAndClear());
      this.CleanUp();
      this.Client.Dispose();
    }
  }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

#nullable enable
namespace ExeNet
{
  public class TcpServer : IDisposable
  {
    public List<TcpSession> Sessions = new List<TcpSession>();
    protected TcpListener _listener;
    private Thread? _acceptThread;
    private ushort _idGen = 1;

    public IPAddress IPAddress { get; private set; }

    public int Port { get; private set; }

    public bool IsRunning { get; private set; }

    public TcpServer(IPAddress ip, int port)
    {
      this.IPAddress = ip;
      this.Port = port;
      this._listener = new TcpListener(ip, port);
    }

    public bool Start()
    {
      if (this.IsRunning)
        return false;
      try
      {
        this._listener.Start();
      }
      catch (SocketException ex)
      {
        this.OnSocketError(ex.SocketErrorCode);
        return false;
      }
      this._idGen = (ushort) 1;
      this.IsRunning = true;
      this._acceptThread = new Thread(new ThreadStart(this.Run));
      this._acceptThread.Name = "TcpServer Worker";
      this._acceptThread.Start();
      this.OnReady();
      return true;
    }

    public void Stop()
    {
      if (!this.IsRunning)
        return;
      this.IsRunning = false;
      this._acceptThread.Join();
      try
      {
        this._listener.Stop();
      }
      catch (SocketException ex)
      {
        this.OnSocketError(ex.SocketErrorCode);
      }
    }

    public TcpSession? GetSession(ushort id)
    {
      return this.Sessions.Where<TcpSession>((Func<TcpSession, bool>) (e => (int) e.ID == (int) id)).FirstOrDefault<TcpSession>();
    }

    public void Dispose()
    {
      this.Stop();
      GC.SuppressFinalize((object) this);
    }

    public ushort RequestID() => this._idGen++;

    private void Run()
    {
      while (this.IsRunning)
      {
        try
        {
          TcpClient client = this._listener.AcceptTcpClient();
          lock (this.Sessions)
          {
            TcpSession session = this.CreateSession(client);
            session.Start();
            this.Sessions.Add(session);
          }
        }
        catch (SocketException ex)
        {
          this.OnSocketError(ex.SocketErrorCode);
        }
        catch (InvalidOperationException ex)
        {
          this.OnError(ex.Message);
        }
      }
    }

    protected virtual TcpSession CreateSession(TcpClient client) => new TcpSession(this, client);

    protected virtual void OnReady()
    {
    }

    protected virtual void OnSocketError(SocketError error)
    {
    }

    protected virtual void OnError(string message)
    {
    }
  }
}

// Decompiled with JetBrains decompiler
// Type: ExeNet.UdpServer
// Assembly: ExeNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7AC46939-1C6E-4A4A-A8C2-645E334BBB85
// Assembly location: C:\Users\User\Desktop\ExeNet.dll

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

#nullable enable
namespace ExeNet
{
  public class UdpServer : IDisposable
  {
    private UdpClient _client;
    private Thread? _readThread;

    public bool IsRunning { get; private set; }

    public int Port { get; private set; }

    public UdpServer(int port) => this.Port = port;

    public void Dispose()
    {
      this.Stop();
      GC.SuppressFinalize((object) this);
    }

    public bool Start()
    {
      try
      {
        this._client = new UdpClient(this.Port);
        this._client.Client.ReceiveBufferSize = 128;
        this._client.Client.SendBufferSize = 128;
        this._client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, true);
      }
      catch (Exception ex)
      {
        this.OnError((IPEndPoint) null, ex.Message);
        return false;
      }
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        this._client.Client.IOControl(-1744830452, new byte[1], new byte[1]);
      this.IsRunning = true;
      this._readThread = new Thread(new ThreadStart(this.Run));
      this._readThread.Name = "UdpServer Worker";
      this._readThread.Start();
      this.OnReady();
      return true;
    }

    public void Send(IPEndPoint endpoint, ref byte[] data)
    {
      this.Send(endpoint, ref data, data.Length);
    }

    public void Send(IPEndPoint endpoint, ref byte[] data, int length)
    {
      this._client.Send(data, length, endpoint);
    }

    public void Stop()
    {
      this.IsRunning = false;
      this._readThread.Join();
    }

    private void Run()
    {
      IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, this.Port);
      while (this.IsRunning)
      {
        byte[] data = this._client.Receive(ref remoteEP);
        if (remoteEP != null)
        {
          try
          {
            this.OnData(remoteEP, ref data);
          }
          catch (SocketException ex)
          {
            this.OnSocketError(remoteEP, ex.SocketErrorCode);
          }
          catch (Exception ex)
          {
            this.OnError(remoteEP, ex.Message);
          }
        }
      }
      this._client.Close();
    }

    protected virtual void OnReady()
    {
    }

    protected virtual void OnSocketError(IPEndPoint endpoint, SocketError error)
    {
    }

    protected virtual void OnError(IPEndPoint? endpoint, string message)
    {
    }

    protected virtual void OnData(IPEndPoint sender, ref byte[] data)
    {
    }
  }
}

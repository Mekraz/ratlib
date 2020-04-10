using karkennet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace karkennet
{
    public class karlient : IDisposable
    {
        public delegate void kStateChanged(karlient sender, bool connected, string reason);
        public delegate void kPacketHeaderReceived(karlient sender, int value);

        public event kStateChanged stateChanged;
        public event kPacketHeaderReceived packetReceived;

        private Thread read;
        public NetworkStream stream;
        public BinaryWriter output;
        public BinaryReader input;
        public Socket xSocket;

        public karlient() : base() { }
        public karlient(Socket socket) { xSocket = socket; }

        public void konek(string host, int port)
        {
            if (xSocket == null)
                xSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            xSocket.BeginConnect(host, port, new AsyncCallback(konnectCallBack), this);
        }
        public void disKonnect()
        {
            if (xSocket == null)
                return;
            xSocket.BeginDisconnect(true, new AsyncCallback(disKonnectCallBack), this); 
        }

        protected static void konnectCallBack(IAsyncResult ar)
        {
            karlient konteks = (karlient)ar.AsyncState;
            try
            {
                konteks.xSocket.EndConnect(ar);
                if (konteks.xSocket.Connected)
                    konteks.OnStateChanged(true, "[+] Client Successfully connected to server");
            }
            catch (SocketException e)
            {
                konteks.OnStateChanged(false, "[-] Client failed to connect to server.../");
            }

        }
        protected static void disKonnectCallBack(IAsyncResult ar)
        {
            karlient konteks = (karlient)ar.AsyncState;
            try
            {
                konteks.xSocket.EndDisconnect(ar);
            }
            catch (SocketException e)
            {
                // lmao
            }
            konteks.OnStateChanged(false, "[-] Successfully disconnected from server");
        }
        protected void OnStateChanged(bool connected, string reason)
        {
            stateChanged?.Invoke(this, connected, reason);
        }
        public void Dispose()
        {
            input?.Dispose();
            stream?.Dispose();
            xSocket?.Dispose();
            output?.Dispose();
        }
        public void Start()
        {
            if (xSocket == null)
                return;
            if (!xSocket.Connected)
                return;

            stream = new NetworkStream(this.xSocket);
            input = new BinaryReader(stream, Encoding.UTF8);
            output = new BinaryWriter(stream, Encoding.UTF8);
            read = new Thread(() =>
            {
                do
                {
                    bool cread = this.xSocket.Poll(3000, SelectMode.SelectRead) && this.xSocket.Available > 0;
                    int paketHeader = 0;

                    try
                    {
                        paketHeader = input.ReadByte();
                    }
                    catch (IOException e)
                    {
                        OnStateChanged(false, "[-] Disconnected: " + e.Message);
                    }
                    if (paketHeader == 0)
                    {
                        OnStateChanged(false, "[-] The connection was aborted");
                        return;
                    }
                    if (paketHeader == -1)
                    {
                        OnStateChanged(false, "[-] The Connection was terminated");
                        return;
                    }
                    OnPacketHeaderReceive(paketHeader);
                } while (true);
            });
            read.IsBackground = true;
            read.Start();
        }
        protected void OnPacketHeaderReceive(int value)
        {
            packetReceived?.Invoke(this, (byte)value);
        }
        
    }
}

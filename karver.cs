using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace karkennet
{
    public class karver : IDisposable
    {

        public delegate void ConnectedClient(karver sender, karlient client);

        public event ConnectedClient cC;


        private Socket socket;
        public void Dispose()
        {
            socket?.Dispose();
        }
        protected void OnKlientKonnected(karlient client)
        {
            cC?.Invoke(this, client);
        }
        public void listen(int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[0];
            IPEndPoint ep = new IPEndPoint(ip, port);
            socket.Bind(ep);
            socket.Listen(150);
            socket.BeginAccept(new AsyncCallback(acceptCallBack), this);
        }
        protected static void acceptCallBack(IAsyncResult ar)
        {
            karver konteks = (karver)ar.AsyncState;
            try
            {
                Socket koneksi = konteks.socket.EndAccept(ar);
                karlient client = new karlient(koneksi);

                konteks.OnKlientKonnected(client);
            }
            catch (SocketException e)
            {
                
            }
            konteks.socket.BeginAccept(new AsyncCallback(acceptCallBack), konteks);
        }

    }
}

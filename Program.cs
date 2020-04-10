using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace karkennet
{
    public class Program
    {
        static karver server = new karver();
        static List<karlient> clientz = new List<karlient>();
        static void Main(string[] args)
        {
            server.cC += server_cC;
            server.listen(8850);

            Console.Read();
        }
        private static void server_cC(karver sender, karlient client)
        {
            Console.WriteLine("[+] Client Successfully connected!!");

            client.packetReceived += client_packetReceived;
            client.stateChanged += client_stateChanged;

            client.Start();

            lock(clientz)
            {
                clientz.Add(client);
            }
        }
        private static void client_packetReceived(karlient sender, int value)
        {
            switch (value)
            {
                case 0x58:
                    ReadMessage(sender);
                    break;
                case 0x48:
                    ReadFile(sender);
                    break;
            }
        }
        private static void ReadMessage(karlient sender)
        {
            string message = sender.input.ReadString();
            Console.WriteLine(message);
        }
        private static void ReadFile(karlient sender)
        {
            int r = 0;
            int tot = 0;
            byte[] buffer = new byte[8192];

            string fullname = sender.input.ReadString();
            string file = Path.GetFileName(fullname);
            long size = sender.input.ReadInt64();

            using (FileStream fileStream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                while((r = sender.input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, r);
                    tot += r;

                    if (tot == size)
                        break;
                }
            }
            Console.WriteLine("[+] Successfully received a file: {0}, Size: {1}KB", file, (decimal)size / 1024m);
        }
        private static void client_stateChanged(karlient client, bool connected, string reason)
        {
            Console.Write("[+] Connected: {0}, Reason: {1}", connected, reason);

            if (!connected)
            {
                lock (clientz)
                {
                    if(clientz.Contains(client))
                    {
                        clientz.Remove(client);
                    }
                }
            }
        }
    }
}

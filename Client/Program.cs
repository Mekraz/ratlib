using karkennet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static karlient lmao = new karlient();
        static void Main(string[] args)
        {
            // Jika connect ke server maka code ini yang mengexecute
            lmao.stateChanged += client_StateChanged;
            lmao.packetReceived += client_PacketReceived;

            lmao.konek("localhost", 8850);

            // Console akan membaca kode 
            Console.In.Read();
        }
        private static void client_StateChanged(karlient client, bool connected, string reason)
        {
            Console.WriteLine("[+] Client: Connected: {0}, reason: {1}", connected, reason);
            // jika terhubung
            if(connected)
            {
                // pengirim / client mengirim suatu file9
                client.Start(); //client mulai mengirim
                // mengirim suatu barang
                kirimPesan("Hello!, Im sending a file.../");
                kirimFile("C:\\Windows\\notepad.exe");
            }
        }
        private static void client_PacketReceived(karlient client, int value)
        {
            // code yang memberitahu tentang packet yang didapat
            //
            Console.WriteLine("[+] Client: Packet received: " + value.ToString("x2"));
        }

        // menambahkan variabel untuk mengirim pesan kepada server.
        private static void kirimPesan(string psn)
        {
            lmao.output.Write((byte)0x58);
            lmao.output.Write(psn);
        }
        private static void kirimFile(string namafile)
        {
            byte[] buffer = new byte[8192];
            int r = 0;

            lmao.output.Write((byte)0x48);
            lmao.output.Write(namafile);
            using (FileStream fs = new FileStream(namafile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                lmao.output.Write(fs.Length);
                while (( r = fs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    lmao.output.Write(buffer, 0, r);
                }
            }
        }
    }
}

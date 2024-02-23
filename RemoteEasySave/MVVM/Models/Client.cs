using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using RemoteEasySave.MVVM.Models;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace RemoteEasySave.MVVM.Models
{
    class Client
    {
        private static IPEndPoint ipep;
        public static ObservableCollection<Backup> BackupListInfo = new ObservableCollection<Backup>();


        public Client() { }

        public Socket SeConnecter()
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Sock.Connect(ipep);
            }
            catch (SocketException SoEx)
            {
                
            }

            return Sock;
        }

        public ObservableCollection<Backup> DialoguerReseau(Socket server)
        {
            byte[] data = new byte[1024];
            int recv = server.Receive(data);
            string stringData;

            stringData = Encoding.UTF8.GetString(data, 0, recv);

            ObservableCollection<Backup> backups = JsonConvert.DeserializeObject<ObservableCollection<Backup>>(stringData);

            while (true)
            {
                
                recv = server.Receive(data);

                stringData = Encoding.UTF8.GetString(data, 0, recv);

                backups = JsonConvert.DeserializeObject<ObservableCollection<Backup>>(stringData);

                return backups;

            }
        }

        public void Deconnecter(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
        }

    }
}

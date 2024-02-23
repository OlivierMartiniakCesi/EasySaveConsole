using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.ObjectModel;

namespace EasySaveV2.MVVM.Models
{
    class Server
    {

        static IPEndPoint clienti;


        public Socket SeConnecter()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);

            newsock.Listen(10);

            return newsock;

        }



        public Socket AccepterConnection(Socket socket)
        {
            Socket client = socket.Accept();
            if (client != null)
            {
                clienti = (IPEndPoint)client.RemoteEndPoint;
            }
            return client;
        }



        public void EcouterReseau(Socket client)
        {
            byte[] data = new byte[1024];

            data = Encoding.UTF8.GetBytes("coucou");
            client.Send(data, data.Length, SocketFlags.None);

            while (true)
            {     

                client.Send(Encoding.UTF8.GetBytes("coucou"));
            }

        }

        public  void Deconnecter(Socket socket)
        {
            socket.Close();
        }
    }
}

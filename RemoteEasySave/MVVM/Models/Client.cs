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
using System.Threading;
using System.Text.RegularExpressions;

namespace RemoteEasySave.MVVM.Models
{
    class Client
    {
        private static IPEndPoint ipep;


        public Client() { }

        public Socket SeConnecter()
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);
            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            Sock.Connect(ipep);
           

            return Sock;
        }





        public void Deconnecter(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
        }

    }
}

﻿using System;
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


        public Client() { }

        public Socket SeConnecter()
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);
            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            Sock.Connect(ipep);
           

            return Sock;
        }

        public void DialoguerReseau(Socket server, List<Backup> BackupListInfo)
        {
            byte[] data = new byte[1024];
            int recv = server.Receive(data);
            string stringData = Encoding.UTF8.GetString(data, 0, recv);

            string[] dataList = stringData.Split(',');

            string name = dataList[0];
            string source = dataList[1];
            string destination = dataList[2];
            string type = dataList[3];

            BackupListInfo.Add(new Backup(name, source, destination, type));
 

        }




        public void Deconnecter(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
        }

    }
}

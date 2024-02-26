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
        public static List<Backup> BackupListInfo { get; set; } = new List<Backup>();


        public Client() { }

        public Socket SeConnecter()
        {
            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);
            Socket Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            Sock.Connect(ipep);
           

            return Sock;
        }


        public async Task<List<Backup>> DialoguerReseau(Socket server)
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead;
                try
                {
                    // Commencer la réception de manière asynchrone
                    bytesRead = await server.ReceiveAsync(buffer, SocketFlags.None);
                }
                catch (SocketException)
                {
                    // Gérer les erreurs de réception
                    break; // Sortir de la boucle en cas d'erreur
                }

                if (bytesRead > 0)
                {
                    string stringData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] dataList = stringData.Split(","); // Utilisation du délimiteur pour séparer les données

                    // Vérifier si dataList contient au moins 4 éléments avant de procéder
                    if (dataList.Length >= 4)
                    {
                        for (int i = 0; i < dataList.Length; i += 4)
                        {
                            string name = dataList[i];
                            string source = dataList[i + 1];
                            string destination = dataList[i + 2];
                            string type = dataList[i + 3];

                            BackupListInfo.Add(new Backup(name, source, destination, type));
                        }
                    }
                }
                else
                {
                    // Aucune donnée reçue, peut-être la connexion est fermée
                    break; // Sortir de la boucle si aucune donnée n'est reçue
                }
            }

            return BackupListInfo;
        }







        public void Deconnecter(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
        }

    }
}

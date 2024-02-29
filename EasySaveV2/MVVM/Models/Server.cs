using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.ObjectModel;

namespace EasySaveV2.MVVM.Models
{
    class Server
    {
        /******************************************************/
        /* Déclaration des attributs en privé pour le serveur */
        /******************************************************/
        private static IPEndPoint clienti;

        /*********************************************************/
        /* Déclaration des attributs en publique pour le serveur */
        /*********************************************************/
        public Socket client;

        /********************************************************/
        /* Déclaration des méthodes en publique pour le serveur */
        /********************************************************/

        // Méthode pour établir une connexion serveur.
        public Socket SeConnecter()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10001);

            // Déclaration d'un socket avec le protocole TCP/IP
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);

            // Socket en état d'écoute 
            newsock.Listen(10);

            return newsock;

        }


        // Méthode pour accepter une connexion.
        public Socket AccepterConnection(Socket socket)
        {
            // Accepte une connexion entrante
            client = socket.Accept();
            
            if (client != null)
            {
                clienti = (IPEndPoint)client.RemoteEndPoint;
            }
            return client;
        }


        // Méthode pour envoyer des données sur le socket client.
        public void EcouterReseau(string name, string source, string destination, string type)
        {
            byte[] data = new byte[1024];

            // Convertit les données à envoyer en tableau d'octets
            data = Encoding.UTF8.GetBytes(name + "," + source + "," + destination + "," + type + "|");

            // Envoie les données sur le socket client
            client.Send(data, data.Length, SocketFlags.None);

        }

        // Méthode pour fermer le socket.
        public void Deconnecter(Socket socket)
        {
            socket.Close();
        }
    }
}

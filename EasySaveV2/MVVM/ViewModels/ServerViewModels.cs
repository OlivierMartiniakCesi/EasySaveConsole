using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using EasySaveV2.MVVM.Models;
using EasySaveV2.MVVM.Views;
using System.Net.Sockets;

namespace EasySaveV2.MVVM.ViewModels
{
    class ServerViewModels
    {
        /*****************************************/
        /* Déclaration des attributs en publique */
        /*****************************************/
        public static Server server = new Server();
        public static Socket socket;
        public static Socket client;
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;

        /***************************************/
        /* Déclaration des méthode en publique */
        /***************************************/

        // Méthode pour démarrer le serveur
        public void start()
        {
            socket = server.SeConnecter();

        }

        // Méthode pour envoyer aux clients les informations des sauvegardes
        public void receiveBackupInfo(string name, string source, string destination, string type)
        {
            server.EcouterReseau(name, source, destination, type);
        }

        // Méthode pour démarrer accepter une connexion
        public void AcceptSocket()
        {
            server.AccepterConnection(socket);
        }

        //  Méthode pour fermer le serveur
        public void exit()
        {
            server.Deconnecter(socket);

        }
    }
}

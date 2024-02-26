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
        public static Server server = new Server();
        public static Socket socket;
        public static Socket client;
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;

        public void start()
        {
            socket = server.SeConnecter();

        }

        public void receiveBackupInfo(string name, string source, string destination, string type)
        {
            server.EcouterReseau(name, source, destination, type);
        }

        public void AcceptSocket()
        {
            server.AccepterConnection(socket);
        }


        public void exit()
        {
            server.Deconnecter(socket);

        }
    }
}

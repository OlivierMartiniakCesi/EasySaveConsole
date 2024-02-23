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
        private static Server server = new Server();
        private static Socket socket;
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;

        public void start()
        {
            socket = server.SeConnecter();

        }

        public void receiveBackupInfo()
        {
            server.EcouterReseau(socket);
        }


        public void exit()
        {
            server.Deconnecter(socket);

        }
    }
}

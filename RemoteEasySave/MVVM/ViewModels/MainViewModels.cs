using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteEasySave.MVVM.Models;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;

namespace RemoteEasySave.MVVM.ViewModels
{
    class MainViewModels
    {
        private static Client client = new Client();
        private static Socket socket;
        public List<Backup> BackupList { get; set; } = Client.BackupListInfo;
        public  void start()
        {
            socket = client.SeConnecter();
            
        }

        public void receiveBackupInfo()
        {
            int currentPosition = 0;


            while (true)
            {
                BackupList = client.DialoguerReseau(socket);

                bool backupListFull = false;

                if (currentPosition == BackupList.Count)
                {
                    backupListFull = true;
                    if (backupListFull == true)
                    {
                        break;
                    }
                }
                else
                {
                    currentPosition += BackupList.Count;
                    backupListFull = false;
                }
              
            }

        }

        public void exit()
        {
            client.Deconnecter(socket);
                
        }

    }
}

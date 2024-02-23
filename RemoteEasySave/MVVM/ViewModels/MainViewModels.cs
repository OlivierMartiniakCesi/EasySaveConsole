﻿using System;
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

namespace RemoteEasySave.MVVM.ViewModels
{

    class MainViewModels
    {
        private static Client client = new Client();
        private static Socket socket;
        public static ObservableCollection<Backup> BackupListInfo = new ObservableCollection<Backup>();

        public  void start()
        {
            socket = client.SeConnecter();
            
        }

        public void receiveBackupInfo()
        {
            client.DialoguerReseau(socket);
        }


        public void exit()
        {
            client.Deconnecter(socket);
                
        }

    }
}

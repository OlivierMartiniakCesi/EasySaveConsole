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
using System.ComponentModel;
using System.Threading.Tasks;

namespace RemoteEasySave.MVVM.ViewModels
{
    class MainViewModels
    {
        private static Client client = new Client();
        private static Socket socket;
        //public List<Backup> BackupList { get; set; } = Client.BackupListInfo;

        public MainViewModels()
        {
            BackupList = new ObservableCollection<Backup>();
        }

        private ObservableCollection<Backup> _backupList;
        public ObservableCollection<Backup> BackupList
        {
            get { return _backupList; }
            set
            {
                _backupList = value;
                OnPropertyChanged("BackupList");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void start()
        {
            socket = client.SeConnecter();

        }
        public async Task ReceiveDataFromServer()
        {
            byte[] data = new byte[1024];

            while (true)
            {
                int recv = await Task.Run(() => socket.Receive(data));

                if (recv == 0)
                {
                    break;
                }


                string stringData = Encoding.UTF8.GetString(data, 0, recv);
                string[] dataList = stringData.Split(',');

                string name = dataList[0];
                string source = dataList[1];
                string destination = dataList[2];
                string type = dataList[3];

                Backup newBackup = new Backup(name, source, destination, type);

                BackupList.Add(newBackup);
            }
        }


        public void exit()
        {
            client.Deconnecter(socket);
                
        }

    }
}

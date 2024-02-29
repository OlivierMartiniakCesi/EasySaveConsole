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
using System.Windows;

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
            // Effacer le contenu de BackupList avant de commencer à recevoir des données
            Application.Current.Dispatcher.Invoke(() =>
            {
                lock (BackupList)
                {
                    BackupList.Clear();
                }
            });

            while (true)
            {
                byte[] data = new byte[1024];
                int recv = await socket.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);

                if (recv == 0)
                {
                    break;
                }

                string stringData = Encoding.UTF8.GetString(data, 0, recv);
                string[] dataList = stringData.Split('|');

                foreach (string item in dataList)
                {
                    string[] elements = item.Split(',');
                    if (elements.Length >= 4)
                    {
                        string name = elements[0];
                        string source = elements[1];
                        string destination = elements[2];
                        string type = elements[3];

                        // Vérifier si la sauvegarde existe déjà dans BackupList
                        bool exists = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            lock (BackupList)
                            {
                                exists = BackupList.Any(b => b.getName() == name && b.getSourceDirectory() == source && b.getTargetDirectory() == destination && b.getType() == type);
                            }
                        });

                        // Si la sauvegarde n'existe pas encore, l'ajouter à BackupList
                        if (!exists)
                        {
                            Backup newBackup = new Backup(name, source, destination, type);
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                lock (BackupList)
                                {
                                    BackupList.Add(newBackup);
                                }
                            });
                        }
                    }
                }
            }
        }





        public void exit()
        {
            client.Deconnecter(socket);
                
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;
using EasySaveV2.MVVM.ViewModels;
using System.Collections.ObjectModel;
using EasySaveV2.MVVM.Models;

namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour DashboardViews.xaml
    /// </summary>
    public partial class DashboardViews : UserControl
    {
        private ListBox ListBackupAff;
        public RelayCommand LaunchBackupCommand { get; set; }
        public DashboardViews()
        {
            InitializeComponent();

            DataContext = new DashboardViewModels();
            LaunchBackupCommand = new RelayCommand(BtnLauch_ClickSolo);
            ListBackupAff = FindName("BackupList") as ListBox;
        }

        private void BtnLauch_Click(object sender, RoutedEventArgs e)
        {
            List<Backup> selectedBackups = new List<Backup>();
            foreach (var selectedItem in ListBackupAff.SelectedItems)
            {
                if (selectedItem is Backup backup)
                {
                    selectedBackups.Add(backup);
                }
            }
            DashboardViewModels.LaunchSlotBackup(selectedBackups);
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.GoToDashboard(sender, e);
        }

        private void BtnLauch_ClickSolo(object parameter)
        {
            // Récupérer le bouton qui a déclenché l'événement
            Button clickedButton = parameter as Button;

            if (clickedButton != null)
            {
                // Récupérer la sauvegarde associée au bouton
                Backup selectedBackup = clickedButton.DataContext as Backup;

                if (selectedBackup != null)
                {
                    // Appeler la méthode BtnLauch_Click avec la sauvegarde sélectionnée
                    BtnLauch_Click(clickedButton, new RoutedEventArgs(Button.ClickEvent));
                }
            }
        }

        public void BtnModify_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveBackups(object sender, RoutedEventArgs e)
        {
            Backup backup = ((Button)sender).Tag as Backup;
            DashboardViewModels.DeleteBackupSetting(backup);
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.GoToDashboard(sender, e);
        }
    }
}

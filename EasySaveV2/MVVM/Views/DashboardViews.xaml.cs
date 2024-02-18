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
        public DashboardViews()
        {
            InitializeComponent();

            DataContext = new DashboardViewModels();
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

        private void BtnLauch_ClickSolo(object sender, RoutedEventArgs e)
        {
            // Récupérer le bouton qui a déclenché l'événement
            Backup backup = ((Button)sender).Tag as Backup;
            List<Backup> selectedBackups = new List<Backup>();
            selectedBackups.Add(backup);
            DashboardViewModels.LaunchSlotBackup(selectedBackups);
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.GoToDashboard(sender, e);
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

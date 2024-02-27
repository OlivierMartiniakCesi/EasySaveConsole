﻿using System;
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
using EasySaveV2.MVVM.ViewModels;
using EasySaveV2.MVVM.Views;
using EasySaveV2.MVVM.Models;
using System.Collections.ObjectModel;
using Serilog;
using System.Windows.Threading;

namespace EasySaveV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerViewModels serverViewModels = new ServerViewModels();
        BackupViewModels backupViewModels = new BackupViewModels();
        public MainWindow()
        {
            InitializeComponent();
            serverViewModels.start();
            serverViewModels.AcceptSocket();
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler((sender, e) => DashboardViewModels.MonitorProcess((ObservableCollection<Backup>) DashboardViewModels.BackupList));
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            BackupViewModels.GetJSON();
            SettingsViewModels.typelog();
            dailylogs.selectedLogger.Information("Application start successfully !");
            DataContext = new DashboardViewModels();
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Language/DictionaryEnglish.xaml", UriKind.RelativeOrAbsolute);
        }

        public void GoToDashboard(object sender, RoutedEventArgs e)
        {
            DataContext = new DashboardViewModels();
        }

        public void GoToEdit(object sender, RoutedEventArgs e)
        {
            Backup backup = ((Button)sender).Tag as Backup;
            DataContext = new EditsViewModels(backup);
        }

        private void GoToBackup(object sender, RoutedEventArgs e)
        {
            DataContext = new BackupViewModels();
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            DataContext = new SettingsViewModels();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            dailylogs.selectedLogger.Information("Application close successfully !");
            Close();
        }
    }
}

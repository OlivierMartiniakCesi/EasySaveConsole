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
using EasySaveV2.MVVM.ViewModels;
using EasySaveV2.MVVM.Views;
using EasySaveV2.MVVM.Models;
using System.Collections.ObjectModel;
using Serilog;

namespace EasySaveV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

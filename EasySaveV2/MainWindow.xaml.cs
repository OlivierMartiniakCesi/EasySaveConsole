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
        }

        private void GoToDashboard(object sender, RoutedEventArgs e)
        {
           Home.Content = new DashboardViews();
        }

        private void GoToBackup(object sender, RoutedEventArgs e)
        {
            Home.Content = new BackupViews();
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            Home.Content = new SettingsViews();
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

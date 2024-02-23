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
using RemoteEasySave.MVVM.Models;
using RemoteEasySave.MVVM.ViewModels;

namespace RemoteEasySave
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainViewModels main = new MainViewModels();
        public MainWindow()
        {
            InitializeComponent();
            main.start();
            main.receiveBackupInfo();

        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            main.exit();
            Close();
        }
    }
}

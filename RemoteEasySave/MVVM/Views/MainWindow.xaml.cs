using System.Threading.Tasks;
using System.Windows;
using RemoteEasySave.MVVM.ViewModels;

namespace RemoteEasySave.MVVM.Views
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
            Task task = Task.Run(async () => await main.receiveBackupInfo());
            _ = task;

        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            main.exit();
            Close();
        }
    }
}

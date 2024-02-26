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
            DataContext = main;
            main.start();
            Task.Run(async () => await main.ReceiveDataFromServer());
            
            


        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            main.exit();
            Close();
        }
    }
}

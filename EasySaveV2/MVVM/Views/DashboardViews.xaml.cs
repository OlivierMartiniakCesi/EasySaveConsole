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

namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour DashboardViews.xaml
    /// </summary>
    public partial class DashboardViews : UserControl
    {
        public DashboardViews()
        {
            InitializeComponent();
        }

        public void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var json = File.ReadAllText(@"C:\JSON\confbackup.json");

            List<DashboardViewModels.Backup> _personnes = JsonConvert.DeserializeObject<List<DashboardViewModels.Backup>>(json);

            GridPeople.ItemsSource = _personnes;
        }
    }
}

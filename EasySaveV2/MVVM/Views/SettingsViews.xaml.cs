using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EasySaveV2.MVVM.ViewModels;
using System.Windows.Resources;
using System.IO;
using System.Configuration;
namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour SettingsViews.xaml
    /// </summary>
    public partial class SettingsViews : UserControl
    {


        SettingsViewModels Settings = new SettingsViewModels();
        public SettingsViews()
        {
            InitializeComponent();
            ButtonLanguage.IsChecked = Settings.ToggleButtonState;
        }


        public void CheckedLanguage(object sender, RoutedEventArgs e)
        {
            Settings.ToggleButtonState = true;
            DockPanel.SetDock(ButtonLanguage, Dock.Right);
            Settings.TraductorFrench();
        }

        public void UncheckedLanguage(object sender, RoutedEventArgs e)
        {
            Settings.ToggleButtonState = false;
            DockPanel.SetDock(ButtonLanguage, Dock.Left);
            Settings.TraductorEnglish();
        }


        private void ToggleButtonLogs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                bool? isChecked = toggleButton.IsChecked;
                if (isChecked.HasValue)
                {
                    SettingsViewModels.Formatlog((bool) isChecked);
                }
                else
                {
                    // La valeur est indéterminée
                }
            }
        }

        private void AddExtensionEncrypt(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string extension = ExtensionBackup.Text.Trim(); 
                if (!string.IsNullOrEmpty(extension) && extension[0] != '.')
                {
                    Settings.AddList(extension);
                }
            }
        }
    }
}

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
using EasySaveV2.MVVM.Models;
using EasySaveV2.MVVM.ViewModels;
using EasySaveV2.MVVM.Views;
using Serilog;

namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour EditsViews.xaml
    /// </summary>
    public partial class EditsViews : UserControl
    {

        public EditsViews()
        {
            InitializeComponent();

        }
        private void OpenFileExplorerDestination(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Title = "Sélectionnez un dossier";

            openFileDialog.CheckFileExists = false;
            openFileDialog.FileName = "Sélectionnez un dossier";

            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du dossier sélectionné
                string selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                backupDestEdit.Text = selectedFolderPath;
            }
        }

        private void OpenFileExplorerSource(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Title = "Sélectionnez un dossier";

            openFileDialog.CheckFileExists = false;
            openFileDialog.FileName = "Sélectionnez un dossier";

            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du dossier sélectionné
                string selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                backupSourceEdit.Text = selectedFolderPath;
            }
        }

        private RadioButton GetSelectedRadioButton(StackPanel stackPanel)
        {
            foreach (var child in stackPanel.Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    return radioButton;
                }
            }
            return null;
        }
        private void EditBackup(object sender, RoutedEventArgs e)
        {
            if (backupNameEdit.Text != "" && backupSourceEdit.Text != "" && backupDestEdit.Text != "")
            {
                RadioButton selectedRadioButton = GetSelectedRadioButton(typeEdit);
                string backupType = selectedRadioButton.Content.ToString();
                EditsViewModels.SaveBackupSettings(backupNameEdit.Text, backupSourceEdit.Text, backupDestEdit.Text, backupType);
                Log.Information("Backup {backupName.Text} successfully !");
                backupNameEdit.Text = "Type the back-up name";
                backupSourceEdit.Text = "Source";
                backupDestEdit.Text = "Destination";
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.GoToDashboard(sender, e);
            }
        }
    }
}

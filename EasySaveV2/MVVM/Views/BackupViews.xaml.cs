using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using EasySaveV2.MVVM.ViewModels;
using System.Windows.Media.Imaging;
using System;
using Serilog;

namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour BackupViews.xaml
    /// </summary>
    public partial class BackupViews : UserControl
    {
        public BackupViews()
        {

            InitializeComponent();
        }
        private void OpenFileExplorerSource_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Title = "Sélectionnez un dossier";

            openFileDialog.CheckFileExists = false;
            openFileDialog.FileName = "Sélectionnez un dossier";

            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du dossier sélectionné
                string selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                backupSource.Text = selectedFolderPath;
            }
        }
        private void OpenFileExplorerDestination_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Title = "Sélectionnez un dossier";

            openFileDialog.CheckFileExists = false;
            openFileDialog.FileName = "Sélectionnez un dossier";

            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du dossier sélectionné
                string selectedFolderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                backupDest.Text = selectedFolderPath;
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

        public void BackupCreator(object sender, RoutedEventArgs e)
        {
            if (backupName.Text != "" && backupSource.Text != "" && backupDest.Text != "")
            {
                RadioButton selectedRadioButton = GetSelectedRadioButton(type);
                string backupType = selectedRadioButton.Content.ToString();
                BackupViewModels.CreateSlotBackup(backupName.Text, backupSource.Text, backupDest.Text, backupType);
                Log.Information("Backup {backupName.Text} successfully !");
                backupName.Text = "Type the back-up name";
                backupSource.Text = "Source";
                backupDest.Text = "Destination";
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.GoToDashboard(sender, e);
            }
        }
    }
}

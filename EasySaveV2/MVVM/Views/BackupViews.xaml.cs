using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using EasySaveV2.MVVM.ViewModels;
using System.Windows.Media.Imaging;
using System;
using Serilog;
using EasySaveV2.MVVM.Models;
using System.Threading;

namespace EasySaveV2.MVVM.Views
{
    /// <summary>
    /// Logique d'interaction pour BackupViews.xaml.
    /// Cette vue permet à l'utilisateur de créer les paramètres d'une sauvegarde non existante, tels que le nom de la sauvegarde, le dossier source le dossier de destination et le type.
    /// Les utilisateurs peuvent sélectionner les dossiers à l'aide de l'explorateur de fichiers intégré et choisir le type de sauvegarde à partir d'une liste de radio boutons et le nom directement par une TextBox.
    /// </summary>
    public partial class BackupViews : UserControl
    {
        public BackupViews()
        {
            // Cette méthode est responsable de charger et d'initialiser tous les éléments graphiques
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
                dailylogs.selectedLogger.Information("Backup {backupName.Text} successfully !");
                backupName.Text = "Type the back-up name";
                backupSource.Text = "Source";
                backupDest.Text = "Destination";
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.GoToDashboard(sender, e);
            }
        }
    }
}

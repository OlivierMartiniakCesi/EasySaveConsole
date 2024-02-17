using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

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
            // Créer une boîte de dialogue de sélection de fichier
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du fichier sélectionné
                string selectedFilePath = openFileDialog.FileName;

                // Stocker le chemin du fichier comme nécessaire (par exemple, dans une variable ou dans une propriété)
                // Vous pouvez utiliser selectedFilePath comme bon vous semble ici
                // Par exemple, affecter la valeur à un TextBox, à une propriété, etc.

                // Afficher le chemin du fichier dans votre TextBlock (ou TextBox, selon votre structure)
                backupSource.Text = selectedFilePath;
            }
        }
        private void OpenFileExplorerDestination_Click(object sender, RoutedEventArgs e)
        {
            // Créer une boîte de dialogue de sélection de fichier
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Afficher la boîte de dialogue et vérifier si l'utilisateur a sélectionné un fichier
            if (openFileDialog.ShowDialog() == true)
            {
                // Récupérer le chemin du fichier sélectionné
                string selectedFilePath = openFileDialog.FileName;

                // Stocker le chemin du fichier comme nécessaire (par exemple, dans une variable ou dans une propriété)
                // Vous pouvez utiliser selectedFilePath comme bon vous semble ici
                // Par exemple, affecter la valeur à un TextBox, à une propriété, etc.

                // Afficher le chemin du fichier dans votre TextBlock (ou TextBox, selon votre structure)
                backupDest.Text = selectedFilePath;
            }
        }
    }
}

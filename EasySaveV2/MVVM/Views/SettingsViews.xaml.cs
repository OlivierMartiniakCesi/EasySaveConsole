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
using Microsoft.Win32;

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
                    SettingsViewModels.Formatlog((bool)isChecked);
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
                string extension = ExtensionBackupTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(extension) && extension[0] == '.')
                {
                    Settings.AddList(extension);
                }
            }
        }

        private void ExtensionBackupTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "Extension")
            {
                textBox.Text = "";
            }
        }

        private void ExtensionBackupTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "Extension";
            }
        }



        // Méthode permettant d'importer un fichier ".txt" contenant les noms d'extensions
        private void ImportButton(object sender, RoutedEventArgs e)
        {
            //
        }



        private void ExportButton(object sender, RoutedEventArgs e)
        {
            /*try
            {
                string extension = ExtensionBackupTextBox.Text.Trim();

                if (!string.IsNullOrEmpty(extension))
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Fichiers texte (*.txt)|*.txt";
                    saveFileDialog.Title = "Exporter les extensions";
                    saveFileDialog.FileName = "extensions";

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Ajouter la nouvelle extension au fichier existant ou créer le fichier s'il n'existe pas
                        File.AppendAllText(saveFileDialog.FileName, extension + Environment.NewLine);

                        MessageBox.Show("Extension ajoutée avec succès dans " + saveFileDialog.FileName, "Export réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Le contenu est vide. Veuillez saisir des extensions avant d'exporter.", "Attention", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'exportation des extensions : " + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }
    }
}

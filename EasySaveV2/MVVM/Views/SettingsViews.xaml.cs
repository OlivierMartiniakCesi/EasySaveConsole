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
            myListBox.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            myListBox.PreviewMouseMove += ListBox_PreviewMouseMove;
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
                string extension = ExtensionTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(extension) && extension[0] == '.')
                {
                    Settings.AddList(extension);
                }
            }
        }



        private string originalText;
        private void ExtensionTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            originalText = textBox.Text;

            if (originalText.Contains("Extension"))
            {
                textBox.Text = "";
            }
        }

        private void ExtensionTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = originalText;
            }
        }


        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            if (listBox.SelectedItem != null)
            {
                DragDrop.DoDragDrop(listBox, listBox.SelectedItem, DragDropEffects.Move);
            }
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBox listBox = (ListBox)sender;

                object draggedItem = listBox.SelectedItem;

                if (draggedItem != null)
                {
                    DataObject dragData = new DataObject("myListBoxItem", draggedItem);
                    DragDrop.DoDragDrop(listBox, dragData, DragDropEffects.Move);
                }
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox listBox = (ListBox)sender;

            if (e.Data.GetDataPresent("myListBoxItem"))
            {
                var droppedItem = e.Data.GetData("myListBoxItem");

                if (droppedItem != null)
                {
                    int index = -1;

                    for (int i = 0; i < listBox.Items.Count; i++)
                    {
                        ListBoxItem item = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);

                        if (item != null)
                        {
                            Point position = e.GetPosition(item);

                            if (position.Y < item.ActualHeight / 2)
                            {
                                index = i;
                                break;
                            }
                        }
                    }

                    if (index == -1)
                    {
                        index = listBox.Items.Count - 1;
                    }

                    if (droppedItem is string extension && SettingsViewModels.ExtensionCryptoSoft.Contains(extension))
                    {
                        SettingsViewModels.ExtensionCryptoSoft.Remove(extension);
                        SettingsViewModels.ExtensionCryptoSoft.Insert(index, extension);
                    }
                }
            }
        }










        // Méthode permettant d'importer un fichier ".txt" contenant les noms d'extensions
        private void ImportButton(object sender, RoutedEventArgs e)
        {
        }


        // Méthode permettant d'exporter un fichier ".txt" contenant les noms d'extensions
        private void ExportButton(object sender, RoutedEventArgs e)
        {
        }
    }
}
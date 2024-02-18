using EasySaveV2.MVVM.Models;
using EasySaveV2;
using EasySaveV2.MVVM.Views;
using EasySaveV2.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Xml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Serilog;
using Newtonsoft.Json.Linq;



namespace EasySaveV2.MVVM.ViewModels
{

    class DashboardViewModels
    {
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;


        public DashboardViewModels()
        {
            BackupViewModels.GetJSON();
        }

        public static void LaunchSlotBackup(List<Backup> backupList)
        {
            foreach (Backup SelectBckup in BackupViewModels.BackupListInfo)
            {
                //Create directory if it doesn't already exist
                if (Directory.Exists(SelectBckup.getTargetDirectory()))
                {
                    foreach (string AllDirectory in Directory.GetDirectories(SelectBckup.getSourceDirectory(), "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(AllDirectory.Replace(SelectBckup.getSourceDirectory(), SelectBckup.getTargetDirectory()));
                    }
                }
                else
                {
                    Directory.CreateDirectory(SelectBckup.getTargetDirectory());
                    foreach (string AllDirectory in Directory.GetDirectories(SelectBckup.getSourceDirectory(), "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(AllDirectory.Replace(SelectBckup.getSourceDirectory(), SelectBckup.getTargetDirectory()));
                    }

                    if (SelectBckup != null)
                    {
                        if (SelectBckup.getType() == "complet" || SelectBckup.getType() == "Complet" || SelectBckup.getType() == "Full" || SelectBckup.getType() == "full")
                        {
                            TypeComplet(SelectBckup.getSourceDirectory(), SelectBckup.getTargetDirectory());
                        }
                        else
                        {
                            TypeDifferential(SelectBckup.getSourceDirectory(), SelectBckup.getTargetDirectory());
                        }
                    }
                    else
                    {
                        Log.Information("Sauvegarde non trouvée.");
                    }
                }
            }
        }
        public static void TypeComplet(string PathSource, string PathTarget)
        {
            // Create All Directories
            foreach (string AllDirectory in Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(AllDirectory.Replace(PathSource, PathTarget));
            }

            // Copy Files
            foreach (string AllFiles in Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories))
            {
                string targetFilePath = Path.Combine(PathTarget, AllFiles.Substring(PathSource.Length + 1));
                CopyFileWithProgress(AllFiles, targetFilePath);
                Log.Information($"Le fichier '{AllFiles}' a été copié vers {PathTarget}");
            }
        }
        /*public static void TypeComplet(string PathSource, string PathTarget)
        {
            //Create All Repertories
            foreach (string AllDirectory in Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(AllDirectory.Replace(PathSource, PathTarget));
            }

            //Copy Files
            foreach (string AllFiles in Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories))
            {
                string targetFilePath = Path.Combine(PathTarget, AllFiles.Substring(PathSource.Length + 1));
                File.Copy(AllFiles, AllFiles.Replace(PathSource, PathTarget), true);
            }

        }*/

        private static void CopyFileWithProgress(string sourceFilePath, string destinationFilePath)
        {
            using (FileStream sourceStream = File.Open(sourceFilePath, FileMode.Open))
            {
                using (FileStream destinationStream = File.Create(destinationFilePath))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    long totalBytes = new FileInfo(sourceFilePath).Length;
                    long copiedBytes = 0;
                    int lastProgressPercentage = 0;

                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        destinationStream.Write(buffer, 0, bytesRead);
                        copiedBytes += bytesRead;

                        // Calculer la progression
                        int progressPercentage = (int)((double)copiedBytes / totalBytes * 100);

                        if (progressPercentage != lastProgressPercentage)
                        {
                            // Afficher la progression uniquement si elle a changé
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"Copy progress: {progressPercentage}% pour " + sourceFilePath);
                            lastProgressPercentage = progressPercentage;
                        }
                    }
                    Console.WriteLine(); // Nouvelle ligne après la copie complète
                }
            }
        }


    }
}
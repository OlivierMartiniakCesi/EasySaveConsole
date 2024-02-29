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
using System.Threading;
using System.Windows.Input;
using System.Net.Sockets;

namespace EasySaveV2.MVVM.ViewModels
{
    class DashboardViewModels
    {
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;
        private static ServerViewModels serverViewModels = new ServerViewModels();

        private static ManualResetEventSlim backupCompletedEvent = new ManualResetEventSlim(false);
        private static bool canBeExecuted = true;
        private static int durationInSeconds;

        public DashboardViewModels()
        {
            foreach (Backup backup in BackupList)
            {
               serverViewModels.receiveBackupInfo(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getType());
            }

        }

        public static void LaunchSlotBackup(List<Backup> backupList)
        {
            // Use Parallel.ForEach to process each backup in a separate thread
            Parallel.ForEach(backupList, backup =>
            {
                {
                    // Use a separate thread for each backup
                    Thread backupThread = new Thread(() =>
                    {
                        backup.setState("On");
                        backup.setStopped("False");
                        backup.States = "NoPaused";
                        // Check if the backup is paused
                        if (!canBeExecuted)
                        {
                            Thread.Sleep(1000); // Wait for 1 second before checking again+promptimpossile
                        }
                        
                        // Create directory if it doesn't already exist
                        if (!Directory.Exists(backup.getTargetDirectory()))
                        {
                            Parallel.ForEach(Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories),
                                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                (AllDirectory) =>
                                {
                                    try
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                        dailylogs.selectedLogger.Information("Created directory ", AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                    }
                                    finally
                                    {
                                    }
                                });
                        }
                        serverViewModels.receiveBackupInfo(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getType());
                        // Vérification du type de la back-up
                        if (backup.getType().Equals("Full", StringComparison.OrdinalIgnoreCase) || backup.getType().Equals("Complet", StringComparison.OrdinalIgnoreCase))
                        {
                            // Lancement de la back-up en type complet
                            TypeComplet(backup);
                            SettingsViewModels.SetSaveStateBackup(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getcrypting());
                            backup.setState("Off");

                        }
                        else
                        {
                            // Lancement de la back-up en type différentiel
                            TypeDifferential(backup);
                            SettingsViewModels.SetSaveStateBackup(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getcrypting());
                            backup.setState("Off");
                        }
                    });
                    backupThread.Start();

                }
            });

            Console.WriteLine("All selected backups have been launched.");
            // Signal that the backup has completed
            backupCompletedEvent.Set();
        }

        public static void MonitorProcess(ObservableCollection<Backup> backupList)
        {
            // Start the process monitoring thread
            Process[] processes = Process.GetProcessesByName("CalculatorApp");

            // check if a software of the list is running
            if (processes.Length > 0)
            {
                canBeExecuted = false;
            }
            else
            {
                canBeExecuted = true;
            }
        }

        public static void ContinueLauch(Backup backup)
        {
            backup.setState("On");
            backup.States = "NoPaused";
            dailylogs.selectedLogger.Information($"Backup {backup.getName()} reprend son exécution");
        }

        public static void PauseLauch(Backup backup)
        {
            if (backup.getState() == "On")
            {
                backup.setState("Off");
                backup.States = "Paused";
                dailylogs.selectedLogger.Information($"Backup {backup.getName()} en pause");
            }
        }

        public static void StopLauch(Backup backup)
        {
            if (backup.getState() == "On")
            {
                backup.setStopped("True");
                backup.States = "Stop";
                dailylogs.selectedLogger.Information($"Backup {backup.getName()} arrêté");
                backup.Progress = 0;
            }
        }
        private static long CalculateTotalBytes(string directory)
        {
            long totalBytes = 0;
            foreach (string file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                totalBytes += new FileInfo(file).Length;
            }
            return totalBytes;
        }
        private static void ReportProgress(Backup backup, long copiedBytes, long totalBytes)
        {
            double progressPercentage = (double)copiedBytes / totalBytes * 100;
            backup.Progress = (int)progressPercentage;
            if (backup.Progress % 1 == 0) // Mettre à jour toutes les 1%
            {
                Console.WriteLine($"Backup {backup.getName()} Progress: {progressPercentage:F2}%");
            }
        }
        public static void TypeComplet(Backup backup)
        {
            long totalBytes = CalculateTotalBytes(backup.getSourceDirectory());
            long copiedBytes = 0;
            List<string> entryFiles = Priority(backup.getSourceDirectory());
            // Create all directories sequentially
            foreach (var directory in Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories))
            {
                if (!canBeExecuted || (backup.getState() == "Off"))
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution paused");
                    while(!canBeExecuted || (backup.getState() == "Off"))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (backup.getStopped() == "True")
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution stopped");
                    return;
                }
                try
                {
                    Directory.CreateDirectory(directory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                    Interlocked.Add(ref copiedBytes, Directory.GetFiles(directory).Length);
                    ReportProgress(backup, copiedBytes, totalBytes);
                    if (copiedBytes % 1 == 0)
                    {
                        ReportProgress(backup, copiedBytes, totalBytes);
                    }
                    dailylogs.selectedLogger.Information("Created directory " + directory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                }
                finally
                {
                }
            }
            foreach (var fileName in entryFiles)
            {
                string filePath = Path.Combine(backup.getSourceDirectory(), fileName);
                if (!canBeExecuted || (backup.getState() == "Off"))
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution paused");
                    while (!canBeExecuted || (backup.getState() == "Off"))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (backup.getStopped() == "True")
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution stopped");
                    return;
                }
                string cryptSoftExecutablePath = @"C:\Users\olivi\source\repos\EasySaveConsole\CryptSoft\bin\Debug\net5.0\CryptSoft.exe";
                FileInfo file = new FileInfo(filePath);
                string targetFilePath = Path.Combine(backup.getTargetDirectory(), filePath.Substring(backup.getSourceDirectory().Length + 1));

                try
                {
                    // Lecture et écriture du fichier
                    using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
                    {
                        using (FileStream destinationStream = File.Create(targetFilePath))
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;


                            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                destinationStream.Write(buffer, 0, bytesRead);
                                copiedBytes += bytesRead;

                                // Vérifie si le fichier a une extension qui nécessite un cryptage
                                if (SettingsViewModels.ExtensionCryptoSoft.Contains(file.Extension))
                                {
                                    DateTime start = DateTime.Now;

                                    // Lance le processus de cryptage sur les fichiers avec l'extension
                                    ProcessStartInfo cryptosoft = new ProcessStartInfo(cryptSoftExecutablePath);
                                    cryptosoft.Arguments = "source " + file.FullName + " destination " + targetFilePath;

                                    try
                                    {
                                        var proc = Process.Start(cryptosoft);
                                        proc.WaitForExit();

                                        DateTime end = DateTime.Now;

                                        // Calculer la durée de cryptage
                                        TimeSpan duration = end - start;
                                        durationInSeconds = (int)duration.TotalSeconds;

                                        backup.setCrypt(durationInSeconds);
                                    }
                                    catch (Exception ex)
                                    {
                                        backup.setCrypt(-1);
                                    }
                                }

                                // Calculer et rapporter la progression
                                ReportProgress(backup, copiedBytes, totalBytes);
                            }
                        }
                    }
                    dailylogs.selectedLogger.Information("Copied file " + filePath + " to " + targetFilePath);
                        
                }

                finally
                {
                }
            }

            
            List<string> sortedFiles = Priority(backup.getTargetDirectory());
            // Supprimer les fichiers de la destination qui n'existent plus dans la source
            foreach (string targetFile in Directory.GetFiles(backup.getTargetDirectory(), "*.*", SearchOption.AllDirectories))
            {
                string sourceFile = Path.Combine(backup.getSourceDirectory(), targetFile.Substring(backup.getTargetDirectory().Length + 1));
                if (!File.Exists(sourceFile))
                {
                    File.Delete(targetFile);
                    dailylogs.selectedLogger.Information("Deleted file " + targetFile + " from destination as it no longer exists in source.");
                }
            }
        }

        public static void TypeDifferential(Backup backup)
        {
            long totalBytes = CalculateTotalBytes(backup.getSourceDirectory());
            long copiedBytes = 0;
            string Name = backup.getName();
            string PathSource = backup.getSourceDirectory();
            string PathTarget = backup.getTargetDirectory();
            string State = backup.getState();
            string Stopped = backup.getStopped();
            // Create all directories sequentially
            foreach (var directory in Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories))
            {
                if (!canBeExecuted || (backup.getState() == "Off"))
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution paused");
                    while (!canBeExecuted || (backup.getState() == "Off"))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (backup.getStopped() == "True")
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution stopped");
                    return;
                }
                try
                {
                    Directory.CreateDirectory(directory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                    Interlocked.Add(ref copiedBytes, Directory.GetFiles(directory).Length);
                    ReportProgress(backup, copiedBytes, totalBytes); // Rapporter la progression
                    if (copiedBytes % 1 == 0) 
                    {
                        ReportProgress(backup, copiedBytes, totalBytes);
                    }
                    dailylogs.selectedLogger.Information("Created directory " + directory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                }
                finally
                {
                }
            }

            foreach (var filePath in Directory.GetFiles(backup.getSourceDirectory(), "*.*", SearchOption.AllDirectories))
            {
                if (!canBeExecuted || (backup.getState() == "Off"))
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution paused");
                    while (!canBeExecuted || (backup.getState() == "Off"))
                    {
                        Thread.Sleep(1000);
                    }
                }
                if (backup.getStopped() == "True")
                {
                    dailylogs.selectedLogger.Information("Backup " + backup.getName() + " execution stopped");
                    return;
                }
                FileInfo file = new FileInfo(filePath);
                string targetFilePath = Path.Combine(backup.getTargetDirectory(), filePath.Substring(backup.getSourceDirectory().Length + 1));
                string cryptSoftExecutablePath = @"C:\Users\olivi\source\repos\EasySaveConsole\CryptSoft\bin\Debug\net5.0\CryptSoft.exe";
                try
                {
                    // Vérifie si le fichier existe dans la cible et s'il est plus ancien que celui de la source
                    if (File.Exists(targetFilePath))
                    {
                        FileInfo targetFile = new FileInfo(targetFilePath);
                        if (targetFile.LastWriteTime < file.LastWriteTime)
                        {
                            // Lecture et écriture du fichier
                            // Using pour libérer automatiquement les ressources
                            using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
                            {
                                using (FileStream destinationStream = File.Create(targetFilePath))
                                {
                                    byte[] buffer = new byte[1024];
                                    int bytesRead;


                                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        destinationStream.Write(buffer, 0, bytesRead);
                                        copiedBytes += bytesRead;

                                        // Vérifie si le fichier a une extension qui nécessite un cryptage
                                        if (SettingsViewModels.ExtensionCryptoSoft.Contains(file.Extension))
                                        {
                                            DateTime start = DateTime.Now;

                                            // Lance le processus de cryptage sur les fichiers avec l'extension
                                            ProcessStartInfo cryptosoft = new ProcessStartInfo(cryptSoftExecutablePath);
                                            cryptosoft.Arguments = "source " + file.FullName + " destination " + targetFilePath;

                                            try
                                            {
                                                var proc = Process.Start(cryptosoft);
                                                proc.WaitForExit();

                                                DateTime end = DateTime.Now;

                                                // Calculer la durée de cryptage
                                                TimeSpan duration = end - start;
                                                durationInSeconds = (int)duration.TotalSeconds;

                                                backup.setCrypt(durationInSeconds);
                                            }
                                            catch (Exception ex)
                                            {
                                                backup.setCrypt(-1);
                                            }
                                        }

                                        // Calculer et rapporter la progression
                                        ReportProgress(backup, copiedBytes, totalBytes);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        
                        // Lecture et écriture du fichier
                        using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
                        {
                            using (FileStream destinationStream = File.Create(targetFilePath))
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead;


                                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    destinationStream.Write(buffer, 0, bytesRead);
                                    copiedBytes += bytesRead;

                                    // Vérifie si le fichier a une extension qui nécessite un cryptage
                                    if (SettingsViewModels.ExtensionCryptoSoft.Contains(file.Extension))
                                    {
                                        DateTime start = DateTime.Now;

                                        // Lance le processus de cryptage sur les fichiers avec l'extension
                                        ProcessStartInfo cryptosoft = new ProcessStartInfo(cryptSoftExecutablePath);
                                        cryptosoft.Arguments = "source " + file.FullName + " destination " + targetFilePath;

                                        try
                                        {
                                            var proc = Process.Start(cryptosoft);
                                            proc.WaitForExit();

                                            DateTime end = DateTime.Now;

                                            // Calculer la durée de cryptage
                                            TimeSpan duration = end - start;
                                            durationInSeconds = (int)duration.TotalSeconds;

                                            backup.setCrypt(durationInSeconds);
                                        }
                                        catch (Exception ex)
                                        {
                                            backup.setCrypt(-1);
                                        }
                                    }

                                    // Calculer et rapporter la progression
                                    ReportProgress(backup, copiedBytes, totalBytes);
                                }
                            }
                        }
                    }
                }
                finally
                {
                }
            }

            // Supprimer les fichiers de la destination qui n'existent plus dans la source
            string[] targetFiles = Directory.GetFiles(backup.getTargetDirectory(), "*.*", SearchOption.AllDirectories);
            foreach (string targetFile in targetFiles)
            {
                string sourceFile = Path.Combine(backup.getSourceDirectory(), targetFile.Substring(backup.getTargetDirectory().Length + 1));
                if (!File.Exists(sourceFile))
                {
                    File.Delete(targetFile);
                    dailylogs.selectedLogger.Information("Deleted file " + targetFile + " from destination as it no longer exists in source.");
                }
            }
        }


        public static void DeleteBackupSetting(Backup backupSettings)
        {
            BackupViewModels.BackupListInfo.Remove(backupSettings);
            backupSettings.Remove();
            BackupViewModels.SaveBackupSettings();
            dailylogs.selectedLogger.Information("Backup deleted with success.");
        }

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

                        /*if (progressPercentage != lastProgressPercentage)
                        {
                            // Afficher la progression uniquement si elle a changé
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"Copy progress: {progressPercentage}%");
                            lastProgressPercentage = progressPercentage;
                        }*/
                    }
                    Console.WriteLine(); // Nouvelle ligne après la copie complète
                }
            }

        }

        public static List<string> Priority(string source)
        {
            DirectoryInfo dir = new DirectoryInfo(source);
            List<FileInfo> listToSort = GettingFiles(source);

            List<string> prioprity = new List<string>();
            List<string> prioprityniv2 = new List<string>();

            foreach (FileInfo file in listToSort)
            {
                if (SettingsViewModels.ExtensionPriority.Contains(file.Extension))
                {
                    prioprity.Add(file.FullName.Substring(dir.FullName.Length + 1));
                }
                else
                {
                    prioprityniv2.Add(file.FullName.Substring(dir.FullName.Length + 1));
                }
            }
            prioprity.AddRange(prioprityniv2);
            return prioprity;
        }

        public static List<FileInfo> GettingFiles(string src)
        {
            List<FileInfo> Files = new List<FileInfo>();
            DirectoryInfo directory = new DirectoryInfo(src);

            // Ajouter les fichiers du répertoire actuel
            foreach (FileInfo file in directory.GetFiles())
            {
                Files.Add(file);
            }

            // Parcourir les sous-répertoires récursivement
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDir in subDirectories)
            {
                Files.AddRange(GettingFiles(subDir.FullName));
            }

            return Files;
        }
    }
}
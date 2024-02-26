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
                        // Check if the backup is paused
                        if (!canBeExecuted)
                        {
                            Thread.Sleep(1000); // Wait for 1 second before checking again+promptimpossile
                        }
                        //string directory = backup.getTargetDirectory() + "\\" + backup.getName();
                        //backup.setTargetDirectory(directory);
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

                        if (backup.getType().Equals("Full", StringComparison.OrdinalIgnoreCase) || backup.getType().Equals("Complet", StringComparison.OrdinalIgnoreCase))
                        {
                            TypeComplet(backup);
                            backup.setState("Off");

                        }
                        else
                        {
                            TypeDifferential(backup);
                            backup.setState("Off");
                        }
                    });
                    backupThread.Start();
                    
                }
            });
            foreach (var backupSetting in BackupViewModels.BackupListInfo)
            {
                SettingsViewModels.SetSaveStateBackup(backupSetting.getName(), backupSetting.getSourceDirectory(), backupSetting.getTargetDirectory());
            }

            Console.WriteLine("All selected backups have been launched.");
            // Signal that the backup has completed
            backupCompletedEvent.Set();
        }

        public static void MonitorProcess(ObservableCollection<Backup> backupList)
        {
            // Start the process monitoring thread
            Process[] processes = Process.GetProcessesByName("CalculatorApp");
            if (processes.Length > 0)   // check if a software of the list is running
            {
                canBeExecuted = false;
                foreach (Backup backup in backupList)
                {
                    if (backup.getState() == "On")
                    {
                        // Pause
                        //Thread.Sleep(1000);
                        backup.setState("Off");
                    }
                }
            }
            else
            {
                canBeExecuted = true;
                foreach (Backup backup in backupList)
                {
                    if (backup.getState() == "Off")
                    {
                        backup.setState("On");
                    }
                }
            }

        }

        public static void PauseLauch(Backup backup)
        {
            if (backup.getState() == "On")
            {
                backup.setState("Pause");
                dailylogs.selectedLogger.Information($"Backup {backup.getName()} en pause");
            }
        }

        public static void StopLauch(Backup backup)
        {
            if (backup.getState() == "On")
            {
                backup.setStopped("True");
                backup.setState("Off");
                dailylogs.selectedLogger.Information($"Backup {backup.getName()} arrêté");
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
                string Name = backup.getName();
                string PathSource = backup.getSourceDirectory();
                string PathTarget = backup.getTargetDirectory();
                string State = backup.getState();
                string Stopped = backup.getStopped();
            //long totalBytes = CalculateTotalBytes(PathSource);
            //long copiedBytes = 0;
            // Create all directories concurrently
            Parallel.ForEach(Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (directory) =>
                {
                    while (!canBeExecuted || (State == "Off"))
                    {
                        Thread.Sleep(1000);
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution paused");
                    }
                    if (Stopped == "True")
                    {
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution stopped");
                        return;
                    }
                    try
                        {
                        Directory.CreateDirectory(directory.Replace(PathSource, PathTarget));
                        Interlocked.Add(ref copiedBytes, Directory.GetFiles(directory).Length);
                        if (copiedBytes % 1 == 0) // Mettre à jour toutes les 100 fichiers copiés (par exemple)
                        {
                            ReportProgress(backup, copiedBytes, totalBytes);
                        }
                        ReportProgress(backup, copiedBytes, totalBytes);
                        dailylogs.selectedLogger.Information("Created directory " + directory.Replace(PathSource, PathTarget));
                        }
                    {
                        Directory.CreateDirectory(directory.Replace(PathSource, PathTarget));
                        dailylogs.selectedLogger.Information("Created directory " + directory.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                    }

                });

            Parallel.ForEach(Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (filePath) =>
                {
                    while (!canBeExecuted || (State == "Off"))
                    {
                        Thread.Sleep(1000);
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution paused");
                    }
                    if (Stopped == "True")
                    {
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution stopped");
                        return;
                    }
                    string cryptSoftExecutablePath = @"..\..\EasySavev1\CryptSoft\bin\Debug\net5.0\CryptSoft.exe";
                    FileInfo file = new FileInfo(filePath);
                    string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));

                    try
                    {
                        string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));
                        File.Copy(filePath, filePath.Replace(PathSource, PathTarget), true);
                        // Mettre à jour les compteurs de progression
                        Interlocked.Add(ref copiedBytes, new FileInfo(filePath).Length);
                        double progressPercentage = (double)copiedBytes / totalBytes * 100;

                        // Mettre à jour la barre de progression de la sauvegarde correspondante
                        Backup backup = BackupList.FirstOrDefault(b => b.getName() == Name);
                        if (backup != null)
                        {
                            backup.Progress = (int)progressPercentage;
                        }
                        dailylogs.selectedLogger.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
                        long fileSize = new FileInfo(filePath).Length;
                        Interlocked.Add(ref copiedBytes, fileSize);
                        ReportProgress(backup, copiedBytes, totalBytes);

                    }
                        // Vérifie si le fichier a une extension qui nécessite un cryptage
                        if (SettingsViewModels.ExtensionCryptoSoft.Contains(file.Extension))
                        {
                            //Lance le processus de cryptage sur les fichiers avec l'extension
                            Process cryptProcess = new Process();
                            string args = $"\"{PathSource}\" \"{PathTarget}\"";
                            cryptProcess.StartInfo.FileName = cryptSoftExecutablePath;
                            cryptProcess.StartInfo.Arguments = args;
                            cryptProcess.Start();
                            cryptProcess.WaitForExit();
                        }
                        else
                        {
                            // Sinon, copie simplement le fichier vers la destination
                            File.Copy(filePath, targetFilePath, true);
                            dailylogs.selectedLogger.Information("Copied file " + filePath + " to " + targetFilePath);
                        }
                    }
                    finally
                    {
                    }
                });
            // Supprimer les fichiers de la destination qui n'existent plus dans la source
            string[] targetFiles = Directory.GetFiles(PathTarget, "*.*", SearchOption.AllDirectories);
            foreach (string targetFile in targetFiles)
            {
                string sourceFile = Path.Combine(PathSource, targetFile.Substring(PathTarget.Length + 1));
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
            string Stopped = backup.getStopped(); // Variable pour stocker le pourcentage de progression

            // Créer tous les répertoires en parallèle
            Parallel.ForEach(Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (directory) =>
                {
                    while (!canBeExecuted)
                    {
                        Thread.Sleep(1000);
                        dailylogs.selectedLogger.Information("Backup execution paused due to the CalculatorApp process running.");
                    }
                    try
                    {
                        Directory.CreateDirectory(directory.Replace(PathSource, PathTarget));
                        Interlocked.Add(ref copiedBytes, Directory.GetFiles(directory).Length);
                        if (copiedBytes % 1 == 0) // Mettre à jour toutes les 100 fichiers copiés (par exemple)
                        {
                            ReportProgress(backup, copiedBytes, totalBytes);
                        }
                        ReportProgress(backup, copiedBytes, totalBytes); // Rapporter la progression
                        dailylogs.selectedLogger.Information("Created directory " + directory.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                    }

                });

            // Copier les fichiers modifiés ou nouveaux
            Parallel.ForEach(Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (filePath) =>
                {
                    while (!canBeExecuted)
                    {
                        Thread.Sleep(1000);
                        dailylogs.selectedLogger.Information("Backup execution paused due to the CalculatorApp process running.");
                    }
                    try
                    {
                        string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));

                        if (File.GetLastWriteTimeUtc(filePath) > File.GetLastWriteTimeUtc(targetFilePath))
                        {
                            File.Copy(filePath, filePath.Replace(PathSource, PathTarget), true);
                            // Mettre à jour les compteurs de progression
                            Interlocked.Add(ref copiedBytes, new FileInfo(filePath).Length);
                            double progressPercentage = (double)copiedBytes / totalBytes * 100;

                            // Mettre à jour la barre de progression de la sauvegarde correspondante
                            Backup backup = BackupList.FirstOrDefault(b => b.getName() == Name);
                            if (backup != null)
                            {
                                backup.Progress = (int)progressPercentage;
                            }
                            dailylogs.selectedLogger.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
                            long fileSize = new FileInfo(filePath).Length;
                            Interlocked.Add(ref copiedBytes, fileSize);
                           // ReportProgress(progressPercentage); // Rapporter la progression
                }
                    // Check if the file exists in the target folder
                    if (File.Exists(destinationFilePath))
                    {
                        // Get the last modification date of both files
                        DateTime lastModifiedSource = File.GetLastWriteTime(file);
                        DateTime lastModifiedTarget = File.GetLastWriteTime(destinationFilePath);

                        // Compare the dates
                        if (lastModifiedSource > lastModifiedTarget)
                        {
                            while (!canBeExecuted || (State == "Off"))
                            {
                                Thread.Sleep(1000);
                                dailylogs.selectedLogger.Information("Backup " + Name + " execution paused");
                            }
                            if (Stopped == "True")
                            {
                                dailylogs.selectedLogger.Information("Backup " + Name + " execution stopped");
                                return;
                            }
                            string cryptSoftExecutablePath = @"..\..\EasySavev1\CryptSoft\bin\Debug\net5.0\CryptSoft.exe";
                            FileInfo fileE = new FileInfo(fileName);
                            // Vérifie si le fichier a une extension qui nécessite un cryptage
                            if (SettingsViewModels.ExtensionCryptoSoft.Contains(fileE.Extension))
                            {
                                //Lance le processus de cryptage sur les fichiers avec l'extension
                                Process cryptProcess = new Process();
                                string args = $"\"{PathSource}\" \"{PathTarget}\"";
                                cryptProcess.StartInfo.FileName = cryptSoftExecutablePath;
                                cryptProcess.StartInfo.Arguments = args;
                                cryptProcess.Start();
                                cryptProcess.WaitForExit();
                            }
                            else
                            {
                                // Copy the file from the source location to the target location with progress
                                CopyFileWithProgress(file, destinationFilePath);
                            }
                            dailylogs.selectedLogger.Information($"File '{fileName}' in {PathSource} has been copied to {PathTarget} as it was modified more recently.");
                        }
                        else if (lastModifiedSource < lastModifiedTarget)
                        {
                            dailylogs.selectedLogger.Information($"File '{fileName}' in {PathTarget} was modified after the file in {PathSource}.");
                        }
                        else
                        {
                            dailylogs.selectedLogger.Information("Skipped copying file " + filePath.Replace(PathSource, PathTarget) + ". Source file is not newer.");
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));
                        File.Copy(filePath, targetFilePath, true);
                        // Mettre à jour les compteurs de progression
                        Interlocked.Add(ref copiedBytes, new FileInfo(filePath).Length);
                        double progressPercentage = (double)copiedBytes / totalBytes * 100;

                        // Mettre à jour la barre de progression de la sauvegarde correspondante
                        Backup backup = BackupList.FirstOrDefault(b => b.getName() == Name);
                        if (backup != null)
                        {
                            backup.Progress = (int)progressPercentage;
                        }
                        dailylogs.selectedLogger.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                    }
                });
            // Supprimer les fichiers de la destination qui n'existent plus dans la source
            string[] targetFiles = Directory.GetFiles(PathTarget, "*.*", SearchOption.AllDirectories);
            foreach (string targetFile in targetFiles)
            {
                string sourceFile = Path.Combine(PathSource, targetFile.Substring(PathTarget.Length + 1));
                if (!File.Exists(sourceFile))
                {
                    File.Delete(targetFile);
                    dailylogs.selectedLogger.Information("Deleted file " + targetFile + " from destination as it no longer exists in source.");
                }
            }

                    }
                    else
                    {
                        string cryptSoftExecutablePath = @"..\..\EasySavev1\CryptSoft\bin\Debug\net5.0\CryptSoft.exe";
                        FileInfo fileE = new FileInfo(fileName);
                        // Vérifie si le fichier a une extension qui nécessite un cryptage
                        if (SettingsViewModels.ExtensionCryptoSoft.Contains(fileE.Extension))
                        {
                            //Lance le processus de cryptage sur les fichiers avec l'extension
                            Process cryptProcess = new Process();
                            string args = $"\"{PathSource}\" \"{PathTarget}\"";
                            cryptProcess.StartInfo.FileName = cryptSoftExecutablePath;
                            cryptProcess.StartInfo.Arguments = args;
                            cryptProcess.Start();
                            cryptProcess.WaitForExit();
                        }
                        else
                        {
                            // Copy the file from the source location to the target location with progress
                            CopyFileWithProgress(file, destinationFilePath);
                        }
                        dailylogs.selectedLogger.Information($"File '{fileName}' has been copied from {PathSource} to {PathTarget} as it didn't exist in {PathTarget}.");
                    }
                }
                finally
                {
                }
            });
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
    }
}
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
                            TypeComplet(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getState(), backup.getStopped());
                            backup.setState("Off");

                        }
                        else
                        {
                            TypeDifferential(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getState(), backup.getStopped());
                            backup.setState("Off");
                        }
                    });
                    backupThread.Start();
                    serverViewModels.receiveBackupInfo(backup.getName(), backup.getSourceDirectory(), backup.getTargetDirectory(), backup.getType());
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

        public static void TypeComplet(string Name, string PathSource, string PathTarget, string State, string Stopped)
        {
            // Create all directories concurrently
            Parallel.ForEach(Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (directory) =>
                {
                    while(!canBeExecuted || (State == "Pause"))
                    {
                                Thread.Sleep(1000);
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution paused");
                    }
                    if(Stopped == "True")
                    {
                        dailylogs.selectedLogger.Information("Backup " + Name + " execution stopped");
                        return;
                    }
                    try
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
                    try
                    {
                        string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));
                        File.Copy(filePath, filePath.Replace(PathSource, PathTarget), true);
                        dailylogs.selectedLogger.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                    }
                });
        }
        public static void TypeDifferential(string Name, string PathSource, string PathTarget, string State, string Stopped)
        {
            // Get the list of files in the source folder
            string[] files = Directory.GetFiles(PathSource);

            //Console.WriteLine("Copy progress: ");

            // Parallelize the loop for concurrent file operations
            Parallel.ForEach(files, file =>
            {
                try
                {
                    // Get the file name
                    string fileName = Path.GetFileName(file);

                    // Path to the file in the destination folder
                    string destinationFilePath = Path.Combine(PathTarget, fileName);

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
                            // Copy the file from the source location to the target location with progress
                            CopyFileWithProgress(file, destinationFilePath);
                            dailylogs.selectedLogger.Information($"File '{fileName}' in {PathSource} has been copied to {PathTarget} as it was modified more recently.");
                        }
                        else if (lastModifiedSource < lastModifiedTarget)
                        {
                            dailylogs.selectedLogger.Information($"File '{fileName}' in {PathTarget} was modified after the file in {PathSource}.");
                        }
                        else
                        {
                            dailylogs.selectedLogger.Information($"Files '{fileName}' were modified on the same date.");
                        }
                    }
                    else
                    {
                        // Copy the file from the source location to the target location with progress
                        CopyFileWithProgress(file, destinationFilePath);
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
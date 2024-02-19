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

namespace EasySaveV2.MVVM.ViewModels
{
    class DashboardViewModels
    {
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;
        private static volatile bool isBackupPaused = false;
        private static ManualResetEventSlim backupCompletedEvent = new ManualResetEventSlim(false);

        // Semaphore for the pause
        private static SemaphoreSlim pauseSemaphore = new SemaphoreSlim(1, 1);

        public DashboardViewModels()
        {
        }

        public static void LaunchSlotBackup(List<Backup> backupList)
        {
            Parallel.ForEach(backupList, backup =>
            {
                Thread backupThread = new Thread(() =>
                {
                    // Check if the backup is paused before starting the backup process
                    while (IsBackupPaused())
                    {
                        Thread.Sleep(1000); // Wait for 1 second before checking again
                    }

                    string directory = backup.getTargetDirectory() + "\\" + backup.getName();
                    backup.setTargetDirectory(directory);

                    if (!Directory.Exists(directory))
                    {
                        Parallel.ForEach(Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories),
                            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                            (allDirectory) =>
                            {
                                Directory.CreateDirectory(allDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                dailylogs.selectedLogger.Information("Created directory ", allDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                            });
                    }

                    if (backup.getType().Equals("Full", StringComparison.OrdinalIgnoreCase) || backup.getType().Equals("Complet", StringComparison.OrdinalIgnoreCase))
                    {
                        TypeComplet(backup.getSourceDirectory(), backup.getTargetDirectory());
                    }
                    else
                    {
                        TypeDifferential(backup.getSourceDirectory(), backup.getTargetDirectory());
                    }
                });

                backupThread.Start();
                // backupThread.Join(); // Removed this line to allow parallel execution of backups
            });

            foreach (var backupSetting in BackupViewModels.BackupListInfo)
            {
                SettingsViewModels.SetSaveStateBackup(backupSetting.getName(), backupSetting.getSourceDirectory(), backupSetting.getTargetDirectory());
            }

            Console.WriteLine("All selected backups have been launched.");

            backupCompletedEvent.Set();
        }

        public static void MonitorProcessesAndLaunchBackup(List<Backup> backupList)
        {
            // Start the process monitoring thread
            Thread monitorThread = new Thread(() =>
            {
                while (true)
                {
                    // Check if backup execution is paused
                    if (!IsBackupPaused())
                    {
                        // Check for running processes
                        Process[] processes = Process.GetProcessesByName("CalculatorApp");
                        if (processes.Length > 0)
                        {
                            // Pause backup execution
                            if (!IsBackupPaused())
                            {
                                PauseBackup();
                                dailylogs.selectedLogger.Information("Backup execution paused due to the CalculatorApp process running.");
                            }
                        }
                        else
                        {
                            // Resume backup execution if it was paused
                            if (IsBackupPaused())
                            {
                                ResumeBackup();
                                dailylogs.selectedLogger.Information("Backup execution resumed.");
                            }
                        }
                    }
                    else
                    {
                        ResumeBackup();
                    }

                    // Check if the backup has completed to exit the loop
                    if (backupCompletedEvent.Wait(0))
                    {
                        break;
                    }

                    // Sleep for a short duration before checking again
                    Thread.Sleep(2000); // Adjust the sleep duration as needed
                }
            });

            // Start the monitoring thread
            monitorThread.Start();

            // Continue with the regular backup launch
            LaunchSlotBackup(backupList);

            // Join the monitoring thread to ensure it finishes before the application exits
            monitorThread.Join();
        }

        private static void PauseBackup()
        {
            pauseSemaphore.Wait();
        }

        private static void ResumeBackup()
        {
            pauseSemaphore.Release();
        }

        private static bool IsBackupPaused()
        {
            return pauseSemaphore.CurrentCount == 0;
        }

        public static void TypeComplet(string PathSource, string PathTarget)
        {
            // Use Semaphore to control access to the shared resource
            SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

            // Create all directories concurrently
            Parallel.ForEach(Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (directory) =>
                {
                    semaphore.Wait();
                    try
                    {
                        Directory.CreateDirectory(directory.Replace(PathSource, PathTarget));
                        dailylogs.selectedLogger.Information("Created directory " + directory.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            // Copy files concurrently
            Parallel.ForEach(Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                (filePath) =>
                {
                    semaphore.Wait();
                    try
                    {
                        string targetFilePath = Path.Combine(PathTarget, filePath.Substring(PathSource.Length + 1));
                        File.Copy(filePath, filePath.Replace(PathSource, PathTarget), true);
                        dailylogs.selectedLogger.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            semaphore.Dispose();
        }
        public static void TypeDifferential(string PathSource, string PathTarget)
        {
            // Get the list of files in the source folder
            string[] files = Directory.GetFiles(PathSource);

            //Console.WriteLine("Copy progress: ");

            // Use Semaphore to control access to the shared resource
            SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

            // Parallelize the loop for concurrent file operations
            Parallel.ForEach(files, file =>
            {
                semaphore.Wait();
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
                    semaphore.Release();
                }
            });

            semaphore.Dispose();
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
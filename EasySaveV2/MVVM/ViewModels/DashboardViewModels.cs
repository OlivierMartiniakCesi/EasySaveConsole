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



namespace EasySaveV2.MVVM.ViewModels
{

    class DashboardViewModels
    {
        public static ObservableCollection<Backup> BackupList { get; set; } = BackupViewModels.BackupListInfo;
        private static bool isBackupPaused = false;
        private static ManualResetEventSlim backupCompletedEvent = new ManualResetEventSlim(false);

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
                        // Check if the backup is paused
                        while (IsBackupPaused())
                        {
                            Thread.Sleep(1000); // Wait for 1 second before checking again
                        }

                        // Create directory if it doesn't already exist
                        if (!Directory.Exists(backup.getTargetDirectory()))
                        {
                            // Use Semaphore to control access to the shared resource
                            SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

                            Parallel.ForEach(Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories),
                                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                (AllDirectory) =>
                                {
                                    semaphore.Wait();
                                    try
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                        Log.Information("Created directory ", AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                });

                            semaphore.Dispose();
                        }

                        if (backup.getType().Equals("complet", StringComparison.OrdinalIgnoreCase))
                        {
                            TypeComplet(backup.getSourceDirectory(), backup.getTargetDirectory());
                        }
                        else
                        {
                            TypeDifferential(backup.getSourceDirectory(), backup.getTargetDirectory());
                        }
                    });

                    backupThread.Start();
                    backupThread.Join(); // Wait for the thread to complete before moving to the next backup
                }
            });

            Console.WriteLine("All selected backups have been launched.");

            // Signal that the backup has completed
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
                    if (!isBackupPaused)
                    {
                        // Check for running processes
                        Process[] processes = Process.GetProcessesByName("CalculatorApp");
                        if (processes.Length > 0)
                        {
                            // Pause backup execution
                            if (!isBackupPaused)
                            {
                                PauseBackup();
                                Log.Information("Backup execution paused due to the CalculatorApp process running.");
                            }
                        }
                        else
                        {
                            // Resume backup execution if it was paused
                            if (isBackupPaused)
                            {
                                ResumeBackup();
                                Log.Information("Backup execution resumed.");
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
            isBackupPaused = true;
        }

        private static void ResumeBackup()
        {
            isBackupPaused = false;
        }

        // Méthode pour vérifier si la sauvegarde est en pause
        private static bool IsBackupPaused()
        {
            return isBackupPaused;
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
                        Log.Information("Created directory " + directory.Replace(PathSource, PathTarget));
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
                        Log.Information("Copied file " + filePath.Replace(PathSource, PathTarget));
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
                            Log.Information($"File '{fileName}' in {PathSource} has been copied to {PathTarget} as it was modified more recently.");
                        }
                        else if (lastModifiedSource < lastModifiedTarget)
                        {
                            Log.Information($"File '{fileName}' in {PathTarget} was modified after the file in {PathSource}.");
                        }
                        else
                        {
                            Log.Information($"Files '{fileName}' were modified on the same date.");
                        }
                    }
                    else
                    {
                        // Copy the file from the source location to the target location with progress
                        CopyFileWithProgress(file, destinationFilePath);
                        Log.Information($"File '{fileName}' has been copied from {PathSource} to {PathTarget} as it didn't exist in {PathTarget}.");
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
            Log.Information("Backup deleted with success.");
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
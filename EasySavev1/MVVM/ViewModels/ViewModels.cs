using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveConsole.MVVM.Models;
using EasySaveConsole.MVVM.Views;
//using EasySaveConsole.MVVM.;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Resources;
using System.Reflection;
using System.Globalization;
using EasySaveConsole.Resources;
using System.Xml.Serialization;
using System.Xml;
using System.Threading;
using System.Diagnostics;

namespace EasySaveConsole.MVVM.ViewModels
{
    class ViewModels
    {

        //Gestionnaire de ressources qui facilite l'accès aux ressources
        private static ResourceManager rm = new ResourceManager("EasySaveConsole.Resources.TextEnglish", Assembly.GetExecutingAssembly());
        private static XmlDocument doc;
        private static Backup _backup = new Backup();
        private static dailylogs logs = new dailylogs();
        static string format_logs;
        static string directoryPath = @"C:\JSON";
        static string filePath = @"C:\JSON\confbackup.json";
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        //private static int totalFilesDone = 0;
        const int MaxBackupSettings = 5;
        private static bool isBackupPaused = false;
        private static List<string> menuInterface = new List<string>() { GetTraductor("Create"), GetTraductor("Launch"), GetTraductor("Edit"), GetTraductor("Language"), GetTraductor("Exit") };
        private static string Choice{get; set;}

        private static Vue _vue = new Vue();

        static List<Backup> LoadBackupSettings()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Backup>>(json);
            }
            else
            {
                return new List<Backup>(); // Retourne une nouvelle liste vide si le fichier n'existe pas
            }
        }

        public static void GetJSON()
        {
            if (File.Exists(filePath))
            {
                string fileName = filePath;
                var fileString = File.ReadAllText(fileName);
                var array = JArray.Parse(fileString);

                if (array.Count() > 0)
                {
                    foreach (var item in array)
                    {
                        // Vérifier si toutes les clés nécessaires existent et ne sont pas vides
                        if (item["Name"] != null && !string.IsNullOrEmpty(item["Name"].ToString()) &&
                            item["Source"] != null && !string.IsNullOrEmpty(item["Source"].ToString()) &&
                            item["Target"] != null && !string.IsNullOrEmpty(item["Target"].ToString()) &&
                            item["Type"] != null && !string.IsNullOrEmpty(item["Type"].ToString()))
                        {
                            Backup backup = new Backup(
                                item["Name"].ToString(),
                                item["Source"].ToString(),
                                item["Target"].ToString(),
                                item["Type"].ToString()
                            );
                            try
                            {
                                BackupListInfo.Add(backup);   // CorrectElements
                            }
                            catch
                            {
                                Log.Information($"Erreur lors de l'ajout de données.");
                            }
                        }
                        else
                        {
                            Log.Information($"Élément de données invalide trouvé.");
                        }
                    }
                }
            }
        }
        static void SaveBackupSettings()
        {
            if (BackupListInfo != null && BackupListInfo.Count > 0)
            {
                string jsonText = "[";

                foreach (Backup item in BackupListInfo)
                {
                    jsonText += item.SaveJson() + ",";
                }

                jsonText = jsonText.TrimEnd(',') + "]"; // Remove trailing comma and add closing bracket

                try
                {
                    File.WriteAllText(filePath, jsonText);
                }
                catch (Exception ex)
                {
                    Log.Information("Une erreur est survenue lors de l'enregistrement des paramètres de sauvegarde : " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine(GetTraductor("NoSave"));
            }
        }
        static void DisplayBackupSettings(List<Backup> backupSettings)
        {
            if (backupSettings.Count == 0)
            {
                Console.WriteLine(GetTraductor("ConfBackup"));
            }
            else
            {
                foreach (var setting in backupSettings)
                {
                    Console.WriteLine($"Nom: {setting.getName()}, Source: {setting.getSourceDirectory()}, Destination: {setting.getTargetDirectory()}, Type: {setting.getType()}");
                }
            }
        }

        static void ModifyBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine(GetTraductor("EnterNameBackup"));
            string nameToModify = Console.ReadLine();

            var settingToModify = backupSettings.Find(s => s.getName() == nameToModify);

            if (settingToModify != null)
            {
                Console.WriteLine($"Sauvegarde trouvée : Nom: {settingToModify.getName()}, Source: {settingToModify.getSourceDirectory()}, Destination: {settingToModify.getTargetDirectory()}, Type: {settingToModify.getType()}");

                Console.WriteLine("Entrez le nouveau nom (ou appuyez sur Entrée pour garder le même) :");
                string newName = Console.ReadLine();
                if (!string.IsNullOrEmpty(newName))
                    settingToModify.setName(newName);

                Console.WriteLine("Entrez le nouveau chemin source (ou appuyez sur Entrée pour garder le même) :");
                string newSourcePath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newSourcePath))
                    settingToModify.setSourceDirectory(newSourcePath);

                Console.WriteLine("Entrez le nouveau chemin de destination (ou appuyez sur Entrée pour garder le même) :");
                string newDestinationPath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newDestinationPath))
                    settingToModify.setTargetDirectory(newDestinationPath);

                Console.WriteLine("Entrez le nouveau type de sauvegarde (Complet ou Diff) (ou appuyez sur Entrée pour garder le même) :");
                string typeInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(typeInput))
                    settingToModify.setType(typeInput);

                Log.Information("Sauvegarde modifiée avec succès.");
            }
            else
            {
                Log.Information("Aucune sauvegarde trouvée avec ce nom.");
            }
        }

        static void DeleteBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine("Enter the name of backup to delete :");
            string nameToDelete = Console.ReadLine();

            var settingToDelete = backupSettings.Find(s => s.getName() == nameToDelete);

            if (settingToDelete != null)
            {
                backupSettings.Remove(settingToDelete);
                Log.Information("Backup deleted with success.");
            }
            else
            {
                Log.Information("None backup with this name.");
            }
        }   

        public static void GetStateBackup()
        {
                string json = File.ReadAllText(@"C:\Temp\stateLog.json");    // Read StateLog file
                stateLogList = JsonConvert.DeserializeObject<List<StateLog>>(json == "" ? "[]" : json);
        }

        public static void StateLogs(string name, string fileSource, string fileTarget, long fileSize, string state, int totalFiles, int nbFilesToGet, int crypting, string format_logs)
        {
            StateLog stateLog = new StateLog(name, fileSource, fileTarget, fileSize, state, totalFiles, nbFilesToGet, crypting);
            string stateLogListPath;

            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            }

            if (format_logs.ToLower() == "json")
            {
                stateLogListPath = @"C:\Temp\stateLog.json";
            }
            else
            {
                stateLogListPath = @"C:\Temp\stateLog.xml";
            }

            List<KeyValuePair<string, StateLog>> stateLogList = new List<KeyValuePair<string, StateLog>>();

            // Check if the file exists and has content
            if (File.Exists(stateLogListPath))
            {
                if (format_logs.ToLower() == "json")
                {
                    string json = File.ReadAllText(stateLogListPath);
                    stateLogList = JsonConvert.DeserializeObject<List<KeyValuePair<string, StateLog>>>(json) ?? new List<KeyValuePair<string, StateLog>>();
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, StateLog>>));

                    using (FileStream fs = new FileStream(stateLogListPath, FileMode.Open))
                    {
                        stateLogList = (List<KeyValuePair<string, StateLog>>)serializer.Deserialize(fs) ?? new List<KeyValuePair<string, StateLog>>();
                    }
                }
                if (stateLogList == null)
                {
                    stateLogList = new List<KeyValuePair<string, StateLog>>();
                }
            }
            else
            {
                stateLogList = new List<KeyValuePair<string, StateLog>>();
            }

            // Add or update the entry in the stateLogList
            var existingEntry = stateLogList.FirstOrDefault(entry => entry.Key == name);

            if (existingEntry.Equals(default(KeyValuePair<string, StateLog>)))
            {
                stateLogList.Add(new KeyValuePair<string, StateLog>(name, stateLog));
            }
            else
            {
                stateLogList[stateLogList.IndexOf(existingEntry)] = new KeyValuePair<string, StateLog>(name, stateLog);
            }

            // Write the entire dictionary to the file
            if (format_logs.ToLower() == "json")
            {
                string jsonToWrite = JsonConvert.SerializeObject(stateLogList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(stateLogListPath, jsonToWrite);
            }
            else
            {
                stateLogListPath = @"C:\Temp\stateLog.xml";

                XmlDocument doc = new XmlDocument();

                // Check if the file exists and has content
                if (File.Exists(stateLogListPath))
                {
                    doc.Load(stateLogListPath);

                    // Find existing entry
                    XmlElement node = (XmlElement)doc.SelectSingleNode($"/ArrayOfKeyValuePairOfStringStateLog/KeyValuePairOfStringStateLog[Name='{name}']");

                    if (node != null)
                    {
                        // Update existing node
                        node.SelectSingleNode("Name").InnerText = name;
                        node.SelectSingleNode("FileSource").InnerText = fileSource;
                        node.SelectSingleNode("FileTarget").InnerText = fileTarget;
                        node.SelectSingleNode("FileSize").InnerText = fileSize.ToString();
                        node.SelectSingleNode("TotalFiles").InnerText = totalFiles.ToString();
                        node.SelectSingleNode("NbFilesToGet").InnerText = nbFilesToGet.ToString();
                        node.SelectSingleNode("State").InnerText = state;
                        node.SelectSingleNode("Crypting").InnerText = crypting.ToString();
                    }
                    else
                    {
                        // Create new entry
                        XmlElement log = doc.CreateElement("KeyValuePairOfStringStateLog");

                        XmlElement nameXML = doc.CreateElement("Name");
                        XmlElement fileSourceXML = doc.CreateElement("FileSource");
                        XmlElement fileTargetXML = doc.CreateElement("FileTarget");
                        XmlElement fileSizeXML = doc.CreateElement("FileSize");
                        XmlElement totalFilesXML = doc.CreateElement("TotalFiles");
                        XmlElement nbFilesToGetXML = doc.CreateElement("NbFilesToGet");
                        XmlElement stateXML = doc.CreateElement("State");
                        XmlElement cryptingXML = doc.CreateElement("Crypting");

                        nameXML.InnerText = name;
                        fileSourceXML.InnerText = fileSource;
                        fileTargetXML.InnerText = fileTarget;
                        fileSizeXML.InnerText = fileSize.ToString();
                        totalFilesXML.InnerText = totalFiles.ToString();
                        nbFilesToGetXML.InnerText = nbFilesToGet.ToString();
                        stateXML.InnerText = state;
                        cryptingXML.InnerText = crypting.ToString();

                        log.AppendChild(nameXML);
                        log.AppendChild(fileSourceXML);
                        log.AppendChild(fileTargetXML);
                        log.AppendChild(fileSizeXML);
                        log.AppendChild(totalFilesXML);
                        log.AppendChild(nbFilesToGetXML);
                        log.AppendChild(stateXML);
                        log.AppendChild(cryptingXML);

                        doc.DocumentElement?.AppendChild(log);
                    }

                    doc.Save(stateLogListPath);
                }
                else
                {
                    // File doesn't exist, create new XML file
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, StateLog>>));
                    using (FileStream fs = new FileStream(stateLogListPath, FileMode.Create))
                    {
                        serializer.Serialize(fs, new List<KeyValuePair<string, StateLog>>());
                    }
                }
            }
        }



        public static void SetSaveStateBackup(string backupName, string src, string dest)
        {
            var dir = new DirectoryInfo(src);
            DirectoryInfo[] dirs = dir.GetDirectories();  // Cache directories before we start copying

            Directory.CreateDirectory(dest);

            int totalFilesDone = 0;  // Initialize totalFilesDone here

            foreach (FileInfo file in dir.GetFiles())
            {
                totalFilesDone++;
                file.CopyTo(Path.Combine(dest, file.Name), true);   // Copy the file into the destination directory
                string fileSrc = Path.Combine(file.DirectoryName, file.Name);
                string fileDest = Path.Combine(dest, file.Name);
                int totalFiles = Directory.GetFiles(src, "*", SearchOption.AllDirectories).Length;
                StateLogs(backupName, fileSrc, fileDest, file.Length, "On", totalFiles, totalFilesDone, 0, format_logs);
            }
        }
        
        public static int mainInterface()
        {
            do
            {
                Console.WriteLine("Before lauch the application, in which format would you have your logs ? (json / xml)");
                format_logs = Console.ReadLine();
            } while (format_logs.ToLower() != "json" && format_logs.ToLower() != "xml");
            logs.Logsjson(format_logs);
            Log.Information("Application started successfully");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            GetJSON();
            while (true)
            {
                switch (_vue.SelectMenu(menuInterface))
                {
                    case 1:
                        if (BackupListInfo.Count < MaxBackupSettings)
                        {
                            CreateSlotBackup();
                            Console.Clear();
                        }
                        else
                        {
                            Console.WriteLine(GetTraductor("MaxBackup"));
                        }
                        break;
                    case 2:
                        LaunchSlotBackup(BackupListInfo);
                        foreach (var backupSetting in BackupListInfo)
                        {
                            SetSaveStateBackup(backupSetting.getName(), backupSetting.getSourceDirectory(), backupSetting.getTargetDirectory());
                        }
                        break;
                    case 3:
                        DisplayBackupSettings(BackupListInfo);
                        ModifyBackupSetting(BackupListInfo);
                        Console.Clear();
                        break;
                    case 4:
                        Lang();
                        Console.Clear();
                        break;
                    case 5:
                        Console.WriteLine(GetTraductor("AppClose"));
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine(GetTraductor("NoValid"));
                        break;
                }
                SaveBackupSettings();
            }
        }

        static void CreateSlotBackup()
        {
            Console.WriteLine(GetTraductor("EnterNewName"));
            string name = Console.ReadLine();

            Console.WriteLine(GetTraductor("PathSrc"));
            string sourcePath = Console.ReadLine();

            Console.WriteLine(GetTraductor("PathDst"));
            string destinationPath = Console.ReadLine();
            destinationPath = destinationPath + @"\" + name;

            string type; // Variable pour stocker le type

            do
            {
                Console.WriteLine(GetTraductor("TypeBackup"));
                type = Console.ReadLine().ToLower();

                if (type != "complet" && type != "diff")
                {
                    Console.WriteLine(GetTraductor("InvalidTypeBackup"));
                }
            } while (type != "complet" && type != "diff");


            BackupListInfo.Add(_backup.CreateBackup(name, sourcePath,destinationPath,type));
        }

        public static void LaunchSlotBackup(List<Backup> backupList)
        {
            Console.WriteLine("Liste des sauvegardes disponibles :");
            foreach (Backup backup in backupList)
            {
                Console.WriteLine(backup.getName());
            }

            Console.Write("Entrez les noms des sauvegardes à lancer (séparés par des virgules) : ");
            string inputNames = Console.ReadLine();

            // Séparer les noms entrés par l'utilisateur
            string[] selectedNames = inputNames.Split(',');

            // Utiliser Parallel.ForEach pour traiter chaque sauvegarde dans un thread séparé
            Parallel.ForEach(backupList, backup =>
            {
                if (selectedNames.Contains(backup.getName()))
                {
                    Console.WriteLine($"Lancement de la sauvegarde : {backup.getName()}");

                    // Utiliser un thread séparé pour chaque sauvegarde
                    Thread backupThread = new Thread(() =>
                    {
                        // Vérifier si la sauvegarde est en pause
                        while (IsBackupPaused())
                        {
                            Thread.Sleep(1000); // Attendre 1 seconde avant de vérifier à nouveau
                        }

                        // Créer le répertoire s'il n'existe pas déjà
                        if (!Directory.Exists(backup.getTargetDirectory()))
                        {
                            // Utiliser Semaphore pour contrôler l'accès à la ressource partagée
                            SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

                            Parallel.ForEach(Directory.GetDirectories(backup.getSourceDirectory(), "*", SearchOption.AllDirectories),
                                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
                                (AllDirectory) =>
                                {
                                    semaphore.Wait();
                                    try
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
                                        Log.Information("Création du répertoire ", AllDirectory.Replace(backup.getSourceDirectory(), backup.getTargetDirectory()));
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
                    backupThread.Join(); // Attendre que le thread se termine avant de passer à la sauvegarde suivante
                }
            });

            Console.WriteLine("Toutes les sauvegardes sélectionnées ont été lancées.");
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
                        Process[] processes = Process.GetProcesses();
                        foreach (Process process in processes)
                        {
                            try
                            {
                                // Check if the process is a business software (.exe)
                                if (process.MainModule.ModuleName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Pause backup execution
                                    PauseBackup();
                                    Log.Information($"Backup execution paused due to the launch of {process.MainModule.ModuleName}.");
                                    break;
                                }
                                else
                                {
                                    if (!isBackupPaused)
                                    {
                                        ResumeBackup();
                                    }
                                }

                            }
                            catch (Exception)
                            {
                                // Handle exceptions when accessing process information
                            }
                        }
                    }

                    // Sleep for a short duration before checking again
                    Thread.Sleep(5000); // Adjust the sleep duration as needed
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
                        Log.Information("Created directory "+ directory.Replace(PathSource, PathTarget));
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
                        Log.Information("Copied file "+ filePath.Replace(PathSource, PathTarget));
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

            semaphore.Dispose();
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

                        if (progressPercentage != lastProgressPercentage)
                        {
                            // Afficher la progression uniquement si elle a changé
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"Copy progress: {progressPercentage}%");
                            lastProgressPercentage = progressPercentage;
                        }
                    }
                    Console.WriteLine(); // Nouvelle ligne après la copie complète
                }
            }
        }

        public static string GetTraductor(string word) => rm.GetString(word);

        public static void Change(string language)
        {
            rm = new ResourceManager("EasySaveConsole.Resources.Text" + language, Assembly.GetExecutingAssembly());
        }

        public static void TypeDifferential(string PathSource, string PathTarget)
        {
            // Get the list of files in the source folder
            string[] files = Directory.GetFiles(PathSource);

            Console.WriteLine("Copy progress: ");

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

        public static void Lang()
        {
            Console.WriteLine(GetTraductor("enterLanguage"));
            string language = Console.ReadLine();
            language = language.ToLower();
            if (language == "french")
                Change("French");
            else if (language == "english")
                Change("English");
            else
                Console.WriteLine("Error");

            menuInterface = new List<string>() { GetTraductor("Create"), GetTraductor("Launch"), GetTraductor("Edit"), GetTraductor("Language"), GetTraductor("Exit") };
            
        }
    }
}



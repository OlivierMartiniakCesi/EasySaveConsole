﻿using System;
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



namespace EasySaveConsole.MVVM.ViewModels
{
    class ViewModels
    {

        //Gestionnaire de ressources qui facilite l'accès aux ressources
        private static ResourceManager rm = new ResourceManager("EasySaveConsole.Resources.TextEnglish", Assembly.GetExecutingAssembly());

        private static Backup _backup = new Backup();
        private static dailylogs logs = new dailylogs();
        static string directoryPath = @"C:\JSON";
        static string filePath = @"C:\JSON\confbackup.json";
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        //private static int totalFilesDone = 0;
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

        public static void StateLogs(string name, string fileSource, string fileTarget, long fileSize, string state, int totalFiles, int nbFilesToGet)
        {
            StateLog stateLog = new StateLog(name, fileSource, fileTarget, fileSize, state, totalFiles, nbFilesToGet);

            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            }

        string stateLogListPath = @"C:\Temp\stateLog.json";

            List<KeyValuePair<string, StateLog>> stateLogList;

            // Check if the file exists and has content
            if (File.Exists(stateLogListPath))
            {
                string json = File.ReadAllText(stateLogListPath);

                stateLogList = JsonConvert.DeserializeObject<List<KeyValuePair<string, StateLog>>>(json);

                if (stateLogList == null)
                {
                    stateLogList = new List<KeyValuePair<string, StateLog>>();
                }
            }
            else
            {
                stateLogList = new List<KeyValuePair<string, StateLog>>();
            }

            // Replace or add the entry for the given name
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
            string jsonToWrite = JsonConvert.SerializeObject(stateLogList, Formatting.Indented);
            File.WriteAllText(stateLogListPath, jsonToWrite);
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
                StateLogs(backupName, fileSrc, fileDest, file.Length, "On", totalFiles, totalFilesDone);
            }
        }
        
        public static int mainInterface()
        {
            logs.Logsjson();
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
                        {
                            CreateSlotBackup();
                            Console.Clear();
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

            Console.Write("Entrez le nom de la sauvegarde à lancer : ");
            string Name = Console.ReadLine();

            Backup selectedBackup = backupList.FirstOrDefault(backup => backup.getName() == Name);

            //Create directory if it doesn't already exist
            if (!Directory.Exists(selectedBackup.getTargetDirectory()))
            {
                foreach (string AllDirectory in Directory.GetDirectories(selectedBackup.getSourceDirectory(), "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(AllDirectory.Replace(selectedBackup.getSourceDirectory(), selectedBackup.getTargetDirectory()));
                }
                Log.Information("Creation of directory ", selectedBackup.getTargetDirectory());
            }

            if (selectedBackup != null)
            {
                if (selectedBackup.getType() == "complet" || selectedBackup.getType() == "Complet")
                {
                    TypeComplet(selectedBackup.getSourceDirectory(), selectedBackup.getTargetDirectory());
                }
                else
                {
                    TypeDifferential(selectedBackup.getSourceDirectory(), selectedBackup.getTargetDirectory());
                }
            }
            else
            {
                Log.Information("Sauvegarde non trouvée.");
            }
        }
        public static void TypeComplet(string PathSource, string PathTarget)
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
            // Obtenir la liste des fichiers dans le premier dossier
            string[] files = Directory.GetFiles(PathSource);

            Console.WriteLine("Copy progress: ");

            // Parcourir chaque fichier dans le dossier 1
            foreach (string filePath1 in files)
            {
                // Obtenir le nom du fichier
                string fileName = Path.GetFileName(filePath1);

                // Chemin vers le fichier dans le dossier destination
                string filePath2 = Path.Combine(PathTarget, fileName);

                // Vérifie si le fichier existe dans le dossier 2
                if (File.Exists(filePath2))
                {
                    // Obtenir la date de dernière modification des deux fichiers
                    DateTime lastModified1 = File.GetLastWriteTime(filePath1);
                    DateTime lastModified2 = File.GetLastWriteTime(filePath2);

                    // Comparer les dates
                    if (lastModified1 > lastModified2)
                    {
                        // Copier le fichier du premier emplacement vers le deuxième emplacement avec la progression
                        CopyFileWithProgress(filePath1, filePath2);
                        Log.Information($"Le fichier '{fileName}' dans {PathSource} a été copié vers {PathTarget} car il a été modifié plus récemment.");
                    }
                    else if (lastModified1 < lastModified2)
                    {
                        Log.Information($"Le fichier '{fileName}' dans {PathTarget} a été modifié après le fichier dans {PathSource}.");
                    }
                    else
                    {
                        Log.Information($"Les fichiers '{fileName}' ont été modifiés à la même date.");
                    }
                }
                else
                {
                    // Copier le fichier du premier emplacement vers le deuxième emplacement avec la progression
                    CopyFileWithProgress(filePath1, filePath2);
                    Log.Information($"Le fichier '{fileName}' a été copié de {PathSource} vers {PathTarget} car il n'existait pas dans {PathTarget}.");
                }
            }
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



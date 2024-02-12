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
        static string filePath = Path.Combine(directoryPath, "confbackup.json");
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        //private static int totalFilesDone = 0;
        const int MaxBackupSettings = 5;
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

        static void SaveBackupSettings(List<Backup> backupSettings)
        {
            if (backupSettings != null)
            {
                string json = JsonConvert.SerializeObject(backupSettings, Formatting.Indented);
                File.WriteAllText(filePath, json);
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
                    Console.WriteLine($"Nom: {setting.getName()}, Source: {setting.getSourceDirectory()}, Destination: {setting.getTargetDirectory()}, Type: {setting.GetType()}");
                }
            }
        }

        static void ModifyBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine(GetTraductor("EnterNameBackup"));
            string nameToModify = Console.ReadLine();

            var settingToModify = backupSettings.Find(s => s.getName() == nameToModify);
        }

        static void DeleteBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine("Enter the name of backup to delete :");
            string nameToDelete = Console.ReadLine();

            var settingToDelete = backupSettings.Find(s => s.getName() == nameToDelete);

            if (settingToDelete != null)
            {
                backupSettings.Remove(settingToDelete);
                Console.WriteLine("Backup deleted with success.");
            }
            else
            {
                Console.WriteLine("None backup with this name.");
            }
        }   

        public static void GetStateBackup()
        {
                string json = File.ReadAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json");    // Read StateLog file
                stateLogList = JsonConvert.DeserializeObject<List<StateLog>>(json == "" ? "[]" : json);
        }

        public static void StateLogs(string name, string fileSource, string fileTarget, long fileSize, string state, int totalFiles, int nbFilesToGet)
        {
            StateLog stateLog = new StateLog(name, fileSource, fileTarget, fileSize, state, totalFiles, nbFilesToGet);

            if (!Directory.Exists(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json"))
            {
                Directory.CreateDirectory(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json");
            }

        string stateLogListPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json";

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

            List<Backup> backupSettings = LoadBackupSettings();

            while (true)
            {
                switch (_vue.SelectMenu(menuInterface))
                {
                    case 1:
                        if (backupSettings.Count < MaxBackupSettings)
                        {
                            CreateSlotBackup(backupSettings);
                            Console.Clear();
                        }
                        else
                        {
                            Console.WriteLine(GetTraductor("MaxBackup"));
                        }
                        break;
                    case 2:
                        foreach (var backupSetting in backupSettings)
                        {
                            SetSaveStateBackup(backupSetting.getName(), backupSetting.getSourceDirectory(), backupSetting.getTargetDirectory());
                        }
                        break;
                    case 3:
                        ModifyBackupSetting(backupSettings);
                        Console.Clear();
                        break;
                    case 4:
                        Lang();
                        Console.Clear();
                        break;
                    case 5:
                        SaveBackupSettings(backupSettings);
                        Console.WriteLine(GetTraductor("AppClose"));
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine(GetTraductor("NoValid"));
                        break;
                }
            }
        }

        static void CreateSlotBackup(List<Backup> backupSettings)
        {
            Console.WriteLine(GetTraductor("EnterNewName"));
            string name = Console.ReadLine();

            Console.WriteLine(GetTraductor("PathSrc"));
            string sourcePath = Console.ReadLine();

            Console.WriteLine(GetTraductor("PathDst"));
            string destinationPath = Console.ReadLine();

            Console.WriteLine(GetTraductor("TypeBackup"));
            string type = Console.ReadLine();
            type = type.ToLower();

            // Utilisation de l'opérateur && pour vérifier que le type n'est ni "Complet" ni "Differentielle"
            if (type != "complet" && type != "diff")
            {
                Console.WriteLine(GetTraductor("TypeBackup"));
                type = Console.ReadLine();
                type = type.ToLower();
            }

            BackupListInfo.Add(_backup.CreateBackup(name, sourcePath, destinationPath, type));
        }

        public static void LaunchSlotBackup(Backup backup)
        {
            foreach (Backup backup1 in BackupListInfo)
            {

                if (backup.getType() == "complet")
                {
                    TypeComplet(backup.getSourceDirectory(), backup.getTargetDirectory());
                }
                else
                {
                    TypeDifferential(backup1.getSourceDirectory(), backup1.getTargetDirectory());
                }
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
                        Console.WriteLine($"Le fichier '{fileName}' dans {PathSource} a été copié vers {PathTarget} car il a été modifié plus récemment.");
                    }
                    else if (lastModified1 < lastModified2)
                    {
                        Console.WriteLine($"Le fichier '{fileName}' dans {PathTarget} a été modifié après le fichier dans {PathSource}.");
                    }
                    else
                    {
                        Console.WriteLine($"Les fichiers '{fileName}' ont été modifiés à la même date.");
                    }
                }
                else
                {
                    // Copier le fichier du premier emplacement vers le deuxième emplacement avec la progression
                    CopyFileWithProgress(filePath1, filePath2);
                    Console.WriteLine($"Le fichier '{fileName}' a été copié de {PathSource} vers {PathTarget} car il n'existait pas dans {PathTarget}.");
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



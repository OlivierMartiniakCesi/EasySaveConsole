using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveConsole.MVVM.Models;
//using EasySaveConsole.MVVM.;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace EasySaveConsole.MVVM.ViewModels
{
    class ViewModels
    {
        private static Backup _backup = new Backup();
        private static daylylogs logs = new daylylogs();
        static string directoryPath = @"C:\JSON";
        static string filePath = Path.Combine(directoryPath, "confbackup.json");
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        private static int totalFilesDone = 0;
        private static string Choice{get; set;}

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
                Console.WriteLine("Les sauvegardes ont été enregistrées avec succès dans le fichier JSON.");
            }
            else
            {
                Console.WriteLine("Aucune sauvegarde à enregistrer.");
            }
        }
        static void DisplayBackupSettings(List<Backup> backupSettings)
        {
            if (backupSettings.Count == 0)
            {
                Console.WriteLine("Aucune sauvegarde n'est actuellement configurée.");
            }
            else
            {
                foreach (var setting in backupSettings)
                {
                    Console.WriteLine($"Nom: {setting.getName()}, Source: {setting.getSourceDirectory()}, Destination: {setting.getTargetDirectory()}, Type: {setting.GetType()}");
                }
            }
        }
        static void AddNewBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine("Entrez le nom de la nouvelle sauvegarde :");
            string name = Console.ReadLine();

            Console.WriteLine("Entrez le chemin source :");
            string sourcePath = Console.ReadLine();

            Console.WriteLine("Entrez le chemin de destination :");
            string destinationPath = Console.ReadLine();

            Console.WriteLine("Entrez le type de sauvegarde (Full ou Differential) :");
            string type = Console.ReadLine();
            if (type == "Full")
            {
                if (backupSettings == null)
                {
                    backupSettings = new List<Backup>();
                }
                backupSettings.Add(new Backup { Name = name, SourceDirectory = sourcePath, TargetDirectory = destinationPath, Type = type });
            }
            else
            {
                Console.WriteLine("Type de sauvegarde invalide.");
            }
        }

        static void ModifyBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine("Entrez le nom de la sauvegarde à modifier :");
            string nameToModify = Console.ReadLine();

            var settingToModify = backupSettings.Find(s => s.Name == nameToModify);

            if (settingToModify != null)
            {
                Console.WriteLine($"Sauvegarde trouvée : Nom: {settingToModify.Name}, Source: {settingToModify.SourceDirectory}, Destination: {settingToModify.TargetDirectory}, Type: {settingToModify.Type}");

                Console.WriteLine("Entrez le nouveau nom (ou appuyez sur Entrée pour garder le même) :");
                string newName = Console.ReadLine();
                if (!string.IsNullOrEmpty(newName))
                    settingToModify.Name = newName;

                Console.WriteLine("Entrez le nouveau chemin source (ou appuyez sur Entrée pour garder le même) :");
                string newSourcePath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newSourcePath))
                    settingToModify.SourceDirectory = newSourcePath;

                Console.WriteLine("Entrez le nouveau chemin de destination (ou appuyez sur Entrée pour garder le même) :");
                string newDestinationPath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newDestinationPath))
                    settingToModify.TargetDirectory = newDestinationPath;

                Console.WriteLine("Entrez le nouveau type de sauvegarde (Full ou Differential) (ou appuyez sur Entrée pour garder le même) :");
                string typeInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(typeInput))
                {
                    string newType = Console.ReadLine();
                    if (newType == "Full")
                    {
                        settingToModify.Type = newType;
                    }
                }

                Console.WriteLine("Sauvegarde modifiée avec succès.");
            }
            else
            {
                Console.WriteLine("Aucune sauvegarde trouvée avec ce nom.");
            }
        }
        static void DeleteBackupSetting(List<Backup> backupSettings)
        {
            Console.WriteLine("Entrez le nom de la sauvegarde à supprimer :");
            string nameToDelete = Console.ReadLine();

            var settingToDelete = backupSettings.Find(s => s.Name == nameToDelete);

            if (settingToDelete != null)
            {
                backupSettings.Remove(settingToDelete);
                Console.WriteLine("Sauvegarde supprimée avec succès.");
            }
            else
            {
                Console.WriteLine("Aucune sauvegarde trouvée avec ce nom.");
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
            bool exit = false;
            logs.Logsjson();
            int IChoice;
            List<Backup> backupSettings = LoadBackupSettings();

            Log.Information("Application started successfully");
            
            Console.WriteLine(" ### ###    ##      ## ##   ##  ##    ## ##     ##     ### ###  ### ###");
            Console.WriteLine("  ##  ##     ##    ##   ##  ##  ##   ##   ##     ##     ##  ##   ##  ##");
            Console.WriteLine("  ##       ## ##   ####     ##  ##   ####      ## ##    ##  ##   ##    ");
            Console.WriteLine("  ## ##    ##  ##   #####    ## ##    #####    ##  ##   ##  ##   ## ## ");
            Console.WriteLine("  ##       ## ###      ###    ##         ###   ## ###   ### ##   ##    ");
            Console.WriteLine("  ##  ##   ##  ##  ##   ##    ##     ##   ##   ##  ##    ###     ##  ##");
            Console.WriteLine(" ### ###  ###  ##   ## ##     ##      ## ##   ###  ##     ##    ### ###");

            Console.WriteLine("\n\n");

            Console.WriteLine("\n1- Create");
            Console.WriteLine("\n2- Launch");
            Console.WriteLine("\n3- Edit");
            Console.WriteLine("\n4- Language");
            Console.WriteLine("\n5- Exit");

            Choice = Console.ReadLine();
            IChoice = int.Parse(Choice);


            while (exit == false)
            {
                GetStateBackup();
                if (backupSettings != null)
                {
                    foreach (var backupSetting in backupSettings)
                    {
                        SetSaveStateBackup(backupSetting.Name, backupSetting.SourceDirectory, backupSetting.TargetDirectory);
                    }
                }
                switch (IChoice)
                {
                    case 1:
                        CreateSlotBackup();
                        break;
                    case 2:
                        //LaunchSlotBackup(BackupListInfo[]);
                        break;
                    case 3:
                        // dcez
                        break;
                    case 4:
                        // dzead
                        break;
                    case 5:
                        exit = true;
                        Log.Information("Application closed successfully");
                        Log.CloseAndFlush();
                        Environment.Exit(0);
                        break;

                }
            }
            return 0;
        }

        public static void CreateSlotBackup()
        {
            Console.WriteLine("Veuillez saisir le nom de la sauvegarde.");
            string Name = Console.ReadLine();

            Console.WriteLine("Veuillez saisir le chemin source de la sauvegarde.");
            string pathSource = Console.ReadLine();

            Console.WriteLine("Veuillez saisir le chemin cible de la sauvegarde.");
            string pathTarget = Console.ReadLine();

            Console.WriteLine("Veuillez saisir le type de sauvegarde (Complet / Differential).");
            string types = Console.ReadLine();

            if (types != "Complet" || types != "Differential")
            {
                Console.WriteLine("Veuillez saisir le bon type de sauvegarde -> (Complete / Differenciel).");
                types = Console.ReadLine();
            }

            BackupListInfo.Add(_backup.CreateBackup(Name, pathSource, pathTarget, types));
        }

        public static void LaunchSlotBackup(Backup backup)
        {
            foreach (Backup backup1 in BackupListInfo)
            {

                if (backup.getType() == "Complet")
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
                            Console.Write($"Progression de la copie: {progressPercentage}%");
                            lastProgressPercentage = progressPercentage;
                        }
                    }
                    Console.WriteLine(); // Nouvelle ligne après la copie complète
                }
            }
        }
        public static void TypeDifferential(string PathSource, string PathTarget)
        {
            // Obtenir la liste des fichiers dans le premier dossier
            string[] files = Directory.GetFiles(PathSource);

            Console.WriteLine("Progression de la copie: ");

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
        private static void StartMethod(string ChoiceMethod)
        {
            int index = 0;
            List<string> slotNameList = BackupListInfo.Select(_backup => _backup.getName()).ToList();
            Console.WriteLine("Sélectionner la sauvegarde");

            switch (ChoiceMethod)
            {
                case "Launch":
                    //LaunchSlotBackup(BackupListInfo);
                    break;
                case "Edit":
                    // Add your code for the "Edit" case here
                    break;
                    // Add more cases if needed
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySavev1.MVVM.Models;
using EasySavev1.MVVM.Views;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace EasySavev1.MVVM.ViewModels
{
    class ViewModels
    {
        private static Backup _backup = new Backup();
        private static daylylogs logs = new daylylogs();
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        private static string Choice{get; set;}

        public static void GetSaveBackup()
        {
            string fileName = @"C:\backup\backuplist.json";
            var fileString = File.ReadAllText(fileName);
            var array = JArray.Parse(fileString);

            if (array.Count() > 0)
            {
                foreach (var item in array)
                {
                    Backup backup = new Backup(
                        item["name"].ToString(),
                        item["PathSource"].ToString(),
                        item["PathTarget"].ToString(),
                        item["type"].ToString()
                    );
                    try
                    {
                        backupList.Add(backup);   // CorrectElements
                    }
                    catch
                    {
                        Console.WriteLine($"error in data");
                    }
                }
            }

            string json = File.ReadAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json");    // Read StateLog file
            stateLogList = JsonConvert.DeserializeObject<List<StateLog>>(json == "" ? "[]" : json);
        }

        public static void StateLogs(string name, string fileSource, string fileTarget, long fileSize, int totalFiles, int totalFilesDone)
        {
            StateLog stateLoglist = new StateLog(name, fileSource, fileTarget, fileSize, totalFiles, totalFilesDone);
            string stateLogListPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\stateLog.json";
            string json = JsonConvert.SerializeObject(stateLogList, Formatting.Indented);
            File.WriteAllText(stateLogListPath, json);
        }

        public static void SetSaveStateBackup(string backupName, string src, string dest)
        {
            var dir = new DirectoryInfo(src);
            DirectoryInfo[] dirs = dir.GetDirectories();  // Cache directories before we start copying

            Directory.CreateDirectory(dest);

            foreach (FileInfo file in dir.GetFiles())
            {
             
                    totalFilesDone++;
                    file.CopyTo(Path.Combine(dest, file.Name), true);   // Copy the file into the destination directory
                    string fileSrc = Path.Combine(file.DirectoryName, file.Name);
                    string fileDest = Path.Combine(dest, file.Name);
                    int totalFiles = Directory.GetFiles(src, "*", SearchOption.AllDirectories).Length;
                    StateLogs(backupName, fileSrc, fileDest, file.Length, totalFiles, totalFilesDone);
             
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
                GetSaveBackup();
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
                        LaunchSlotBackup(BackupListInfo[]);
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
        public static void GetSaveBackup()
        {
            string fileName = @"C:\backup\backuplist.json";
            var fileString = File.ReadAllText(fileName);
            var array = JArray.Parse(fileString);

            if (array.Count() > 0)
            {
                foreach (var item in array)
                {
                    Backup backup = new Backup(
                        item["name"].ToString(),
                        item["PathSource"].ToString(),
                        item["PathTarget"].ToString(),
                        item["type"].ToString()
                    );
                    try
                    {
                        BackupListInfo.Add(backup);   // CorrectElements
                    }
                    catch
                    {
                        Console.WriteLine($"error in data");
                    }
                }
            }

            string json = File.ReadAllText(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\Logs\statelog.json");    // Read StateLog file
            //stateLogList = JsonConvert.DeserializeObject<List<StateLog>>(json == "" ? "[]" : json);
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
                    LaunchSlotBackup(BackupListInfo);
                    break;
                case "Edit":
                    // Add your code for the "Edit" case here
                    break;
                    // Add more cases if needed
            }
        }
    }
}

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
        private static List<string> menuInterface = new List<string>() { GetTraductor("Create"), GetTraductor("Launch"), GetTraductor("Edit"), GetTraductor("Delete"), GetTraductor("Language"), GetTraductor("Exit") };
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
                        if (item["Id"] != null && !string.IsNullOrEmpty(item["Id"].ToString()) &&
                            item["Name"] != null && !string.IsNullOrEmpty(item["Name"].ToString()) &&
                            item["Source"] != null && !string.IsNullOrEmpty(item["Source"].ToString()) &&
                            item["Target"] != null && !string.IsNullOrEmpty(item["Target"].ToString()) &&
                            item["Type"] != null && !string.IsNullOrEmpty(item["Type"].ToString()))
                        {
                            Backup backup = new Backup(
                                item["Id"].ToString(),
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
            else
            {
                Log.Information($"Élément de données invalide trouvé.");
                File.WriteAllText(filePath, "[]");
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
                    Console.WriteLine($"Id: {setting.getID()}, Nom: {setting.getName()}, Source: {setting.getSourceDirectory()}, Destination: {setting.getTargetDirectory()}, Type: {setting.getType()}");
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

                Console.WriteLine(GetTraductor("ModifyBackupNewName"));
                string newName = Console.ReadLine();
                if (!string.IsNullOrEmpty(newName))
                    settingToModify.setName(newName);

                Console.WriteLine(GetTraductor("ModifyBackupNewSourcePath"));
                string newSourcePath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newSourcePath))
                    settingToModify.setSourceDirectory(newSourcePath);

                Console.WriteLine(GetTraductor("ModifyBackupNewDestinationPath"));
                string newDestinationPath = Console.ReadLine();
                if (!string.IsNullOrEmpty(newDestinationPath))
                    settingToModify.setTargetDirectory(newDestinationPath);

                Console.WriteLine(GetTraductor("ModifyBackupNewType"));
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
            Console.WriteLine(GetTraductor("DeleteBackup"));
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

           // Directory.CreateDirectory(dest);

            int totalFilesDone = 0;  // Initialize totalFilesDone here

            foreach (FileInfo file in dir.GetFiles())
            {
                totalFilesDone++;
               // file.CopyTo(Path.Combine(dest, file.Name), true);   // Copy the file into the destination directory
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
                        DisplayBackupSettings(BackupListInfo);
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
                        DisplayBackupSettings(BackupListInfo);
                        DeleteBackupSetting(BackupListInfo);
                        break;
                    case 5:
                        Lang();
                        Console.Clear();
                        break;
                    case 6:
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


            string jsonContent = File.ReadAllText(filePath);

            JArray jsonArray = JArray.Parse(jsonContent);

            int maxId = 0;
            bool foundId = false;
            foreach (var item in jsonArray)
            {
                int id = (int)item["Id"];
                if (id > maxId)
                {
                    maxId = id;
                    foundId = true;
                }
            }
            maxId++;

            if (!foundId)
            {
                maxId = 1;
            }

            BackupListInfo.Add(_backup.CreateBackup(maxId.ToString(), name, sourcePath, destinationPath, type));
        }

        public static void LaunchSlotBackup(List<Backup> backupList)
        {
            Console.Write(GetTraductor("LunchSlopBackup"));
            string ChoiceBackup = Console.ReadLine();


            string[] parts = ChoiceBackup.Split(new char[] { ';' , ' '}, StringSplitOptions.RemoveEmptyEntries);



            foreach (var backupInfo in backupList)
            {
                foreach (string part in parts)
                {
                    if (part.Contains("-"))
                    {
                        string[] rangeParts = part.Split('-');
                        int start = int.Parse(rangeParts[0]);
                        int end = int.Parse(rangeParts[1]);

                        for (int i = start; i <= end; i++)
                        {
                            if (i.ToString() == backupInfo.getID())
                            {
                                //Create directory if it doesn't already exist
                                if (Directory.Exists(backupInfo.getTargetDirectory()))
                                {
                                    foreach (string AllDirectory in Directory.GetDirectories(backupInfo.getSourceDirectory(), "*", SearchOption.AllDirectories))
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory()));
                                    }
                                }
                                else
                                {
                                    Directory.CreateDirectory(backupInfo.getTargetDirectory());
                                    foreach (string AllDirectory in Directory.GetDirectories(backupInfo.getSourceDirectory(), "*", SearchOption.AllDirectories))
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory()));
                                    }
                                }

                                if (backupInfo != null)
                                {
                                    if (backupInfo.getType() == "complet" || backupInfo.getType() == "Complet")
                                    {
                                        TypeComplet(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory());
                                    }
                                    else
                                    {
                                        TypeDifferential(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory());
                                    }
                                }
                                else
                                {
                                    Log.Information("Sauvegarde non trouvée.");
                                }
                            }
                        }
                    }
                    else
                    {
                        int number;
                        if (int.TryParse(part, out number))
                        {
                            if (number.ToString() == backupInfo.getID())
                            {
                                //Create directory if it doesn't already exist
                                if (Directory.Exists(backupInfo.getTargetDirectory()))
                                {
                                    foreach (string AllDirectory in Directory.GetDirectories(backupInfo.getSourceDirectory(), "*", SearchOption.AllDirectories))
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory()));
                                    }
                                }
                                else
                                {
                                    Directory.CreateDirectory(backupInfo.getTargetDirectory());
                                    foreach (string AllDirectory in Directory.GetDirectories(backupInfo.getSourceDirectory(), "*", SearchOption.AllDirectories))
                                    {
                                        Directory.CreateDirectory(AllDirectory.Replace(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory()));
                                    }
                                }

                                if (backupInfo != null)
                                {
                                    if (backupInfo.getType() == "complet" || backupInfo.getType() == "Complet")
                                    {
                                        TypeComplet(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory());
                                    }
                                    else
                                    {
                                        TypeDifferential(backupInfo.getSourceDirectory(), backupInfo.getTargetDirectory());
                                    }
                                }
                                else
                                {
                                    Log.Information("Sauvegarde non trouvée.");
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void TypeComplet(string PathSource, string PathTarget)
        {
            // Create All Directories
            foreach (string AllDirectory in Directory.GetDirectories(PathSource, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(AllDirectory.Replace(PathSource, PathTarget));
            }

            // Copy Files
            foreach (string AllFiles in Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories))
            {
                string targetFilePath = Path.Combine(PathTarget, AllFiles.Substring(PathSource.Length + 1));
                CopyFileWithProgress(AllFiles, targetFilePath);
                Log.Information($"Le fichier '{AllFiles}' a été copié vers {PathTarget}");
            }
        }
        /*public static void TypeComplet(string PathSource, string PathTarget)
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

        }*/

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
                            Console.Write($"Copy progress: {progressPercentage}% pour " +sourceFilePath);
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
            // Obtenir la liste des fichiers dans le premier dossier et des sous-dossiers récursivement
            string[] files = Directory.GetFiles(PathSource, "*.*", SearchOption.AllDirectories);

            // Parcourir chaque fichier dans le dossier 1
            foreach (string filePath1 in files)
            {
                // Obtenir le chemin relatif du fichier par rapport au dossier source
                string relativePath = filePath1.Substring(PathSource.Length + 1);

                // Chemin vers le fichier dans le dossier destination
                string filePath2 = Path.Combine(PathTarget, relativePath);

                // Vérifie si le fichier existe dans le dossier 2
                if (File.Exists(filePath2))
                {
                    // Obtenir la date de dernière modification des deux fichiers
                    DateTime lastModified1 = File.GetLastWriteTime(filePath1);
                    DateTime lastModified2 = File.GetLastWriteTime(filePath2);

                    // Comparer les dates
                    if (lastModified1 > lastModified2)
                    {
                        // Copier le fichier du premier emplacement vers le deuxième emplacement
                        File.Copy(filePath1, filePath2, true);
                        CopyFileWithProgress(filePath1, filePath2);
                        Log.Information($"Le fichier '{relativePath}' dans {PathSource} a été copié vers {PathTarget} car il a été modifié plus récemment.");
                    }
                    else if (lastModified1 < lastModified2)
                    {
                        Log.Information($"Le fichier '{relativePath}' dans {PathTarget} a été modifié après le fichier dans {PathSource}.");
                    }
                    else
                    {
                        CopyFileWithProgress(filePath1, filePath2);
                        Log.Information($"Les fichiers '{relativePath}' ont été modifiés à la même date.");
                    }
                }
                else
                {
                    // Créer le répertoire s'il n'existe pas déjà dans le dossier cible
                    string directoryPath = Path.GetDirectoryName(filePath2);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Copier le fichier du premier emplacement vers le deuxième emplacement
                    File.Copy(filePath1, filePath2);
                    CopyFileWithProgress(filePath1, filePath2);
                    Log.Information($"Le fichier '{relativePath}' a été copié de {PathSource} vers {PathTarget} car il n'existait pas dans {PathTarget}.");
                }
            }
            // Supprimer les fichiers dans le dossier cible qui n'existent plus dans le dossier source
            string[] targetFiles = Directory.GetFiles(PathTarget, "*.*", SearchOption.AllDirectories);
            foreach (string targetFilePath in targetFiles)
            {
                string relativePath = targetFilePath.Substring(PathTarget.Length + 1);
                string sourceFilePath = Path.Combine(PathSource, relativePath);
                if (!File.Exists(sourceFilePath))
                {
                    File.Delete(targetFilePath);
                    Log.Information($"Le fichier '{relativePath}' a été supprimé de {PathTarget} car il n'existe plus dans {PathSource}.");
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

            menuInterface = new List<string>() { GetTraductor("Create"), GetTraductor("Launch"), GetTraductor("Edit"),GetTraductor("Delete"), GetTraductor("Language"), GetTraductor("Exit") };
            
        }
    }
}



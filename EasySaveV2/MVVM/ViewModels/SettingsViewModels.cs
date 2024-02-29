using EasySaveV2.MVVM.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using System.Configuration;
using EasySaveV2.MVVM.Views;
using EasySaveV2.MVVM.ViewModels;

namespace EasySaveV2.MVVM.ViewModels
{
    class SettingsViewModels
    {
        /*****************************************/
        /* Déclaration des attributs en publique */
        /*****************************************/
        public static ObservableCollection<string> ExtensionCryptoSoft { get; set; } = new ObservableCollection<string>();
        public static ObservableCollection<string> ExtensionPriority { get; set; } = new ObservableCollection<string>();

        /**************************************/
        /* Déclaration des attributs en privé */
        /**************************************/
        private static dailylogs logs = new dailylogs();
        private static bool type;

        /****************************************/
        /* Déclaration des méthodes en publique */
        /****************************************/

        // Méthode pour changer de formation de log
        public static void Formatlog(bool format_logs)
        {
            type = format_logs;
            logs.Logsjson(format_logs);
            dailylogs.selectedLogger.Information("Application started successfully");
        }

        // Méthode pour ajouter une extension dans la liste de cryptage
        public void AddList(string ext)
        {
            ExtensionCryptoSoft.Add(ext);
        }

        // Méthode pour ajouter une extension dans la liste de priorité
        public void AddListPriority(string ext)
        {
            ExtensionPriority.Add(ext);
        }

        // Méthode pour le type de format des logs
        public static void typelog()
        {
            Formatlog(type);
        }

        // Méthode pour changer l'état des states logs
        public static void StateLogs(string name, string fileSource, string fileTarget, long fileSize, string state, int totalFiles, int nbFilesToGet, int crypting)
        {
            StateLog stateLog = new StateLog(name, fileSource, fileTarget, fileSize, state, totalFiles, nbFilesToGet, crypting);
            string stateLogListPath;

            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            }

            if (!type)
            {
                stateLogListPath = @"C:\Temp\stateLog.json";
            }
            else
            {
                stateLogListPath = @"C:\Temp\stateLog.xml";
            }

            List<KeyValuePair<string, StateLog>> stateLogList = new List<KeyValuePair<string, StateLog>>();

            // Vérifier si le fichier existe et s'il a un contenu
            if (File.Exists(stateLogListPath))
            {
                if (!type)
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

            // Ajouter ou mettre à jour l'entrée dans la liste des états (stateLogList)
            var existingEntry = stateLogList.FirstOrDefault(entry => entry.Key == name);

            if (existingEntry.Equals(default(KeyValuePair<string, StateLog>)))
            {
                stateLogList.Add(new KeyValuePair<string, StateLog>(name, stateLog));
            }
            else
            {
                stateLogList[stateLogList.IndexOf(existingEntry)] = new KeyValuePair<string, StateLog>(name, stateLog);
            }

            // Écrire le dictionnaire entier dans le fichier
            if (!type)
            {
                string jsonToWrite = JsonConvert.SerializeObject(stateLogList, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(stateLogListPath, jsonToWrite);
            }
            else
            {
                stateLogListPath = @"C:\Temp\stateLog.xml";

                XmlDocument doc = new XmlDocument();

                // Vérifier si le fichier existe et s'il a un contenu
                if (File.Exists(stateLogListPath))
                {
                    doc.Load(stateLogListPath);

                    // Rechercher une entrée existante
                    XmlElement node = (XmlElement)doc.SelectSingleNode($"/ArrayOfKeyValuePairOfStringStateLog/KeyValuePairOfStringStateLog[Name='{name}']");

                    if (node != null)
                    {
                        // Mise à jour d'un nœud existant
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
                        // Créer une nouvelle entrée
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
                    // Le fichier n'existe pas, créer un nouveau fichier XML
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KeyValuePair<string, StateLog>>));
                    using (FileStream fs = new FileStream(stateLogListPath, FileMode.Create))
                    {
                        serializer.Serialize(fs, new List<KeyValuePair<string, StateLog>>());
                    }
                }
            }
        }


        // Méthode pour enregistrer dans les states logs l'état des sauvegardes
        public static void SetSaveStateBackup(string backupName, string src, string dest, int crypting)
        {
            var dir = new DirectoryInfo(src);
            // Mettre en cache les répertoires avant de commencer à les copier
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(dest);

            // Initialiser ici le totalFilesDone
            int totalFilesDone = 0;  

            //Récupére chaque information des fichiers pour enregistrer dans les State logs
            foreach (FileInfo file in dir.GetFiles())
            {
                totalFilesDone++;
                string fileSrc = Path.Combine(file.DirectoryName, file.Name);
                string fileDest = Path.Combine(dest, file.Name);
                int totalFiles = Directory.GetFiles(src, "*", SearchOption.AllDirectories).Length;
                StateLogs(backupName, fileSrc, fileDest, file.Length, "On", totalFiles, totalFilesDone, crypting);
            }
        }

        // Méthode pour garder la position du button
        public bool ToggleButtonState
        {
            get => bool.Parse(ConfigurationManager.AppSettings["ToggleButtonState"] ?? "false");
            set => ConfigurationManager.AppSettings["ToggleButtonState"] = value.ToString();
        }

        // Méthode pour changer la langue en anglais
        public void TraductorEnglish()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Language/DictionaryEnglish.xaml", UriKind.RelativeOrAbsolute);
        }

        // Méthode pour changer la langue en français
        public void TraductorFrench()
        {
            Application.Current.Resources.MergedDictionaries[0].Source = new Uri("Language/DictionaryFrench.xaml", UriKind.RelativeOrAbsolute);
        }


    }
}

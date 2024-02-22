using EasySaveV2.MVVM.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace EasySaveV2.MVVM.ViewModels
{
    class BackupViewModels
    {
        private static XmlDocument doc;
        private static Backup _backup = new Backup();
        static string directoryPath = @"C:\JSON";
        static string filePath = @"C:\JSON\confbackup.json";
        public static ObservableCollection<Backup> BackupListInfo = new ObservableCollection<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        //private static int totalFilesDone = 0;
        const int MaxBackupSettings = 5;

        public static ObservableCollection<Backup> getBackupList()
        {
            return BackupListInfo;
        }

        public static void CreateSlotBackup(string name, string sourcePath, string destinationPath, string type)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            BackupListInfo.Add(_backup.CreateBackup(name, sourcePath, destinationPath, type, "Off", "True"));
            SaveBackupSettings();
        }
        public static void SaveBackupSettings()
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
                    dailylogs.selectedLogger.Information("Une erreur est survenue lors de l'enregistrement des paramètres de sauvegarde : " + ex.Message);
                }
            }
            else
            {
                //Console.WriteLine(GetTraductor("NoSave"));
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
                            item["Type"] != null && !string.IsNullOrEmpty(item["Type"].ToString()) &&
                            item["State"] != null && !string.IsNullOrEmpty(item["State"].ToString()) &&
                            item["Stopped"] != null && !string.IsNullOrEmpty(item["Stopped"].ToString()))
                        {
                            Backup backup = new Backup(
                                item["Name"].ToString(),
                                item["Source"].ToString(),
                                item["Target"].ToString(),
                                item["Type"].ToString(),
                                item["State"].ToString(),
                                item["Stopped"].ToString()
                            );
                            try
                            {
                                BackupListInfo.Add(backup);   // CorrectElements
                            }
                            catch
                            {
                                dailylogs.selectedLogger.Information($"Erreur lors de l'ajout de données.");
                            }
                        }
                        else
                        {
                            dailylogs.selectedLogger.Information($"Élément de données invalide trouvé.");
                        }
                    }
                }
            }
        }
    }
}

using EasySaveV2.MVVM.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EasySaveV2.MVVM.ViewModels
{
    class BackupViewModels
    {
        private static XmlDocument doc;
        private static Backup _backup = new Backup();
        private static dailylogs logs = new dailylogs();
        static string format_logs;
        static string directoryPath = @"C:\JSON";
        static string filePath = @"C:\JSON\confbackup.json";
        public static List<Backup> BackupListInfo = new List<Backup>();
        private static List<StateLog> stateLogList = new List<StateLog>();
        //private static int totalFilesDone = 0;
        const int MaxBackupSettings = 5;

        public static void CreateSlotBackup(string name, string sourcePath, string destinationPath, string type)
        {
            BackupListInfo.Add(_backup.CreateBackup(name, sourcePath, destinationPath, type));
            SaveBackupSettings();
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
                //Console.WriteLine(GetTraductor("NoSave"));
            }
        }
    }
}

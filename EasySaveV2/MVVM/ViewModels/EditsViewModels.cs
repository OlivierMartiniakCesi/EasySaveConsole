using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveV2.MVVM.Models;
using EasySaveV2.MVVM.ViewModels;
using EasySaveV2.MVVM.Views;
using System.IO;
using Serilog;


namespace EasySaveV2.MVVM.ViewModels
{
    class EditsViewModels
    {
        public static Backup EditorBackup { get; set; } = new Backup();
        public EditsViewModels(Backup SelectBackup)
        {
            EditorBackup = SelectBackup;
        }

        public static void SaveBackupSettings(string name, string source, string destination, string type)
        {
            string filePath = @"C:\JSON\confbackup.json";

            if (BackupViewModels.BackupListInfo != null && BackupViewModels.BackupListInfo.Count >= 0)
            {
                int backupIndex = BackupViewModels.BackupListInfo.IndexOf(EditorBackup);
                string jsonText = "[";

                BackupViewModels.BackupListInfo[backupIndex].setName(name);
                BackupViewModels.BackupListInfo[backupIndex].setSourceDirectory(source);
                BackupViewModels.BackupListInfo[backupIndex].setTargetDirectory(destination);
                BackupViewModels.BackupListInfo[backupIndex].setType(type);

                foreach (Backup item in BackupViewModels.BackupListInfo)
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
        }
    }
}

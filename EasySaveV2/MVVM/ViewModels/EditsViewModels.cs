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
        /*****************************************/
        /* Déclaration des attributs en publique */
        /*****************************************/
        public static Backup EditorBackup { get; set; } = new Backup();

        /*********************************/
        /* Déclaration des constructeurs */
        /*********************************/
        public EditsViewModels(Backup SelectBackup)
        {
            EditorBackup = SelectBackup;
        }


        /****************************************/
        /* Déclaration des méthodes en publique */
        /****************************************/

        // Méthode pour modifier les paramètres du JSON
        public static void SaveBackupSettings(string name, string source, string destination, string type)
        {
            string filePath = @"C:\JSON\confbackup.json";

            if (BackupViewModels.BackupListInfo != null && BackupViewModels.BackupListInfo.Count >= 0)
            {
                int backupIndex = BackupViewModels.BackupListInfo.IndexOf(EditorBackup);
                string jsonText = "[";

                // Modifie les paramètres
                BackupViewModels.BackupListInfo[backupIndex].setName(name);
                BackupViewModels.BackupListInfo[backupIndex].setSourceDirectory(source);
                BackupViewModels.BackupListInfo[backupIndex].setTargetDirectory(destination);
                BackupViewModels.BackupListInfo[backupIndex].setType(type);

                foreach (Backup item in BackupViewModels.BackupListInfo)
                {
                    jsonText += item.SaveJson() + ",";
                }
                //Supprime la dernière virgule et ferme le JSON
                jsonText = jsonText.TrimEnd(',') + "]";
                try
                {
                    //Ecrit les paramètres dans le JSON
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

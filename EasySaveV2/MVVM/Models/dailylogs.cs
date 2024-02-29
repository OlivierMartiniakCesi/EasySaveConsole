using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveV2.MVVM.Models
{
    class dailylogs
    {
        /***************************************************************/
        /* Déclaration des attributs en privé pour les logs jounaliers */
        /***************************************************************/
        private static LoggerConfiguration jsonLoggerConfiguration;
        private static LoggerConfiguration xmlLoggerConfiguration;
        private static string logDirectory = @"C:\Temp";
        private static ILogger jsonLogger;
        private static ILogger xmlLogger;

        /******************************************************************/
        /* Déclaration des attributs en publique pour les logs jounaliers */
        /******************************************************************/
        public static ILogger selectedLogger;

        /*********************************/
        /* Déclaration d'un constructeur */
        /*********************************/
        public dailylogs()
        {
            // Création du dossier log
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Configurer les logs 
            jsonLoggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDirectory, "log.json"), rollingInterval: RollingInterval.Day);

            xmlLoggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDirectory, "log.xml"), rollingInterval: RollingInterval.Day);

            // Création d'un logger JSON ou XML
            // Utiliser pour écrire dans le fichier log
            jsonLogger = jsonLoggerConfiguration.CreateLogger();
            xmlLogger = xmlLoggerConfiguration.CreateLogger();
        }

        /*******************************************************************/
        /* Déclaration des méthodes en publiques pour les logs journaliers */
        /*******************************************************************/

        //Méthode pour changer le format des logs
        public void Logsjson(bool logformat)
        {
            string dateFormat = DateTime.Now.ToString("yyyyMMdd");

            // Construit le nom de fichier de log en fonction du format de log
            string logFileName = logformat ? $"log{dateFormat}.xml" : $"log{dateFormat}.json";
            string logFilePath = Path.Combine(logDirectory, logFileName);

            // Sélectionner le format pour les logs
            selectedLogger = logformat ? xmlLogger : jsonLogger;
            selectedLogger.Information("Changement d'extension des logs réalisé");
        }
    }
}
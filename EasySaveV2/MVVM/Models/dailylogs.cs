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
        private static LoggerConfiguration jsonLoggerConfiguration;
        private static LoggerConfiguration xmlLoggerConfiguration;
        private static bool isLoggerCreated = false;
        static string logDirectory = @"C:\Temp";
        private static ILogger jsonLogger;
        private static ILogger xmlLogger;
        public static ILogger selectedLogger;

        static dailylogs()
        {
            // Create directory if it's needed
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            jsonLoggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDirectory, "log.json"), rollingInterval: RollingInterval.Day);

            xmlLoggerConfiguration = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDirectory, "log.xml"), rollingInterval: RollingInterval.Day);
            jsonLogger = jsonLoggerConfiguration.CreateLogger();
            xmlLogger = xmlLoggerConfiguration.CreateLogger();
        }

        public void Logsjson(bool logformat)
        {
            string dateFormat = DateTime.Now.ToString("yyyyMMdd");
            string logFileName = logformat ? $"log{dateFormat}.xml" : $"log{dateFormat}.json";
            string logFilePath = Path.Combine(logDirectory, logFileName);
            selectedLogger = logformat ? xmlLogger : jsonLogger;
            selectedLogger.Information("Changement d'extension des logs réalisé");
        }
    }
}
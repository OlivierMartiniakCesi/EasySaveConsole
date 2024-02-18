using EasySaveV2.MVVM.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveV2.MVVM.ViewModels
{
    class SettingsViewModels
    {
        private static dailylogs logs = new dailylogs();
        static bool type;

        public static void Formatlog(bool format_logs)
        {
            type = format_logs;
            logs.Logsjson(format_logs);
            Log.Information("Application started successfully");
        }
        public static void typelog()
        {
            Formatlog(type);
        }
    }
}

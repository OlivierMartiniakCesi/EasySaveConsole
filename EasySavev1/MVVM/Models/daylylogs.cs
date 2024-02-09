using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveConsole.Models
{
    class daylylogs
    {
        public void Logsjson()
        {
            // Create directory if it's needed
            string logDirectory = @"C:\Temp";
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()  // Write logs on console
            .WriteTo.File(@"C:\Temp\log.json", rollingInterval: RollingInterval.Day) // Write daily logs on JSON File 
            .CreateLogger();
        }
    }
}

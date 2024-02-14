using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveConsole.MVVM.Models
{
    public class StateLog
    {
        public string name;
        public string fileSource;
        public string fileTarget;
        public long fileSize;
        public string state;
        public long totalFiles;
        public long nbFilestoGet;
        public int crypting;
        public int progression;
        public string Time;

        public StateLog(string name, string fileSource, string fileTarget, long fileSize, string state, long totalFiles, long totalFilesDone, int crypting)
        {
            this.name = name;
            this.fileSource = fileSource;
            this.fileTarget = fileTarget;
            this.totalFiles = totalFiles;
            this.nbFilestoGet = totalFiles - totalFilesDone;
            if (this.nbFilestoGet == 0) 
            {
                this.state = "Off";
            }
            else
            {
                this.state = "On";
            }
            this.fileSize = fileSize;
            if (totalFilesDone == 0)
            {
                this.progression = 0;
            }
            else
            {
                this.progression = Convert.ToInt32(totalFilesDone / totalFiles) * 100;
            }
            this.crypting = crypting;
            this.Time = DateTime.Now.ToString();
        }
    }
}

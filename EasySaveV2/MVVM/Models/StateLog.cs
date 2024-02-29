using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveV2.MVVM.Models
{
    public class StateLog
    {
        /**************************************************************/
        /* D�claration des attributs en publique pour les states logs */
        /**************************************************************/
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

        /*******************************/
        /* D�claration du constructeur */
        /*******************************/
        public StateLog(string name, string fileSource, string fileTarget, long fileSize, string state, long totalFiles, long totalFilesDone, int crypting)
        {
            this.name = name;
            this.fileSource = fileSource;
            this.fileTarget = fileTarget;
            this.totalFiles = totalFiles;
            this.nbFilestoGet = totalFiles - totalFilesDone;
            this.crypting = crypting;
            this.fileSize = fileSize;

            //V�rifie si la sauvegarde est lanc�
            if (this.nbFilestoGet == 0)
            {
                this.state = "Off";
            }
            else
            {
                this.state = "On";
            }
            

            // V�rifie en pourcentage le nombre de fichier envoy�
            if (totalFilesDone == 0)
            {
                this.progression = 0;
            }
            else
            {
                this.progression = Convert.ToInt32(totalFilesDone / totalFiles) * 100;
            }

            this.Time = DateTime.Now.ToString();
        }
    }
}
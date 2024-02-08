using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySavev1.MVVM.Models
{
    public enum BackupType
    {
        Complet,
        Differentiel
    }
    class Backup
    {
        private string Name { get; set; }
        private string SourceDirectory { get; set; }
        private string TargetDirectory { get; set; }
        public BackupType Type { get; set; }
        
        //Constructor
        public Backup() { }

        //Constructor with parameter
        public Backup(string Name, string PathSource, string PathTarget, int type) 
        {
            this.Name = Name;
            this.SourceDirectory = PathSource;
            this.TargetDirectory = PathTarget;
            this.Type = (BackupType)type;
        }

        ~Backup() 
        {
            System.Diagnostics.Trace.WriteLine("finalizer is called.");
        }

        public string getName()
        {
            return Name;
        }

        public string getSourceDirectory()
        {
            return SourceDirectory;
        }

        public string getTargetDirectory()
        {
            return TargetDirectory;
        }

        public new BackupType GetType()
        {
            return Type;
        }

        public string getAllInfo()
        {
            string Information = "Name : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }


    }
}

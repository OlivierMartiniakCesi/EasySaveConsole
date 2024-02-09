using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveConsole.MVVM.Models
{
    class Backup
    {
        public string Name { get; set; }
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string Type { get; set; }
        
        //Constructor
        public Backup() { }

        //Constructor with parameter
        public Backup(string Name, string PathSource, string PathTarget, string type) 
        {
            this.Name = Name;
            this.SourceDirectory = PathSource;
            this.TargetDirectory = PathTarget;
            this.Type = type;
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

        public string getType()
        {
            return Type;
        }

        public string getAllInfo()
        {
            string Information = "Name : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }

        public Backup CreateBackup(string Name, string PathSource, string PathTarget, string type)
        {
            return new Backup(Name, PathSource, PathTarget, type);
        }
    }
}

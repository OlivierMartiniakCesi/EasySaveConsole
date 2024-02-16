using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveConsole.MVVM.Models
{
    class Backup
    {
        private string Name { get; set; }
        private string SourceDirectory { get; set; }
        private string TargetDirectory { get; set; }
        private string Type { get; set; }
        private string Id { get; set; }

        //Constructor
        public Backup() { }

        //Constructor with parameter
        public Backup(string Id, string Name, string PathSource, string PathTarget, string type)
        {
            this.Id = Id;
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

        public void setName(string Name)
        {
            this.Name = Name;
        }

        public string getSourceDirectory()
        {
            return SourceDirectory;
        }
        public void setSourceDirectory(string SourceDirectory)
        {
            this.SourceDirectory = SourceDirectory;
        }

        public string getTargetDirectory()
        {
            return TargetDirectory;
        }

        public void setTargetDirectory(string TargetDirectory)
        {
            this.TargetDirectory = TargetDirectory;
        }

        public string getType()
        {
            return Type;
        }

        public void setType(string Type)
        {
            this.Type = Type;
        }

        public string getID()
        {
            return Id;
        }
        public string getAllInfo()
        {
            string Information = "Id = " + Id + "\tName : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }

        public Backup CreateBackup(string Id, string Name, string PathSource, string PathTarget, string type)
        {
            return new Backup(Id, Name, PathSource, PathTarget, type);
        }

        public string SaveJson()
        {
            string FileStock = "{" +
             Environment.NewLine + "\t\"Id\":\"" + Id + "\"," +
             Environment.NewLine + "\t\"Name\":\"" + Name + "\"," +
             Environment.NewLine + "\t\"Source\":\"" + SourceDirectory + "\"," +
             Environment.NewLine + "\t\"Target\":\"" + TargetDirectory + "\"," +
             Environment.NewLine + "\t\"Type\":\"" + Type + "\"" +
             Environment.NewLine + "}";

            return FileStock.Replace("\\", "\\\\");
        }

    }
}

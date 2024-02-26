using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;

namespace RemoteEasySave.MVVM.Models
{
    class Backup
    {
        private string Name;
        private string SourceDirectory;
        private string TargetDirectory;
        private string Type;


        public string DashboardName
        {
            get { return Name; }
            set { Name = value; }
        }
        public string DashboardSource
        {
            get { return SourceDirectory; }
            set { SourceDirectory = value; }
        }

        public string DashboardDestination
        {
            get { return TargetDirectory; }
            set { TargetDirectory = value; }
        }

        public string DashboardType
        {
            get { return Type; }
            set { Type = value; }
        }

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

        public string getAllInfo()
        {
            string Information = "Name : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }

        public Backup CreateBackup(string Name, string PathSource, string PathTarget, string type)
        {
            return new Backup(Name, PathSource, PathTarget, type);
        }

        public string SaveJson()
        {
            string FileStock = "{" +
             Environment.NewLine + "\t\"Name\":\"" + Name + "\"," +
             Environment.NewLine + "\t\"Source\":\"" + SourceDirectory + "\"," +
             Environment.NewLine + "\t\"Target\":\"" + TargetDirectory + "\"," +
             Environment.NewLine + "\t\"Type\":\"" + Type + "\"" +
             Environment.NewLine + "}";

            return FileStock.Replace("\\", "\\\\");
        }

        public void Remove() 
        {
            this.Name = null;
            this.SourceDirectory = null;
            this.TargetDirectory = null;
            this.Type = null;

            GC.Collect();
        }
    }
}

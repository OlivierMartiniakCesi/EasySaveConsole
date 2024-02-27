using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Input;
using System.ComponentModel;

namespace EasySaveV2.MVVM.Models
{
    class Backup : INotifyPropertyChanged
    {
        private string Name { get; set; }
        private string SourceDirectory { get; set; }
        private string TargetDirectory { get; set; }
        private string Type { get; set; }
        private string State { get; set; }
        private string Stopped { get; set; }

        //Constructor
        public Backup() { }

        //Constructor with parameter
        public Backup(string Name, string PathSource, string PathTarget, string type, string State, string Stopped)
        {
            this.Name = Name;
            this.SourceDirectory = PathSource;
            this.TargetDirectory = PathTarget;
            this.Type = type;
            this.State = State;
            this.Stopped = Stopped;
        }
        private int progress;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int Progress
        {
            get { return progress; }
            set
            {
                if (progress != value)
                {
                    progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }
        private string _state;
        public string States
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(States)); // Modifier State en States
                }
            }
        }
        ~Backup()
        {
            System.Diagnostics.Trace.WriteLine("finalizer is called.");
        }

        public string DashboardName 
        {
            get
            {
                var NameDash = Name;
                return NameDash;
            }
        }
        public string DashboardSource 
        {
            get 
            {
                var Repertory = new DirectoryInfo(SourceDirectory);
                var getRepertory = Directory.GetParent(Repertory.ToString());
                var Source = @"..\" + getRepertory.Name + @"\" + Repertory.Name;
                return Source;
            }
        }

        public string DashboardDestination
        {
            get
            {
                var Repertory = new DirectoryInfo(TargetDirectory);
                var getRepertory = Directory.GetParent(Repertory.ToString());
                var Destination = @"..\" + getRepertory.Name + @"\" + Repertory.Name;
                return Destination;
            }
        }

        public string DashboardType 
        {
            get
            {
                var getTypeBackup = Type;

                return getTypeBackup;
            }
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

        public string getState()
        {
            return State;
        }

        public void setState(string State)
        {
            this.State = State;
        }
        public string getStopped()
        {
            return Stopped;
        }

        public void setStopped(string Stopped)
        {
            this.Stopped = Stopped;
        }

        public string getAllInfo()
        {
            string Information = "Name : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }

        public Backup CreateBackup(string Name, string PathSource, string PathTarget, string type, string State, string Stopped)
        {
            return new Backup(Name, PathSource, PathTarget, type, State, Stopped);
        }

        public string SaveJson()
        {
            string FileStock = "{" +
             Environment.NewLine + "\t\"Name\":\"" + Name + "\"," +
             Environment.NewLine + "\t\"Source\":\"" + SourceDirectory + "\"," +
             Environment.NewLine + "\t\"Target\":\"" + TargetDirectory + "\"," +
             Environment.NewLine + "\t\"Type\":\"" + Type + "\"," +
             Environment.NewLine + "\t\"State\":\"" + State + "\"," +
             Environment.NewLine + "\t\"Stopped\":\"" + Stopped + "\""+
             Environment.NewLine + "}";

            return FileStock.Replace("\\", "\\\\");
        }

        public void Remove() 
        {
            this.Name = null;
            this.SourceDirectory = null;
            this.TargetDirectory = null;
            this.Type = null;
            this.State = null;
            this.Stopped = null;

            GC.Collect();
        }
    }
}

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
        /***********************************************************/
        /* Déclaration des attributs en privé pour les sauvegardes */
        /***********************************************************/
        private string Name { get; set; }
        private string SourceDirectory { get; set; }
        private string TargetDirectory { get; set; }
        private string Type { get; set; }
        private string State { get; set; }
        private string Stopped { get; set; }

        // Attribut déclare un type nullable
        private int? crypting { get; set; }

        /**************************************************************/
        /* Déclaration des attributs en publique pour les sauvegardes */
        /*       Attribut pour le tableau dynamique dans le XAML      */
        /**************************************************************/
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

        /*******************************************************************/
        /* Déclaration des attributs en privé pour la barre de progression */
        /*******************************************************************/
        private int progress;
        private string _state;

        /**********************************************************************/
        /* Déclaration des attributs en publique pour la barre de progression */
        /**********************************************************************/

        public event PropertyChangedEventHandler PropertyChanged;
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

        /*********************************/
        /* Déclaration des constructeurs */
        /*********************************/
        public Backup() { }
        public Backup(string Name, string PathSource, string PathTarget, string type, string State, string Stopped, int? crypting)
        {
            this.Name = Name;
            this.SourceDirectory = PathSource;
            this.TargetDirectory = PathTarget;
            this.Type = type;
            this.State = State;
            this.Stopped = Stopped;
            this.crypting = crypting;
        }


        /*********************************************************/
        /* Déclaration des méthodes pour la barre de progression */
        /*********************************************************/

        //Méthode pour vérifier et informer tout changement de la barre de progression
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /*************************************************************/
        /* Déclaration des méthodes en publique pour les sauvegardes */
        /*************************************************************/

        //Méthode pour obtenir l'attribut Name
        public string getName()
        {
            return Name;
        }

        //Méthode pour modifier l'attribut Name
        public void setName(string Name)
        {
            this.Name = Name;
        }

        //Méthode pour obtenir l'attribut SourceDirectory
        public string getSourceDirectory()
        {
            return SourceDirectory;
        }

        //Méthode pour modifier l'attribut SourceDirectory
        public void setSourceDirectory(string SourceDirectory)
        {
            this.SourceDirectory = SourceDirectory;
        }

        //Méthode pour obtenir l'attribut TargetDirectory
        public string getTargetDirectory()
        {
            return TargetDirectory;
        }

        //Méthode pour modifier l'attribut TargetDirectory
        public void setTargetDirectory(string TargetDirectory)
        {
            this.TargetDirectory = TargetDirectory;
        }

        //Méthode pour obtenir l'attribut Type
        public string getType()
        {
            return Type;
        }

        //Méthode pour modifier l'attribut Type
        public void setType(string Type)
        {
            this.Type = Type;
        }

        //Méthode pour obtenir l'attribut State
        public string getState()
        {
            return State;
        }

        //Méthode pour modifier l'attribut State  
        public void setState(string State)
        {
            this.State = State;
        }

        //Méthode pour obtenir l'attribut Stopped  
        public string getStopped()
        {
            return Stopped;
        }

        //Méthode pour modifier l'attribut Stopped  
        public void setStopped(string Stopped)
        {
            this.Stopped = Stopped;
        }

        //Méthode pour obtenir l'attribut crypting  
        public int getcrypting()
        {
            return (int) crypting;
        }

        //Méthode pour modifier l'attribut crypting  
        public void setCrypt(int crypting)
        {
            this.crypting = crypting;
        }

        //Méthode pour récupérer toute les informations d'une sauvegarde
        public string getAllInfo()
        {
            string Information = "Name : " + Name + "\tSource directory : " + SourceDirectory + "\tTarget directory : " + TargetDirectory + "\tType : " + Type;
            return Information;
        }

        //Méthode créer une sauvegarde
        public Backup CreateBackup(string Name, string PathSource, string PathTarget, string type, string State, string Stopped, int crypting)
        {
            return new Backup(Name, PathSource, PathTarget, type, State, Stopped, crypting);
        }

        //Méthode pour sauvegarder les informations au format JSON
        public string SaveJson()
        {
            string FileStock = "{" +
             Environment.NewLine + "\t\"Name\":\"" + Name + "\"," +
             Environment.NewLine + "\t\"Source\":\"" + SourceDirectory + "\"," +
             Environment.NewLine + "\t\"Target\":\"" + TargetDirectory + "\"," +
             Environment.NewLine + "\t\"Type\":\"" + Type + "\"," +
             Environment.NewLine + "\t\"State\":\"" + State + "\"," +
             Environment.NewLine + "\t\"Stopped\":\"" + Stopped + "\""+
             Environment.NewLine + "\t\"Crypting\":\"" + crypting + "\"" +
             Environment.NewLine + "}";

            return FileStock.Replace("\\", "\\\\");
        }

        //Méthode pour supprimer une sauvegarde
        public void Remove() 
        {
            this.Name = null;
            this.SourceDirectory = null;
            this.TargetDirectory = null;
            this.Type = null;
            this.State = null;
            this.Stopped = null;
            this.crypting = null;

            //Libérer automatiquement la mémoire des objets 
            //Méthode pour forcer le garbage collector à s'executer
            GC.Collect();
        }
    }
}

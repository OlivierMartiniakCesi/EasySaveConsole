using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySaveConsole.MVVM.Models;
using EasySaveConsole.MVVM.ViewModels;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Resources;
using System.Reflection;
using System.Globalization;


namespace EasySaveConsole.MVVM.Views
{
    class Vue
    {
        //Gestionnaire de ressources qui facilite l'accès aux ressources
        private static ResourceManager rm = new ResourceManager("EasySave.Resources.Text", Assembly.GetExecutingAssembly());

        public Vue() { }

        private void Menu(List<string> menu)
        {
            foreach (string menu1 in menu)   // Show options
            {
                Console.WriteLine((menu.IndexOf(menu1) + 1) + " - " + menu1);
            }
        }

        internal int SelectMenu(List<string> menu)  // Show different options available and make them selectable
        {
            Console.Clear();
    
            Menu(menu);


            bool validated = false;
            int res = 0;
            while (!validated)
            {
                try
                {
                    res = Convert.ToInt32(Console.ReadLine());
                    validated = true;
                }
                catch
                {
                    Console.Clear();
                    Menu(menu);
                }
            }
            return res;
        }

        //Retourne la valeur de la ressource de chaîne spécifiée.
        public static string getTraductor(string word)
        {
            return rm.GetString(word);
        }
    }

}

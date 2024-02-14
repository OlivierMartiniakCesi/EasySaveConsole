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
        private static ResourceManager rm = new ResourceManager("EasySaveConsole.Resources.TextEnglish", Assembly.GetExecutingAssembly());

        public Vue() { }

        private void Menu(List<string> menu)
        {
            foreach (string menuItem in menu)   // Afficher les options
            {
                Console.WriteLine((menu.IndexOf(menuItem) + 1) + " - " + menuItem);
            }
        }

        internal int SelectMenu(List<string> menu)
        {
            Console.Clear();

            Console.WriteLine(" ### ###    ##      ## ##   ##  ##    ## ##     ##     ### ###  ### ###");
            Console.WriteLine("  ##  ##     ##    ##   ##  ##  ##   ##   ##     ##     ##  ##   ##  ##");
            Console.WriteLine("  ##       ## ##   ####     ##  ##   ####      ## ##    ##  ##   ##    ");
            Console.WriteLine("  ## ##    ##  ##   #####    ## ##    #####    ##  ##   ##  ##   ## ## ");
            Console.WriteLine("  ##       ## ###      ###    ##         ###   ## ###   ### ##   ##    ");
            Console.WriteLine("  ##  ##   ##  ##  ##   ##    ##     ##   ##   ##  ##    ###     ##  ##");
            Console.WriteLine(" ### ###  ###  ##   ## ##     ##      ## ##   ###  ##     ##    ### ###");

            Console.WriteLine("\n\n");

            // Afficher le menu en utilisant la méthode Menu fournie
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
                    // Réafficher le menu en cas d'erreur de saisie
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

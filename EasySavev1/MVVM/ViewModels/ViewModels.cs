using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasySavev1.MVVM.Models;
using EasySavev1.MVVM.Views;

namespace EasySavev1.MVVM.ViewModels
{
    class ViewModels
    {
        private static Backup _backup = new Backup();
        private static List<Backup> BackupListInfo = new List<Backup>();
        private static string Choice{get; set;}

        public static int mainInterface()
        {
            bool exit = false;
            int IChoice;
            Console.WriteLine(" ### ###    ##      ## ##   ##  ##    ## ##     ##     ### ###  ### ###");
            Console.WriteLine("  ##  ##     ##    ##   ##  ##  ##   ##   ##     ##     ##  ##   ##  ##");
            Console.WriteLine("  ##       ## ##   ####     ##  ##   ####      ## ##    ##  ##   ##    ");
            Console.WriteLine("  ## ##    ##  ##   #####    ## ##    #####    ##  ##   ##  ##   ## ## ");
            Console.WriteLine("  ##       ## ###      ###    ##         ###   ## ###   ### ##   ##    ");
            Console.WriteLine("  ##  ##   ##  ##  ##   ##    ##     ##   ##   ##  ##    ###     ##  ##");
            Console.WriteLine(" ### ###  ###  ##   ## ##     ##      ## ##   ###  ##     ##    ### ###");

            Console.WriteLine("\n\n");

            Console.WriteLine("\n1- Create");
            Console.WriteLine("\n2- Launch");
            Console.WriteLine("\n3- Edit");
            Console.WriteLine("\n4- Language");
            Console.WriteLine("\n5- Exit");

            Choice = Console.ReadLine();
            IChoice = int.Parse(Choice);


            while (exit == false)
            {
                switch (IChoice)
                {
                    case 1:
                        // dcez
                        break;
                    case 2:
                        // dzad
                        break;
                    case 3:
                        // dcez
                        break;
                    case 4:
                        // dzead
                        break;
                    case 5:
                        exit = true;
                        Environment.Exit(0);
                        break;

                }
            }
            return 0;
        }
    }
}

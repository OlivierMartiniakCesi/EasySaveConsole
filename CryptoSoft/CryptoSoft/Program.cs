using System;
using System.IO;
using System.Diagnostics;

namespace CryptoSoft
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Veuillez saisir l'emplacement du fichier :");
            string filePath = Console.ReadLine();
            string key = @"C:\Users\olivi\Downloads\CryptoSoft\CryptoSoft\CryptoSoft\Key\KeyXOR.txt";
            CryptoXOR FileEncrypted = new CryptoXOR(filePath, key);

            FileEncrypted.TransformFile();
        }
    }
}

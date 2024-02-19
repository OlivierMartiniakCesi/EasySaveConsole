using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CryptoSoft
{
    class CryptoXOR
    {
        private static string _filePath;
        private static string _key;

        public CryptoXOR(string path, string key)
        {
            _filePath = path;
            _key = key;
        }

        private string ReadKeyFromFile(string keyFilePath)
        {
            _key = keyFilePath;

            if (!File.Exists(keyFilePath))
            {
                Console.WriteLine("Key file not found.");
                Environment.Exit(1);
            }

            string key = File.ReadAllText(keyFilePath);

            if (key.Length < 8)
            {
                key = key.PadRight(8, '0');
            }
            else if (key.Length > 8)
            {
                key = key.Substring(0, 8);
            }

            return key;
        }
        public void TransformFile()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("File not found.");
                Environment.Exit(1);
            }

            byte[] fileBytes = File.ReadAllBytes(_filePath);
            byte[] keyBytes = Encoding.UTF8.GetBytes(ReadKeyFromFile(_key));

            byte[] encryptedBytes = XorMethod(fileBytes, keyBytes);

            // Change the destination path as needed
            string encryptedFilePath = _filePath + ".encrypted";
            File.WriteAllBytes(encryptedFilePath, encryptedBytes);

            Console.WriteLine("File encrypted successfully: " + encryptedFilePath);
        }


        private static byte[] XorMethod(byte[] fileBytes, byte[] keyBytes)
        {
            byte[] result = new byte[fileBytes.Length];
            for (int i = 0; i < fileBytes.Length; i++)
            {
                result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return result;
        }
    }
}

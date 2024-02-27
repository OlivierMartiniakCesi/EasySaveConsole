using System;
using System.IO;

namespace CryptSoft
{
    public class Program
    {
        private const string SourceDirectoryArg = "file.FullName";
        private const string TargetDirectoryArg = "targetFilePath";
        private const int KeyLength = 8;

        static int Main(string[] args)
        {
            if (args.Length % 2 != 0)
            {
                Console.WriteLine("Invalid number of arguments");
                return -1;
            }

            string source = "";
            string destination = "";

            for (int i = 0; i < args.Length; i += 2)
            {
                string argName = args[i];
                string argValue = args[i + 1];

                switch (argName)
                {
                    case SourceDirectoryArg:
                        source = argValue;
                        break;
                    case TargetDirectoryArg:
                        destination = argValue;
                        break;
                    default:
                        Console.WriteLine($"Unknown argument: {argName}");
                        return -1;
                }
            }

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination))
            {
                Console.WriteLine("Missing arguments");
                return -1;
            }

            if (!File.Exists(source))
            {
                Console.WriteLine("Source file doesn't exist.");
                return -1;
            }

            try
            {
                DateTime startTimeFile = DateTime.Now;

                byte[] byteToEncrypt = File.ReadAllBytes(source);
                byte[] byteCrypted = Encrypt(byteToEncrypt);

                File.WriteAllBytes(destination, byteCrypted);

                TimeSpan cryptTime = DateTime.Now - startTimeFile;
                Console.WriteLine($"Encryption successful. Time taken: {cryptTime.TotalMilliseconds} ms");
                return 0;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error accessing file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return -1;
        }

        private static byte[] Encrypt(byte[] data)
        {
            byte[] byteKey = new byte[KeyLength] { 12, 200, 100, 100, 5, 20, 100, 100 };
            byte[] byteCrypted = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                byteCrypted[i] = (byte)(data[i] ^ byteKey[i % KeyLength]);
            }

            return byteCrypted;
        }
    }
}

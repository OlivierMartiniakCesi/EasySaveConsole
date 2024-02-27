using System;
using System.Collections;
using System.IO;

namespace CryptSoft
{
    using System;
    using System.IO;

    public class Program
    {
        static int Main(string[] args)
        {
            // Key for XOR encryption
            byte[] encryptionKey = { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x11, 0x22 };

            int argsSize = args.Length;
            string src = "";
            string dst = "";

            for (int i = 0; i < argsSize; i++)
            {
                if (args[i] == "source" && i + 1 < argsSize)
                {
                    src = args[i + 1];
                    i++;
                }
                else if (args[i] == "destination" && i + 1 < argsSize)
                {
                    dst = args[i + 1];
                    i++;
                }
            }

            if (src.Length == 0 || dst.Length == 0)
            {
                Console.WriteLine("Missing arguments");
                return -1;
            }
            else if (!File.Exists(src))
            {
                Console.WriteLine("Source file doesn't exist.");
                return -1;
            }

            try
            {
                DateTime startTimeFile = DateTime.Now;

                byte[] bytesToEncrypt = File.ReadAllBytes(src);

                // XOR encryption
                byte[] encryptedBytes = new byte[bytesToEncrypt.Length];
                for (int i = 0; i < bytesToEncrypt.Length; i++)
                {
                    encryptedBytes[i] = (byte)(bytesToEncrypt[i] ^ encryptionKey[i % encryptionKey.Length]);
                }

                File.WriteAllBytes(dst, encryptedBytes);

                TimeSpan cryptTime = DateTime.Now - startTimeFile;
                return (int)cryptTime.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot crypt this file: {ex.Message}");
                return -1;
            }
        }
    }
}

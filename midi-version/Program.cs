using System;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib;

namespace ce_lua_decode
{
    class Program
    {
        static void Main(string[] args)
        {
            Banner();

            if (args.Length != 2)
            {
                Usage();
            }
            else
            {
                string arg0 = args[0];
                string arg1 = args[1];

                switch (arg0)
                {
                    case "-f":
                        DecodeFile(arg1);
                        break;
                    case "-s":
                        DecodeString(arg1);
                        break;
                    default:
                        Console.WriteLine("Unknown switch");
                        break;
                }
            }
        }

        private static void DecodeFile(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine("File not found");
                return;
            }

            string content = File.ReadAllText(filename);
            DecodeString(content);
        }

        private static void DecodeString(string str)
        {
            DateTime datetime = DateTime.Now;

            using (FileStream fs = new FileStream($"output_{datetime.ToShortDateString().Replace("/", "")}_{datetime.ToShortTimeString().Replace(":", "")}.chunk", FileMode.Create))
            {
                MemoryStream ms = new MemoryStream();

                CEBase85.TryBase85ToBin(str, ref ms);

                using (ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream zs = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.InflaterInputStream(ms))
                {
                    zs.CopyTo(fs);
                    fs.Flush();
                }

                ms.Dispose();
            }
        }

        private static void Banner()
        {
            Console.WriteLine("ce-lua-decode");
            Console.WriteLine("A toy to decode lua bytecode embedded into Cheat Engine \"decodeFunction\" parameters");
        }

        private static void Usage()
        {
            Console.WriteLine("\nUsage :");
            Console.WriteLine("ce-lua-decode [-f filename | -s raw_string]");
            Console.WriteLine("\tSwitch -f filename : run the program with a file containing the string to decode to bytecode");
            Console.WriteLine("\tSwitch -s raw_string : run the program with a string to decode to bytecode");
        }
    }
}

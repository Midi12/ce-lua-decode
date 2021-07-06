using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEtrainer_extract
{
    public static class ScriptHandler
    {
        public static void HandleScripts(string path)
        {
            var trainer = File.ReadAllText(path);
            var scripts = FindScripts(trainer);

            if(scripts.Length == 0)
            {
                Log.Critical("No scripts found.");
                return;
            }

            int index = 0;
            foreach(var script in scripts)
            {
                var decompressed = DecompressScript(script, index);
                if(decompressed.Length > 0)
                {
                    File.WriteAllBytes("cetrainer_script_" + index + ".luac", decompressed);
                    Log.Info("Decompressed cetrainer_script_" + index + " successfully.");
                    index++;
                }
            }

            Console.WriteLine();
            Log.Critical("Decompressed scripts.");
        }

        private static byte[] DecompressScript(string compressed, int index)
        {
            try
            {
                int length1 = 4 * (compressed.Length / 5);
                int length2 = compressed.Length;
                char[] outBuffer = new char[(length2 % 5 + length1)];

                Base85ToBin(compressed.ToCharArray(), ref outBuffer);
                return ZlibStream.UncompressBuffer(outBuffer.Select(c => (byte)c).ToArray());
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to decompress script cetrainer_script_{index}: {ex.Message}");
                return new byte[0];
            }
        }

        private static string[] FindScripts(string trainer)
        {
            List<string> scripts = new List<string>();

            string remainingText = trainer;
            while(remainingText.IndexOf("decodeFunction(") != -1)
            {
                var indexOfStart = remainingText.IndexOf("decodeFunction(") + 16;
                var stringTerminator = remainingText[indexOfStart-1];
                var script = remainingText.Substring(indexOfStart);
                script = script.Remove(script.IndexOf(stringTerminator));

                scripts.Add(script);
                remainingText = remainingText.Substring(indexOfStart + script.Length);
            }

            return scripts.ToArray();
        }


        const string customBase85 = "0123456789" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "!#$%()*+,-./:;=?@[]^_{}";


        //this is the same function, but taken right out of IDA. somehow this only works for scripts
        private static long Base85ToBin(char[] a1, ref char[] a2)
        {
            int v2 = 0; // er13
            int v5; // edi
            int v6; // er14
            uint v7; // er12
            long v8; // rdx
            long v9; // rdx
            long v10; // rdx
            int v11; // er13
            int v12; // er13

            v5 = a1.Length;
            v6 = 0;
            v7 = 0;
            while (v5 > v6)
            {
                v2 = 52200625
                   * ((int)customBase85.IndexOf(a1[v6++])
                    );
                if (v5 > v6)
                    v2 += 614125
                        * ((int)customBase85.IndexOf(a1[v6++])
                         );
                if (v5 > v6)
                    v2 += 7225
                        * ((int)customBase85.IndexOf(a1[v6++])
                         );
                if (v5 > v6)
                    v2 += 85
                        * ((int)customBase85.IndexOf(a1[v6++])
                         );
                if (v5 > v6)
                {
                    v2 += ((int)customBase85.IndexOf(a1[v6++])
                        );
                    a2[v7] = (char)HIBYTE(v2);
                    a2[v7 + 1] = (char)BYTE2(v2);
                    a2[v7 + 2] = (char)BYTE1(v2);
                    a2[v7 + 3] = (char)v2;
                    v7 += 4;
                }
            }
            v8 = v5 % 5;
            if (v8 >= 2)
            {
                v9 = v8 - 2;
                if (v9 != 0)
                {
                    v10 = v9 - 1;
                    if (v10 != 0)
                    {
                        if (v10 == 1)
                        {
                            v12 = v2 + 84;
                            a2[v7] = (char)HIBYTE(v12);
                            a2[v7 + 1] = (char)BYTE2(v12);
                            a2[v7 + 2] = (char)BYTE1(v12);
                            v7 += 3;
                        }
                    }
                    else
                    {
                        v11 = v2 + 7224;
                        a2[v7] = (char)HIBYTE(v11);
                        a2[v7 + 1] = (char)BYTE2(v11);
                        v7 += 2;
                    }
                }
                else
                {
                    a2[v7++] = (char)((v2 + 614124) >> 24);
                }
            }
            return v7;
        }

        public static byte BYTE1(int x)
        {
            return BYTEn(x, 1);
        }

        public static byte BYTE2(int x)
        {
            return BYTEn(x, 2);
        }

        public static unsafe byte BYTEn(int x, int n)
        {
            return (*((byte*)&(x) + n));
        }

        public static byte HIBYTE(int x)
        {
            return BYTEn(x, LAST_IND());
        }

        public static int LAST_IND()
        {
            return (sizeof(int) / sizeof(byte) - 1);
        }
    }
}

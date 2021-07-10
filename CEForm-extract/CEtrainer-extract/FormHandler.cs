using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CEtrainer_extract
{
    public static class FormHandler
    {
        public static void DecompressForms(string path)
        {
            try
            {
                var xml = XDocument.Load(path);
                var forms = xml.Descendants().First().Descendants().First().Descendants();
                Console.WriteLine();

                foreach (var form in forms)
                {
                    if (form.Attribute("Encoding").Value.Contains("85"))
                    {
                        DecompressForm(form.Name.ToString(), form.Value.ToString());
                        Log.Info($"Decompressed {form.Name} -> {form.Name}.dfm");
                    }
                    else
                        Log.Error($"Form {form} is not encoded with Ascii85, this is not supported yet.");
                }

                Console.WriteLine();
                Log.Critical("Forms decompressed.");
                Console.WriteLine();
            }
            catch
            {
                Log.Fatal("Could not parse XML document.");
            }
        }


        private static void DecompressForm(string formName, string content)
        {
            char[] b = new char[0];
            int size = 0;

            try
            {

                size = (content.Length / 5) * 4 + (content.Length % 5);
                b = new char[size * 2];
                size = (int)Base85ToBin(content.ToCharArray(), ref b);
                byte[] buffer = b.Select(c => (byte)c).ToArray();

                File.WriteAllBytes(formName + ".dfm", Decompress(buffer));
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to decompress {formName} -> {ex.Message}");
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream compressStream = new MemoryStream(data))
                {
                    using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressedStream);
                    }
                }
                decompressedArray = decompressedStream.ToArray();
            }

            return decompressedArray;
        }


        const string customBase85 = "0123456789" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "!#$%()*+,-./:;=?@[]^_{}";

        // this is darkbytes weird pascal code in C#, somehow this only works for forms
        private static long Base85ToBin(char[] inputStringBase85, ref char[] BinValue)
        {
            int i, j;
            int size;
            uint a = 0;

            size = inputStringBase85.Length;
            i = 0;
            j = 0;

            while (i < size)
            {
                a = ((uint)customBase85.IndexOf(inputStringBase85[i])) * 85 * 85 * 85 * 85;
                ++i;

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i])) * 85 * 85 * 85;
                    ++i;
                }

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i])) * 85 * 85;
                    ++i;
                }

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i])) * 85;
                    ++i;
                }

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i]));
                    ++i;

                    var lol = Convert.ToChar((a >> 24) & 0xFF);
                    BinValue[j + 0] = lol;
                    BinValue[j + 1] = Convert.ToChar((a >> 16) & 0xFF);
                    BinValue[j + 2] = Convert.ToChar((a >> 8) & 0xFF);
                    BinValue[j + 3] = Convert.ToChar(a & 0xFF);
                    j += 4;

                }
            }

            switch (size % 5)
            {
                case 2:
                    a = a + 84 * 85 * 85 + 84 * 85 + 84;
                    BinValue[j + 0] = Convert.ToChar((a >> 24) & 0xFF);
                    ++j;
                    break;
                case 3:
                    a = a + 84 * 85 + 84;
                    BinValue[j + 0] = Convert.ToChar((a >> 24) & 0xFF);
                    BinValue[j + 1] = Convert.ToChar((a >> 16) & 0xFF);
                    j += 2;
                    break;
                case 4:
                    a = a + 84;
                    BinValue[j + 0] = Convert.ToChar((a >> 24) & 0xFF);
                    BinValue[j + 1] = Convert.ToChar((a >> 16) & 0xFF);
                    BinValue[j + 2] = Convert.ToChar((a >> 8) & 0xFF);
                    j += 3;
                    break;
            }

            return j;

        }
    }
}

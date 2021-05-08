using ICSharpCode.SharpZipLib.GZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEForm_extract
{
    class Program
    {
        static void Main(string[] args)
        {
            var content = File.ReadAllText("trainer.txt");
            LoadFromXML(content);
        }


        public static  void LoadFromXML(string content)
        {
            string s;
            char[] b = new char[0];
            MemoryStream m;
            MemoryStream dc = new MemoryStream();
            int size;
            int read;
            uint realsize;
            bool wasActive;
            bool useascii85;

            s = content;
            useascii85 = true;

            try
            {
                if (useascii85)
                {
                    size = (s.Length / 5) * 4 + (s.Length % 5);
                    b = new char[size];
                    size = Base85ToBin(s.ToCharArray(), ref b);
                }
            }
            catch
            {

            }
            byte[] buffer = b.Select(c => (byte)c).ToArray();
            
            m = new MemoryStream(buffer);
            m.Seek(0, SeekOrigin.Begin);


            GZip.Decompress(m, dc, false);

            dc.Seek(0, SeekOrigin.Begin);

            byte[] buff = new byte[sizeof(uint)];
            
            dc.Read(buff, 0, buff.Length);
            realsize = BitConverter.ToUInt32(buff, 0);

            byte[] finalBuffer = new byte[realsize];
            read = dc.Read(finalBuffer, 0, finalBuffer.Length);

            File.WriteAllBytes("decompressed.txt", finalBuffer);
        }


        const string customBase85 = "0123456789" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "!#$%()*+,-./:;=?@[]^_{}";


        public static int Base85ToBin(char[] inputStringBase85, ref char[] BinValue)
        {
            int i, j;
            int size;
            uint a = 0;

            size = inputStringBase85.Length;
            i = 0;
            j = 0;

            while(i < size)
            {
                a = ((uint)customBase85.IndexOf(inputStringBase85[i]) - 1) * 85 * 85 * 85 * 85;
                ++i;

                if(i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i]) - 1) * 85 * 85 * 85;
                    ++i;
                }

                if(i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i]) - 1) * 85 * 85;
                    ++i;
                }

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i]) - 1) * 85;
                    ++i;
                }

                if (i < size)
                {
                    a = a + ((uint)customBase85.IndexOf(inputStringBase85[i]) - 1);
                    ++i;

                    BinValue[j + 0] = Convert.ToChar((a >> 24) & 0xFF);
                    BinValue[j + 1] = Convert.ToChar((a >> 16) & 0xFF);
                    BinValue[j + 2] = Convert.ToChar((a >> 8) & 0xFF);
                    BinValue[j + 3] = Convert.ToChar(a & 0xFF);
                    j += 4;

                }
            }
            
            switch(size % 5)
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

using System;
using System.IO;

namespace ce_lua_decode
{
    public static class CEBase85
    {
        private static string _base85alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%()*+,-./:;=?@[]^_{}";

        public static bool TryBase85ToBin(string input, ref MemoryStream output)
        {
            bool success = false;
            
            try {
                Base85ToBin(input, ref output);
            }
            catch (Exception ex) {
                success = false;
            }

            return success;
        }

        private static void Base85ToBin(string input, ref MemoryStream output)
        {
            int inputSize = input.Length;
            int i = 0;
            UInt32 value = 0;

            while (i < inputSize)
            {
                char c = input[i];
                value = Convert.ToUInt32(_base85alphabet.IndexOf(c)) * Convert.ToUInt32(Math.Pow(85, 4));
                i += 1;

                if (i < inputSize)
                {
                    c = input[i];
                    value += Convert.ToUInt32(_base85alphabet.IndexOf(c)) * Convert.ToUInt32(Math.Pow(85, 3));
                    i += 1;
                }

                if (i < inputSize)
                {
                    c = input[i];
                    value += Convert.ToUInt32(_base85alphabet.IndexOf(c)) * Convert.ToUInt32(Math.Pow(85, 2));
                    i += 1;
                }

                if (i < inputSize)
                {
                    c = input[i];
                    value += Convert.ToUInt32(_base85alphabet.IndexOf(c)) * Convert.ToUInt32(Math.Pow(85, 1));
                    i += 1;
                }

                if (i < inputSize)
                {
                    value += Convert.ToUInt32(_base85alphabet.IndexOf(c));
                    i += 1;

                    output.WriteByte(Convert.ToByte((value >> 24) & 0xFF));
                    output.WriteByte(Convert.ToByte((value >> 16) & 0xFF));
                    output.WriteByte(Convert.ToByte((value >> 8) & 0xFF));
                    output.WriteByte(Convert.ToByte(value & 0xFF));
                }
            }

            switch (inputSize % 5)
            {
                case 2:
                    value += 84 * 85 + 84;
                    output.WriteByte(Convert.ToByte((value >> 24) & 0xFF));
                    break;
                case 3:
                    value += 84 * Convert.ToUInt32(Math.Pow(85, 2)) + 84 * 85 + 84;
                    output.WriteByte(Convert.ToByte((value >> 24) & 0xFF));
                    output.WriteByte(Convert.ToByte((value >> 16) & 0xFF));
                    break;
                case 4:
                    value += 84;
                    output.WriteByte(Convert.ToByte((value >> 24) & 0xFF));
                    output.WriteByte(Convert.ToByte((value >> 16) & 0xFF));
                    output.WriteByte(Convert.ToByte((value >> 8) & 0xFF));
                    break;
                default:
                    break;
            }

            output.Seek(0, SeekOrigin.Begin);
        }
    }
}

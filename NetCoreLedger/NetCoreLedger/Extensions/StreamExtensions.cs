using System;
using System.IO;
using System.Text;
using NetCoreLedger.Business;

namespace NetCoreLedger.Extensions
{
    public static class StreamExtensions
    {
        public static void WriteInt(this Stream stream, uint i)
        {
            var bytes = BitConverter.GetBytes(i);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteString(this Stream stream, string s)
        {
            //UnicodeEncoding uniEncoding = new UnicodeEncoding();
            //stream.Write(uniEncoding.GetBytes(s), 0, s.Length);

            var bytes = Encoding.UTF8.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteX2String(this Stream stream, string s)
        {
            int numberChars = s.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);

            stream.Write(bytes, 0, bytes.Length);

            //var bytes = Encoding.UTF8.GetBytes(s);
            //stream.Write(bytes, 0, bytes.Length);
        }

        public static uint ReadString(this Stream stream, ref string to, int length)
        {
            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            var by = new byte[length];
            var result = (uint)stream.Read(by, 0, length);

            to = Encoding.UTF8.GetString(by);

            return result;
        }

        public static uint ReadX2String(this Stream stream, ref string to, int length)
        {
            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            var by = new byte[length];
            var result = (uint)stream.Read(by, 0, length);

            to = Hasher.GetStringFromHash(by); 

            return result;
        }

        public static uint ReadUInt(this Stream stream, ref uint to)
        {
            var by = new byte[sizeof(uint)];
            var result = (uint)stream.Read(by, 0, sizeof(uint));

            to = BitConverter.ToUInt32(by, 0);
            return result;
        }
    }
}

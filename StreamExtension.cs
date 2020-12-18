using System.IO;
using System.Text;

namespace DMXReader
{
    public static class StreamExtension
    {
        public static long ByteRemaining(this Stream r)
        {
            return r.Length - r.Position;
        }

        public static long ByteRemaining(this BinaryReader r)
        {
            return r.BaseStream.Length - r.BaseStream.Position;
        }

        public static string ReadTerminatedString(this BinaryReader r)
        {
            string str = "";

            while (true)
            {
                int c = r.ReadChar();
                if (c <= 0)
                    break;

                str += (char)c;
            }

            return str;
        }

        public static string ReadString(this BinaryReader r, int count)
        {
            return new string(r.ReadChars(count));
        }

        public static string ReadUString(this BinaryReader r, int count)
        {
            return Encoding.ASCII.GetString(r.ReadBytes(count));
        }
    }
}

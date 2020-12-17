using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PCFReader
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
    }

    public class ParticleInfo
    {

    }

    public class PFC
    {
        protected class Header
        {
            public bool IsBinary => Encoding == "binary";
            public int Length = 0;

            public string Encoding = "binary";
            public int EncodingVersion = 0;

            public string Format = "pcf";
            public int FormatVersion = 0;
        }

        protected Header header;
        protected string FileName;
        protected string FilePath;

        readonly string DMX_HEADER_PATTERN = @"<!-- dmx encoding (\w+) (\d+) format (\w+) (\d+) -->\n";

        readonly int DMX_BINARY_VER_STRINGTABLE = 2;
        readonly int DMX_BINARY_VER_GLOBAL_STRINGTABLE = 4;
        readonly int DMX_BINARY_VER_STRINGTABLE_LARGESYMBOLS = 5;

        readonly int CURRENT_BINARY_ENCODING = 5;
        readonly int DMX_MAX_FORMAT_NAME_MAX_LENGTH = 64;
        private int DMX_MAX_HEADER_LENGTH
        {
            set { }
            get
            {
                return 40 + 2 * DMX_MAX_FORMAT_NAME_MAX_LENGTH;
            }
        }

        public List<ParticleInfo> particles;

        public PFC(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(path);
        }

        public bool CreateHeader()
        {
            using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read)))
            {
                string headerString = reader.ReadTerminatedString();
                Regex regex = new Regex(DMX_HEADER_PATTERN);
                Match match = regex.Match(headerString);
                if (!match.Success)
                    return false;

                header = new Header
                {
                    Encoding = match.Groups[1].Value,
                    EncodingVersion = int.Parse(match.Groups[2].Value),
                    Format = match.Groups[3].Value,
                    FormatVersion = int.Parse(match.Groups[4].Value)
                };

                if (header.FormatVersion == 0)
                {
                    Console.WriteLine("reading file '%s' of legacy format '%s' - dmxconvert this file to a newer format!\n", FileName, header.Format);
                }

                header.Length = Encoding.ASCII.GetByteCount(headerString);
                reader.Close();
                return header.Format == "pcf";
            }
        }

        public void GetParticles()
        {
            if (header == null) return;

            using (BinaryReader reader = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read)))
            {
                if (header.IsBinary)
                    Unserialize(reader);
                else
                    Console.WriteLine("Only binary files are currently supported.");
            }
        }

        private bool Unserialize(BinaryReader reader)
        {
            if (header.EncodingVersion < 0 || header.EncodingVersion > CURRENT_BINARY_ENCODING)
                return false;

            bool readStringTable = header.EncodingVersion >= DMX_BINARY_VER_STRINGTABLE;
            bool useLargeSymbols = header.EncodingVersion >= DMX_BINARY_VER_STRINGTABLE_LARGESYMBOLS;

            reader.BaseStream.Seek(header.Length, SeekOrigin.Begin);

            while (reader.ReadChar() != 0)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    return false;
            }

            int stringCount = 0;
            if (readStringTable)
                stringCount = header.EncodingVersion >= DMX_BINARY_VER_GLOBAL_STRINGTABLE ? reader.ReadInt32() : reader.ReadInt16();

            string[] stringTable = null;
            if (stringCount > 0)
                stringTable = GetStringTable(reader, stringCount);

            int elementCount = reader.ReadInt32();
            if (elementCount == 0)
                return true;

            if (elementCount < 0 || (readStringTable && stringTable == null))
                return false;

            for (int i = 0; i < elementCount; ++i)
            {
                string name;
                if (stringTable != null)
                {
                    name = GetStringFromBuffer(reader, useLargeSymbols, stringTable);
                }
                else
                    name = reader.ReadString(256);

                Console.WriteLine("From Buffer: " + name);
            }

            return true;
        }

        private string GetStringFromBuffer(BinaryReader reader, bool useLargeSymbols, string[] stringTables)
        {
            int i = useLargeSymbols ? reader.ReadInt32() : reader.ReadInt16();
            Console.WriteLine(i);

            return (i > stringTables.Length || i < 0) ? null : stringTables[i];
        }

        private string[] GetStringTable(BinaryReader reader, int stringCount)
        {
            string[] stringTable = new string[stringCount];

            for (int i = 0; i < stringCount; i++)
            {
                stringTable[i] = reader.ReadTerminatedString();
            }

            return stringTable;
        }

        public override string ToString() => "";
    }

    class Program
    {
        static void Main(string[] args)
        {
            PFC particleFile = new PFC("smissmas2020_unusuals.pcf");
            particleFile.CreateHeader();
            particleFile.GetParticles();

            Console.ReadLine();
        }
    }
}
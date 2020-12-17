using System;
using System.IO;
using PCFReader.DMX;

namespace PCFReader
{
    public class PFC
    {
        protected string FileName;
        protected string FilePath;

        public PFC(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(path);
        }

        public void Load()
        {
            Deserialiser dmx = new Deserialiser(FilePath);
            dmx.Unserialize();
        }

        public override string ToString() => "";
    }

    class Program
    {
        static void Main(string[] args)
        {
            PFC particleFile = new PFC("smissmas2020_unusuals.pcf");
            particleFile.Load();

            Console.ReadLine();
        }
    }
}
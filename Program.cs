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
#if DEBUG
            PFC particleFile = new PFC("example.pcf");
            particleFile.Load();
#else
            if(args == null || args.Length == 0)
            {
                Console.WriteLine("Please drag and drop a pfc file onto this exe.");
                return;
            }

            PFC particleFile = new PFC(args[0]);
            particleFile.Load();
#endif

            Console.ReadLine();
        }
    }
}
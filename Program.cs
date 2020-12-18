using System;
using DMXReader.DMX;

namespace DMXReader
{
    class Program
    {
        static void Main(string[] args)
        {

#if DEBUG
            Deserialiser dmx = new Deserialiser("example.pcf");
            dmx.Unserialize();
#else
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Please drag and drop a pcf file onto this exe.");
                return;
            }

            Deserialiser dmx = new Deserialiser(args[0]);
            dmx.Unserialize();
#endif

            Console.ReadLine();
        }
    }
}
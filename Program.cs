using System;
using System.Collections.Generic;
using DMXReader.DMX;

namespace DMXReader
{
    class Program
    {
        static string Tab(int i = 0)
        {
           return new string('\t', i);
        }

        static void PrintElements(DXElement[] elements, int depth = 0)
        {
            foreach (DXElement element in elements)
            {
                Console.WriteLine($"{Tab(depth)}[Element: {element.Id} | {element.Type} - {element.Name}]");
                PrintAttributes(element.Attributes, depth);
            }
        }

        static void PrintAttributes(List<DXAttribute> attributes, int depth = 0)
        {
            foreach (DXAttribute attribute in attributes)
            {
                if (attribute.Type != DmAttributeType_t.AT_ELEMENT || attribute.Type != DmAttributeType_t.AT_ELEMENT_ARRAY)
                {
                    Console.WriteLine($"{Tab(depth)}\t[Attributes: {attribute.Name} - {attribute.Value}]");
                }
            }

            foreach (DXAttribute attribute in attributes)
            {
                if (attribute.Type == DmAttributeType_t.AT_ELEMENT)
                {
                    PrintElements(new DXElement[1] { (DXElement)attribute.Value }, depth++);;
                }
                else if (attribute.Type == DmAttributeType_t.AT_ELEMENT_ARRAY)
                {
                    PrintElements((DXElement[])attribute.Value, depth++);
                }
            }
        }

        static void Main(string[] args)
        {
            Deserialiser dmx;

#if DEBUG
            dmx = new Deserialiser("example.pcf");
#else
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Please drag and drop a pcf file onto this exe.");
                return;
            }

            dmx = new Deserialiser(args[0]);
#endif

            DXElement root = dmx.Unserialize();
            PrintElements(new DXElement[1] { root });

            Console.ReadLine();
        }
    }
}
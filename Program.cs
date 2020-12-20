using DMXReader.DMX;
using System;
using System.Collections.Generic;

namespace DMXReader
{
    class Program
    {
        static string Tab(int i = 0)
        {
            return new string('\t', i);
        }

        static bool DisplayInfo = true;
        static void Log(string str)
        {
            if (DisplayInfo)
                Console.WriteLine(str);
        }

        static List<object> PrintElement(DXElement element, ref List<object> foundAttributeValues, string name = null)
        {
            Log($"[Root Element: {element.Id} | {element.Type} - {element.Name}]");
            PrintAttributes(element.Attributes, depth: 0, find: name, foundAttributeValues: ref foundAttributeValues);

            return foundAttributeValues;
        }

        static void PrintElements(DXElement[] elements, ref List<object> foundAttributeValues, int depth = 0, string find = null)
        {
            foreach (DXElement element in elements)
            {
                Log($"{Tab(depth)}[Element: {element.Id} | {element.Type} - {element.Name}]");
                PrintAttributes(element.Attributes, depth: depth, find: find, foundAttributeValues: ref foundAttributeValues);
            }
        }

        static void PrintAttributes(List<DXAttribute> attributes, ref List<object> foundAttributeValues, int depth = 0, string find = null)
        {
            foreach (DXAttribute attribute in attributes)
            {
                if (attribute.Type != DmAttributeType_t.AT_ELEMENT || attribute.Type != DmAttributeType_t.AT_ELEMENT_ARRAY)
                {
                    if (find != null && attribute.Name.Contains(find))
                    {
                        if (!foundAttributeValues.Contains(attribute.Value))
                            foundAttributeValues.Add(attribute.Value);
                    }
                    else
                    {
                        Log($"{Tab(depth)}\t[Attributes: {attribute.Name} - {attribute.Value}]");
                    }
                }
            }

            foreach (DXAttribute attribute in attributes)
            {
                if (attribute.Type == DmAttributeType_t.AT_ELEMENT)
                {
                    PrintElements(new DXElement[1] { (DXElement)attribute.Value }, depth: depth++, find: find, foundAttributeValues: ref foundAttributeValues);
                }
                else if (attribute.Type == DmAttributeType_t.AT_ELEMENT_ARRAY)
                {
                    PrintElements((DXElement[])attribute.Value, depth: depth++, find: find, foundAttributeValues: ref foundAttributeValues);
                }
            }
        }

        static void Main(string[] args)
        {
            List<Deserialiser> dmxList = new List<Deserialiser>();

#if DEBUG
            Deserialiser dmx = new Deserialiser("example.pcf");
            dmx.Unserialize();
            dmxList.Add(dmx);
#else
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Please drag and drop a pcf file onto this exe.");
                return;
            }

            for (int i = 0; i < args.Length; ++i)
            {
                Deserialiser dmx = new Deserialiser(args[i]);
                dmx.Unserialize();
                dmxList.Add(dmx);
            }
#endif

            Console.WriteLine("Type exit, print or search");

            bool cycle = true;
            while (cycle)
            {
                string input = Console.ReadLine().ToLower().Trim();
                switch (input)
                {
                    case "exit":
                        cycle = false;
                        break;
                    case "print":
                        DisplayInfo = true;
                        dmxList.ForEach(dmx =>
                        {
                            Console.WriteLine(dmx.FileName);
                            // TODO: This is dumb
                            List<object> foundList = new List<object>();
                            PrintElement(dmx.RootElement, ref foundList);
                        });
                        break;
                    case "search":
                        Console.Write("Type the attribute name to list: ");
                        string name = Console.ReadLine().Trim();

                        DisplayInfo = false;

                        string output = "";
                        List<object> foundAttributeValues = new List<object>();
                        dmxList.ForEach(dmx =>
                        {
                            List<object> values = PrintElement(dmx.RootElement, ref foundAttributeValues, name);
                            foreach (object value in values)
                            {
                                Console.WriteLine(value);
                                output += value + Environment.NewLine;
                            }
                        });

                        System.IO.File.WriteAllText("dmxsearch.txt", output);
                        break;
                }
            }
        }
    }
}
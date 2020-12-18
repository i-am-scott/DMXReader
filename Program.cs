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

        static List<object> PrintElement(DXElement element, string name = null)
        {
            List<object> foundAttributeValues = new List<object>();

            Console.WriteLine($"[Root Element: {element.Id} | {element.Type} - {element.Name}]");
            PrintAttributes(element.Attributes, depth: 0, find: name, foundAttributeValues: ref foundAttributeValues);

            return foundAttributeValues;
        }

        static void PrintElements(DXElement[] elements, ref List<object> foundAttributeValues, int depth = 0, string find = null)
        {
            foreach (DXElement element in elements)
            {
                Console.WriteLine($"{Tab(depth)}[Element: {element.Id} | {element.Type} - {element.Name}]");
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
                        Console.WriteLine($"{Tab(depth)}\t[Attributes: {attribute.Name} - {attribute.Value}]");
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

            Console.WriteLine("Type exit, print or list");

            bool cycle = true;
            while(cycle)
            {
                string input = Console.ReadLine().ToLower();
                switch(input)
                {
                    case "exit":
                        cycle = false;
                        break;
                    case "print":
                        PrintElement(root);
                        break;
                    case "list":
                        Console.Write("Type the attribute name to list: ");
                        string name = Console.ReadLine();
                        List<object> values = PrintElement(root, name);
                        string output = "";

                        foreach (object value in values)
                        {
                            Console.WriteLine(value);
                            output += value + Environment.NewLine;
                        }

                        System.IO.File.WriteAllText("output.txt", output);
                        break;
                }
            }
        }
    }
}
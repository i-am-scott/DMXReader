using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DMXReader.DMX
{
    public class DXElement
    {
        public string Id;
        public string Type;
        public string Name;
        public List<DXAttribute> Attributes = new List<DXAttribute>();

        public void AddAttribute(DXAttribute attrib)
        {
            Attributes.Add(attrib);
        }

        public override string ToString()
        {
            return $"Element: {Id}, {Type}, {Name}";
        }
    }

    public class DXAttribute
    {
        public DmAttributeType_t Type;
        public string Id;
        public string Name;
        public object Value;

        public override string ToString()
        {
            string val = "";
            switch (Value)
            {
                case DmAttributeType_t.AT_BOOL:
                    val = (bool)Value ? "True" : "False";
                    break;
                case DmAttributeType_t.AT_FLOAT:
                    val = Convert.ToString((Single)Value);
                    break;
                case DmAttributeType_t.AT_INT:
                    val = Convert.ToString((int)Value);
                    break;
                case DmAttributeType_t.AT_ELEMENT:
                    DXElement el = (DXElement)Value;
                    val = el.ToString();
                    break;
                default:
                    val = Value.ToString();
                    break;
            }

            return Name + " " + val;
        }
    }

    public class DMXHeader
    {
        public bool IsBinary => Encoding == "binary";
        public int Length = 0;

        public string Encoding = "binary";
        public int EncodingVersion = 0;

        public string Format = "pcf";
        public int FormatVersion = 0;

        public string[] StringTable;
    }

    public class Deserialiser
    {
        readonly string DMX_HEADER_PATTERN = @"<!-- dmx encoding (\w+) (\d+) format (\w+) (\d+) -->\n";
        readonly int DMX_BINARY_VER_STRINGTABLE = 2;
        readonly int DMX_BINARY_VER_GLOBAL_STRINGTABLE = 4;
        readonly int DMX_BINARY_VER_STRINGTABLE_LARGESYMBOLS = 5;
        readonly int CURRENT_BINARY_ENCODING = 5;

        readonly int ELEMENT_INDEX_NULL = -1;
        readonly int ELEMENT_INDEX_EXTERNAL = -2;

        protected string FilePath;
        protected string FileName;

        protected DMXHeader header;

        private bool ReadStringTable => header.EncodingVersion >= DMX_BINARY_VER_STRINGTABLE;
        private bool UseLargeSymbols => header.EncodingVersion >= DMX_BINARY_VER_STRINGTABLE_LARGESYMBOLS;
        private bool IsGlobalStringTable => (ReadStringTable && header.EncodingVersion >= DMX_BINARY_VER_GLOBAL_STRINGTABLE);

        private string[] StringTable;

        private BinaryReader reader;

        public Deserialiser(string path)
        {
            FilePath = path;
            FileName = Path.GetFileName(FilePath);
        }

        public bool ReadHeader()
        {
            reader = new BinaryReader(File.Open(FilePath, FileMode.Open, FileAccess.Read));

            string headerString = reader.ReadTerminatedString();
            Regex regex = new Regex(DMX_HEADER_PATTERN);
            Match match = regex.Match(headerString);
            if (!match.Success)
                return false;

            header = new DMXHeader
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
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            return header.Format == "pcf";
        }

        public bool Unserialize()
        {
            if (!ReadHeader())
                return false;

            if (header.IsBinary)
            {
                return Unserialize(reader);
            }
            else
            {
                Console.WriteLine("Only binary files are currently supported.");
                return false;
            }
        }

        private bool Unserialize(BinaryReader reader)
        {
            if (header.EncodingVersion < 0 || header.EncodingVersion > CURRENT_BINARY_ENCODING)
                return false;

            reader.BaseStream.Seek(header.Length, SeekOrigin.Begin);

            while (reader.ReadChar() != 0)
            {
                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    return false;
            }

            int stringCount = 0;
            if (ReadStringTable)
                stringCount = IsGlobalStringTable ? reader.ReadInt32() : reader.ReadInt16();
 
            StringTable = stringCount == 0 ? null : GetStringTable(reader, stringCount);

            int elementCount = reader.ReadInt32();
            if (elementCount == 0)
                return true;

            if (elementCount < 0 || (ReadStringTable && StringTable == null))
                return false;

            DXElement[] elements = new DXElement[elementCount];
            ReadElements(ref elements);
            ReadAttributes(ref elements);

            return true;
        }

        private void ReadElements(ref DXElement[] elements)
        {
            for (int i = 0; i < elements.Length; ++i)
            {
                string type = StringTable != null ? GetStringFromBuffer() : reader.ReadString(256);
                string name = IsGlobalStringTable ? GetStringFromBuffer() : reader.ReadTerminatedString();
                string id = BitConverter.ToString(reader.ReadBytes(16));

                DXElement element = new DXElement
                {
                    Id = id,
                    Type = type,
                    Name = name,
                };

                element.AddAttribute(new DXAttribute
                {
                    Type = DmAttributeType_t.AT_STRING,
                    Name = "name",
                    Value = name
                });

                elements[i] = element;
            }
        }

        private void ReadAttributes(ref DXElement[] elements)
        {
            for (int i = 0; i < elements.Length; ++i)
            {
                DXElement element = elements[i];
                int attributeCount = reader.ReadInt32();

                for (int j = 0; j < attributeCount; ++j)
                {
;                   if (ReadAttribute(out DXAttribute attribute, elements))
                    {
                        element.AddAttribute(attribute);
                    }
                }
            }
        }

        private bool ReadAttribute(out DXAttribute attribute, DXElement[] elements)
        {
            attribute = new DXAttribute
            {
                Type = DmAttributeType_t.AT_TYPE_INVALID
            };

            string name = (StringTable != null) ? GetStringFromBuffer() : reader.ReadString(1024);
            DmAttributeType_t type = (DmAttributeType_t)reader.ReadByte();

            if (type < DmAttributeType_t.AT_FIRST_VALUE_TYPE || type > DmAttributeType_t.AT_TYPE_COUNT)
            {
                Console.WriteLine($"Attribute {name} is outside of the type range");
                return false;
            }

            attribute.Type = type;
            attribute.Name = name;

            switch (attribute.Type)
            {
                case DmAttributeType_t.AT_ELEMENT:
                    AttributeUnserializeElement(attribute, elements);
                    break;
                case DmAttributeType_t.AT_ELEMENT_ARRAY:
                    AttributeUnserializeElementArray(attribute, elements);
                    break;
                case DmAttributeType_t.AT_STRING:
                    AttributeUnserializeElementString(attribute);
                    break;
                case DmAttributeType_t.AT_BOOL:
                    attribute.Value = reader.ReadBoolean();
                    break;
                case DmAttributeType_t.AT_INT:
                    attribute.Value = reader.ReadInt32();
                    break;
                case DmAttributeType_t.AT_VECTOR3:
                    attribute.Value = new float[3] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
                    break;
                case DmAttributeType_t.AT_FLOAT:
                    attribute.Value = reader.ReadSingle();
                    break;
                case DmAttributeType_t.AT_COLOR:
                    attribute.Value = reader.ReadSingle();
                    break;
                default:
                    AttributeUnseriailizeDefault(attribute);
                    break;
            }

            Console.WriteLine(attribute);
            return true;
        }

        public void AttributeUnseriailizeDefault(DXAttribute attribute)
        {
            throw new NotImplementedException();
        }

        public void AttributeUnserializeElement(DXAttribute attribute, DXElement[] elements)
        {
            attribute.Value = AttributeUnserializeElementIndex(elements);
        }

        public DXElement AttributeUnserializeElementIndex(DXElement[] elements)
        {
            int index = reader.ReadInt32();
            if (index == ELEMENT_INDEX_EXTERNAL)
            {
                Console.WriteLine("Reading externally referenced elements is not supported!");
                reader.ReadChars(40);
                return null;
            }

            if(index > elements.Length || (index < 0 && index != ELEMENT_INDEX_NULL))
                return null;

            return elements[index];
        }

        public void AttributeUnserializeElementArray(DXAttribute attribute, DXElement[] elements)
        {
            int elementCount = reader.ReadInt32();
            DXElement[] attributeElements = new DXElement[elementCount];

            for (int i = 0; i < elementCount; ++i)
            {
                DXElement element = AttributeUnserializeElementIndex(elements);
                attributeElements[i] = element;
            }

            attribute.Value = attributeElements;
        }

        public void AttributeUnserializeElementString(DXAttribute attribute)
        {
            if (IsGlobalStringTable)
            {
                attribute.Value = GetStringFromBuffer();
            }
            else
            {
                attribute.Value = reader.ReadTerminatedString();
            }
        }

        private string GetStringFromBuffer()
        {
            int i = UseLargeSymbols ? reader.ReadInt32() : reader.ReadInt16();
            return (i > StringTable.Length || i < 0) ? null : StringTable[i];
        }

        private string[] GetStringTable(BinaryReader reader, int stringCount)
        {
            string[] stringTable = new string[stringCount];

            for (int i = 0; i < stringCount; i++)
                stringTable[i] = reader.ReadTerminatedString();

            return stringTable;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HighFlyers.Protocol.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: " + Process.GetCurrentProcess().ProcessName + " <input hfproto file> <output cs file>");
                return;
            }

            try
            {
                var generator = new CodeGenerator(args[0], args[1]);
                generator.Generate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot generate cs file: " + ex.Message);
            }
        }
    }

    internal class CodeGenerator
    {
        enum CurrentType
        {
            None,
            Structure,
            Enumeration
        }

        abstract class ObjectType
        {
            protected readonly string[][] Input;
            protected readonly string name;

            protected ObjectType(string name, string[][] input)
            {
                Input = input;
            }

            protected abstract string GenerateHeader();
            protected abstract IEnumerable<string> GenerateBody();
            
            private string GenerateBottom()
            {
                return "}";
            }

            public IEnumerable<string> GenerateClass()
            {
                yield return GenerateHeader();

                foreach(var line in GenerateBody())
                    yield return line;

                yield return GenerateBottom();
            }
        }

        class Structure : ObjectType
        {
            public Structure(string name, string[][] input)
                : base(name, input)
            {
            }

            protected override string GenerateHeader()
            {
                return "struct " + name;
            }

            protected override IEnumerable<string> GenerateBody()
            {
                foreach (var words in Input)
                {
                    if (words.Length != 2)
                        throw new Exception("Expected two words in line!");

                    // todo check types (words[0])
                    yield return string.Join(" ", words) + ";";
                }
            }
        }

        class Enumeration : ObjectType
        {
            public Enumeration(string name, string[][] input)
                : base(name, input)
            {
            }

            protected override string GenerateHeader()
            {
                return "enum " + name;
            }

            protected override IEnumerable<string> GenerateBody()
            {
                foreach (var words in Input)
                {
                    if (words.Length != 1)
                        throw new Exception("Expected one word in line!");

                    yield return words[0];
                }
            }
        }

        readonly List<ObjectType> objectsTypes = new List<ObjectType>();
        private string[] data;
        private readonly List<string[]> currentCollector = new List<string[]>();
        private readonly string outputFileName;
        private readonly string inputFileName;
        private CurrentType currType = CurrentType.None;
        private bool wasStartBracket;
        readonly StringBuilder builder = new StringBuilder();
        private string currentName;
        
        public CodeGenerator(string inputFileName, string outputFileName)
        {
            this.inputFileName = inputFileName;
            this.outputFileName = outputFileName;
        }

        public void Generate()
        {
            builder.Clear();

            ReadFromFile();
            PrepareData();

            GenerateHeaders();
            GenerateObjectTypes();
            GenerateBottom();

            SaveToFile();
        }

        private void ReadFromFile()
        {
            data = System.IO.File.ReadAllLines(inputFileName);            
        }

        private void SaveToFile()
        {
            var file = new System.IO.StreamWriter(outputFileName);
            file.WriteLine(builder.ToString());
            file.Close();
        }

        private void GenerateHeaders()
        {
            builder.AppendLine("namespace HighFlyers.Protocol.Frames");
            builder.AppendLine("{");
        }

        private void GenerateBottom()
        {
            builder.AppendLine("}");
        }

        private void GenerateObjectTypes()
        {
            foreach (var line in objectsTypes.SelectMany(objType => objType.GenerateClass()))
            {
                builder.Append("\t");
                builder.AppendLine(line);
            }
        }

        private void PrepareData()
        {
            foreach (
                string[] words in
                    data.Select(
                        line => System.Text.RegularExpressions.Regex.Replace(line.Trim(), @"\s+", " ").Split(' '))
                        .Where(words => words.Length != 0))
            {
                if (words.Length == 2 && (words[0] == "struct" || words[0] == "enum"))
                {
                    if (currType != CurrentType.None)
                        throw new Exception("Unexpected keyword 'struct' usage!");

                    currentName = words[1];
                    currentCollector.Clear();
                    currType = (words[0] == "struct" ? CurrentType.Structure : CurrentType.Enumeration);
                }
                else if (words.Length == 1 && words[0] == "{")
                {
                    if (wasStartBracket)
                        throw new Exception("Unexpected '{' token");

                    wasStartBracket = true;
                }
                else if (words.Length == 1 && words[0] == "}")
                {
                    if (!wasStartBracket || currType == CurrentType.None)
                        throw new Exception("Unexpected '}' token");

                    switch (currType)
                    {
                        case CurrentType.Structure:
                            objectsTypes.Add(new Structure(currentName, currentCollector.ToArray()));
                            break;
                        case CurrentType.Enumeration:
                            objectsTypes.Add(new Enumeration(currentName, currentCollector.ToArray()));
                            break;
                    }

                    wasStartBracket = false;
                    currType = CurrentType.None;
                }
                else if (!wasStartBracket)
                    throw new Exception("Unexpected value " + words[0]);
                else
                    currentCollector.Add(words);
            }
        }
    }
}

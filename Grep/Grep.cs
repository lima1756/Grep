using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Grep
{

    class Grep
    {

        // flags
        public bool Color { get; set; }
        public bool LineNumber { get; set;  }
        public bool Recursive { get; set;  }
        public string regex;

        public string Regex
        {
            get => regex;
            set
            {
                regex = value;
                _nfa = new Regex(value).GetNFA();
            }
        }

        private string workingPath = Directory.GetCurrentDirectory();
        private NFA _nfa;

        public Grep(string regex, bool color=false, bool caseInsensitive=false, bool lineNumber=false, bool recursive=false)
        {
            this.Regex = regex;
            this.Color = color;
            this.LineNumber = lineNumber;
            this.Recursive = recursive;
        }

        public Grep()
        {
            this.regex = null;
            this.Color = false;
            this.LineNumber = false;
            this.Recursive = false;
        }

        public void Execute()
        {
            if (Regex == null)
            {
                throw new Exception("expresion regular no introducida");
            }

            if (Recursive)
            {
                RecursiveSearch(workingPath);
            }
            else
            {
                Execute(workingPath);
            }
        }

        private void Execute(string path)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = _nfa.Execute(Path.GetFileName(file));
                var content = _nfa.Execute(File.ReadAllText(file));
                if (fileName && content)
                {
                    WriteColor(Path.GetFileName(file), ConsoleColor.DarkGreen);
                    Console.Write(" - ");
                    WriteColor("Content has a match of the regex pattern", ConsoleColor.Green);
                    Console.WriteLine();
                }
                else if (content)
                {
                    WriteColor(Path.GetFileName(file), ConsoleColor.Red);
                    Console.Write(" - ");
                    WriteColor("Content has a match of the regex pattern", ConsoleColor.Green);
                    Console.WriteLine();
                }
                else if (fileName)
                {
                    WriteColor(Path.GetFileName(file), ConsoleColor.DarkGreen);
                    Console.Write(" - ");
                    WriteColor("Content doesn't have a match of the regex pattern", ConsoleColor.Red);
                    Console.WriteLine();
                }
            }
        }

        public void RecursiveSearch(string currentPath)
        {
            Execute(currentPath);
            var directories = Directory.EnumerateDirectories(currentPath);
            foreach (string path in directories)
            {
                Console.Write("Searching in: ");
                WriteColor(path+"\n", ConsoleColor.Blue);
                RecursiveSearch(path);
            }
        }


        private void WriteLineColor(string word)
        {

            WriteColor(word + "\n", ConsoleColor.Red);
        }

        private static void WriteColor(string word, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(word);
            Console.ResetColor();
        }
        
    }
}

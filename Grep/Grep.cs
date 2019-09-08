using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Grep
{

    class Grep
    {

        // flags
        private bool color;
        private bool lineNumber;
        private bool recursive;
        private string regex;
        private string workingPath = Directory.GetCurrentDirectory();

        public Grep(string regex, bool color=false, bool caseInsensitive=false, bool lineNumber=false, bool recursive=false)
        {
            this.regex = regex;
            this.color = color;
            this.lineNumber = lineNumber;
            this.recursive = recursive;
        }

        public Grep()
        {
            this.regex = null;
            this.color = false;
            this.lineNumber = false;
            this.recursive = false;
            if (recursive)
            {
                recursiveSearch(workingPath);
            }
            else
            {
                execute(workingPath);
            }
        }

        public void execute(string currentPath)
        {
            if (regex == null)
            {
                throw new Exception("expresion regular no introducida");
            }
        }

        public void recursiveSearch(string currentPath)
        {
            // TODO: check the path name
            execute(currentPath);
            var directories = Directory.EnumerateDirectories(currentPath);
            foreach (string path in directories)
            {
                Console.WriteLine(path);
                recursiveSearch(path);
            }
        }

        public void SetColor(bool color)
        {
            this.color = color;
        }

        public void SetLineNumber(bool lineNumber)
        {
            this.lineNumber = lineNumber;
        }

        public void SetRecursive(bool recursive)
        {
            this.recursive = recursive;
        }

        private void WriteLineColor(string word)
        {

            WriteColor(word + "\n");
        }

        private void WriteColor(string word)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(word);
            Console.ResetColor();
        }
        
    }
}

using System;
using System.Collections.Generic;

namespace Grep
{
    class Program
    {
        private static Grep grep;

        static void Main(string[] args)
        {
            //grep = new Grep();
            //string regex = "";
            //foreach(string arg in args)
            //{
            //    if (arg[0] == '-')
            //    {
            //        SetFlag(arg);
            //    }
            //    else if (regex == "")
            //    {
            //        regex = arg;
            //    }
            //    else
            //    {
            //        throw new Exception("Argumento \""+arg+"\" introducido no definido");
            //    }
            //}
            //Console.WriteLine(regex);
            Console.WriteLine("(ab(a|b)*a|b+)");
            var regex = new Regex("(ab(a|b)*a|b+)");
            Console.WriteLine(regex.Evaluate("aaa"));
            // TODO: call Grep class with the data
            Console.ReadKey();
        }

        public static void SetFlag(string arg)
        {
            List<string> flags = new List<string>();
            if (arg[1] == '-')
            {
                flags.Add(arg.Substring(2));
            }
            else if (arg.Length > 2)
            {
                foreach(char c in arg.Substring(1))
                {
                    flags.Add(c.ToString());
                }
            }
            else
            {
                flags.Add(arg[1].ToString());
            }
            foreach(string flag in flags)
            {
                switch (flag)
                {
                    case "color":
                        grep.SetColor(true);
                        break;
                    case "r":
                        grep.SetRecursive(true);
                        break;
                    case "n":
                        grep.SetLineNumber(true);
                        break;
                    default:
                        throw new Exception("The provided parameter \""+flag+"\"doesn't exist");

                }
            }
        }
    }
}

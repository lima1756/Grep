using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grep
{
    class NFA
    {
        public static char Null { get; } = '\0';
        private volatile bool _finished = false;
        public Node Start { get; set; }
        public Node End { get; set; }
        public bool Finished { get => _finished; set => _finished = value; }

        public NFA()
        {
            Start = new Node();
            End = new Node();
            Start.AddTransition(null, End);
        }

        public NFA(Node start, Node end)
        {
            this.Start = start;
            this.End = end;
        }

        public bool Execute(string str)
        {
            if (Start == End)
            {
                throw new Exception("The expression will return empty matches. It will match infinite times.");
            }
            return Execute(this.Start, str);
        }

        private bool Execute(Node current, string str)
        {
            if (_finished)
            {
                return _finished;
            }
            if (str.Length == 0 && current == End)
            {
                return _finished = true;
            }
            else if (str.Length == 0 && !current.Transitions.ContainsKey(Null))
            {
                return false;
            }
            if (current.Transitions.ContainsKey(Null))
            {
                var tasks = new List<Task>();
                foreach (var node in current.Transitions[Null])
                {
                    tasks.Add(Task.Factory.StartNew(() => Execute(node, str)));
                }
                Task.WaitAll(tasks.ToArray());
            }
            if (str.Length > 0 && current.Transitions.ContainsKey(str[0]))
            {
                var tasks = new List<Task>();
                foreach (var node in current.Transitions[str[0]])
                {
                    tasks.Add(Task.Factory.StartNew(() => Execute(node, str.Substring(1))));
                }
                Task.WaitAll(tasks.ToArray());
            }
            if (str.Length > 0)
            {
                return Execute(current, str.Substring(1));
            }
            return _finished;
        }

    }
}

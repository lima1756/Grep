using System;
using System.Collections.Generic;

namespace Grep
{
    class Node
    {
        public Dictionary<char, List<Node>> Transitions { get; }

        public Node()
        {
            Transitions = new Dictionary<char, List<Node>>();
        }

        public override string ToString()
        {
            var s = "";
            foreach (var c in Transitions.Keys)
            {
                s += (c.ToString() + "=>");
            }
            return s;
        }

        public void AddTransition(RegexElement re, Node node)
        {
            if (re == null && Transitions.ContainsKey(NFA.Null))
            {
                Transitions[NFA.Null].Add(node);
            }
            else if (re == null)
            {
                Transitions.Add(NFA.Null, new List<Node>(new Node[] { node }));
            }
            else if (Transitions.ContainsKey(re.Value))
            {
                Transitions[re.Value].Add(node);
            }
            else
            {
                Transitions.Add(re.Value, new List<Node>(new Node[] { node }));
            }
        }

        public void RemoveTransition(RegexElement re, Node node)
        {
            if (Transitions.ContainsKey(re.Value) && Transitions[re.Value].Contains(node))
            {
                if (Transitions[re.Value].Count > 1)
                {
                    Transitions[re.Value].Remove(node);
                }
                else
                {
                    Transitions.Remove(re.Value);
                }
            }
            else
            {
                throw new Exception("Transition doesn't exist");
            }
        }
    }
}
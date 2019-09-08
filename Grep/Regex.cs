using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Grep
{
    class Regex
    {
        private static readonly List<char> Operators = new List<char>(new char[] { '*', '|', '+', '(', ')' });

        private readonly List<RegexElement> _postFix = new List<RegexElement>();
        private readonly string _regex;
        

        public Regex(string regex)
        {
            this._regex = regex;
            this.CreatePostFix();
        }

        private void CreatePostFix()
        {
            var stack = new Stack<RegexElement>();
            var lastChar = false;

            foreach (var c in this._regex)
            {
                if (!Operators.Contains(c) && !lastChar)
                {
                    _postFix.Add(new RegexElement(c));
                    lastChar = true;
                }
                else if (c == '(')
                {
                    if (lastChar)
                    {
                        AddOperatorToPostFix(stack, new RegexElement(Operations.Concat));
                    }
                    stack.Push(new RegexElement(Operations.LeftParenthesis));
                    lastChar = false;
                }
                else if (c == ')')
                {
                    while (stack.Count > 0 && stack.Peek().CompareTo(Operations.LeftParenthesis) != 0)
                    {
                        _postFix.Add(stack.Pop());
                    }

                    if (stack.Count > 0 && stack.Peek().CompareTo(Operations.LeftParenthesis) != 0)
                    {
                        throw new InvalidExpressionException();
                    }
                    else
                    {
                        stack.Pop();
                    }
                    lastChar = true;
                }
                else if (!Operators.Contains(c) && lastChar)
                {
                    AddOperatorToPostFix(stack, new RegexElement(Operations.Concat));
                    _postFix.Add(new RegexElement(c));
                    lastChar = true;
                }
                else
                {
                    if (c == '*')
                    {
                        AddOperatorToPostFix(stack, new RegexElement(Operations.Kleene));
                        lastChar = true;
                    }
                    else if (c == '+')
                    {
                        AddOperatorToPostFix(stack, new RegexElement(Operations.Positive));
                        lastChar = true;
                    }
                    else if (c == '|')
                    {
                        AddOperatorToPostFix(stack, new RegexElement(Operations.Union));
                        lastChar = false;
                    }
                }
            }
            while (stack.Count > 0)
            {
                _postFix.Add(stack.Pop());
            }
        }

        private void AddOperatorToPostFix(Stack<RegexElement> stack, RegexElement op)
        {
            while (stack.Count > 0 && op.CompareTo(stack.Peek()) <= 0)
            {
                _postFix.Add(stack.Pop());
            }
            stack.Push(op);
        }

        public bool Evaluate(string str)
        {
            var stack = new Stack<object>();
            foreach(var re in _postFix)
            {
                if (re.IsOperation)
                {
                    object firstElement, secondElement;
                    switch (re.Operation)
                    {
                        case Operations.Concat:
                            secondElement = stack.Pop();
                            firstElement = stack.Pop();
                            stack.Push(Concat(firstElement, secondElement));
                            break;
                        case Operations.Union:
                            secondElement = stack.Pop();
                            firstElement = stack.Pop();
                            stack.Push(Union(firstElement, secondElement));
                            break;
                        case Operations.Kleene:
                            stack.Push(Kleene(stack.Pop()));
                            break;
                        case Operations.Positive:
                            stack.Push(Positive(stack.Pop()));
                            break;
                    }
                }
                else
                {
                    stack.Push(re);
                }
            }
            var nfa = (NFA)stack.Pop();
            return nfa.Execute(str);
        }

        private static NFA Concat(object element1, object element2)
        {
            var start = new Node();
            var end = new Node();
            var nodes = new List<Node>();
            if (element1 is RegexElement && element2 is RegexElement)
            {
                Node middle = new Node();
                start.AddTransition((RegexElement)element1, middle);
                middle.AddTransition((RegexElement)element2, end);
            }
            else if (element1 is NFA && element2 is NFA)
            {
                start = ((NFA)element1).Start;
                end = ((NFA)element2).End;
                ((NFA)element1).End.AddTransition(null, ((NFA)element2).Start);
            }
            else if (element1 is NFA)
            {
                start = ((NFA)element1).Start;
                ((NFA)element1).End.AddTransition((RegexElement)element2, end);
            }
            else if (element2 is NFA)
            {
                end = ((NFA)element2).End;
                start.AddTransition((RegexElement)element1, ((NFA)element2).Start);
            }
            
            return new NFA(start, end);
        }

        private NFA Union(object element1, object element2)
        {
            Node start = new Node();
            Node end = new Node();
            if (element1 is RegexElement && element2 is RegexElement)
            {
                Node middle1 = new Node();
                Node middle2 = new Node();
                start.AddTransition((RegexElement)element1, middle1);
                start.AddTransition((RegexElement)element2, middle2);
                middle1.AddTransition(null, end);
                middle2.AddTransition(null, end);
            }
            else if(element2 is NFA && element1 is NFA)
            {
                start.AddTransition(null, ((NFA)element1).Start);
                start.AddTransition(null, ((NFA)element2).Start);
                ((NFA)element1).End.AddTransition(null, end);
                ((NFA)element2).End.AddTransition(null, end);
            }
            else if (element1 is NFA)
            {
                Node middle1 = new Node();
                start.AddTransition(null, ((NFA)element1).Start);
                start.AddTransition((RegexElement)element2, middle1);
                ((NFA)element1).End.AddTransition(null,end);
                middle1.AddTransition(null, end);
            }
            else if (element2 is NFA)
            {
                Node middle1 = new Node();
                start.AddTransition(null, ((NFA)element2).Start);
                start.AddTransition((RegexElement)element1, middle1);
                ((NFA)element2).End.AddTransition(null, end);
                middle1.AddTransition(null, end);
            }
            
            return new NFA(start, end);
        }

        private NFA Kleene(object element1)
        {
            if(element1 is NFA)
            {
                ((NFA)element1).End.AddTransition(null, ((NFA)element1).Start);
                ((NFA)element1).End = ((NFA)element1).Start;
                return ((NFA)element1);
            }
            var start = new Node();
            start.AddTransition((RegexElement)element1, start);
            return new NFA(start, start);
        }

        private NFA Positive(object element1)
        {
            if (element1 is NFA)
            {
                ((NFA)element1).End.AddTransition(null, ((NFA)element1).Start);
                return ((NFA)element1);
            }
            var start = new Node();
            var end = new Node();
            start.AddTransition((RegexElement)element1, end);
            end.AddTransition(null, start);
            return new NFA(start, end);
        }

        private class InvalidExpressionException : Exception
        {
            public InvalidExpressionException() :
                base("Invalid expression")
            { }
        }


    }

    internal class RegexElement: IComparable
    {
        private readonly char _value = NFA.Null;
        private readonly Operations _operation = Operations.Nill;

        public RegexElement(char value)
        {
            if (_operation == Operations.Nill)
            {
                this._value = value;
            }
            else
            {
                throw new MultipleValueException();
            }
        }

        public RegexElement(Operations operation)
        {
            if (_value == NFA.Null)
            {
                this._operation = operation;
            }
            else
            {
                throw new MultipleValueException();
            }
        }

        public int CompareTo(object obj)
        {
            Operations op = Operations.Nill;
            if(obj is Operations)
            {
                op =(Operations)obj;
            }
            else if(obj is RegexElement)
            {
                op = ((RegexElement)obj)._operation;
            }
            if (this._operation == Operations.Nill || op == Operations.Nill)
                throw new CompareException();
            return ((int)_operation).CompareTo((int)(op));
        }

        public bool IsOperation => _operation != Operations.Nill;

        public Operations Operation => _operation;
        public char Value => _value;

        public override string ToString()
        {
            if (_value == NFA.Null)
            {
                return OperationToString();
            }
            else
            {
                return _value.ToString();
            }
        }

        private string OperationToString()
        {
            switch (_operation)
            {
                case Operations.Kleene:
                    return "*";
                case Operations.Positive:
                    return "+";
                case Operations.Union:
                    return "|";
                case Operations.Concat:
                    return "^";
            }
            return null;
        }

        private class MultipleValueException:Exception
        {
            public MultipleValueException(): 
                base("An element can't be an operation and an character at the same time")
            {}
        }

        private class CompareException : Exception
        {
            public CompareException() :
                base("You can only compare two not nill Operations")
            { }
        }

    }

    internal enum Operations
    {
        Nill = -1,
        LeftParenthesis = 0,
        RightParenthesis = 0,
        Union = 1,
        Concat = 2,
        Positive = 3,
        Kleene = 4
    }
}

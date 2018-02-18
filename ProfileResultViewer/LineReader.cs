using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ProfileResultViewer
{
    public class LineReader
    {
        private List<Line> callees { get; set; }
        private List<Line> callers { get; set; }
        private List<Line> roots { get; set; }

        internal IEnumerable<Line> Callees()
        {
            return callees.SortedAccess();
        }
        internal IEnumerable<Line> Callers()
        {
            return callers.SortedAccess();
        }
        internal IEnumerable<Line> Roots()
        {
            return roots.SortedAccess();
        }

        public Line GetRootElement()
        {
            Debug.Assert(roots.Count != 0);
            if (roots.Count == 0)
                return null;

            var element = roots.First();
            while (element.Caller != null)
            {
                element = element.Caller;
            }
            return element;
        }

        public LineReader()
        {
            callees = new List<Line>();
            callers = new List<Line>();
            roots = new List<Line>();
        }

        public void ReadLines(string filepath)
        {
            ParseFile(filepath);

            BuildRootTree();
            BuildCalleeTree();
            BuildCallerTree();

            MergeCallers();
        }

        /// <summary>
        /// Read the file and save it in a list. Nothing clever here.
        /// </summary>
        /// <param name="filepath"></param>
        private void ParseFile(string filepath)
        {
            using (StreamReader reader = new StreamReader(filepath))
            {
                using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(reader))
                {
                    parser.SetDelimiters(",");
                    parser.ReadLine();
                    while (!parser.EndOfData)
                    {
                        var line = parser.ReadFields();
                        LineType type = (LineType)Enum.Parse(typeof(LineType), line[0], true);
                        string name = line[1];
                        string rootname = line[11];
                        double incl = Convert.ToDouble(line[4]);
                        double excl = Convert.ToDouble(line[5]);
                        Line l = new Line(name, incl, excl, type, rootname);
                        if (l.Type == LineType.Callee)
                            callees.Add(l);
                        else if (l.Type == LineType.Caller)
                            callers.Add(l);
                        else if (l.Type == LineType.Root)
                            roots.Add(l);
                        else
                            Debug.Assert(false, "Invalid Line Type");
                    }
                }
            }
        }

        /// <summary>
        /// Link the parsed lines to form the "Root"-Tree
        /// </summary>
        private void BuildRootTree()
        {
            foreach (var root in roots)
            {
                var children = callees.FindAll(c => c.RootName == root.Name);
                foreach (var child in children)
                {
                    var childelem = roots.Find(r => r.Name == child.Name);
                    root.AddChild(childelem);
                }
            }
        }

        /// <summary>
        /// Link the parsed lines to form the "Callee"-Tree
        /// </summary>
        private void BuildCalleeTree()
        {
            foreach (var callee in callees)
            {
                var parents = callees.FindAll(c => c.Name == callee.RootName);
                foreach (var parent in parents)
                {
                    callee.AddChild(parent);
                }
            }
        }

        /// <summary>
        /// Link the parsed lines to form the "Caller"-Tree
        /// </summary>
        private void BuildCallerTree()
        {
            foreach (var caller in callers)
            {
                var children = callers.FindAll(c => c.Name == caller.RootName);
                foreach (var child in children)
                {
                    caller.AddChild(child);
                }
            }
        }

        private void MergeCallers()
        {
            List<Line> tmp = new List<Line>();
            callers.Sort((l1, l2) =>
            {
                if (l1.Name == l2.Name)
                    return l2.Inclusive.CompareTo(l1.Inclusive);
                else
                    return l1.Name.CompareTo(l2.Name);
            });

            var current = callers.First();
            foreach (var caller in callers)
            {
                if (caller.Name == current.Name && caller != current)
                {
                    foreach (var child in caller.Callees)
                    {
                        current.AddChild(child);
                    }
                }
                else
                {
                    tmp.Add(current);
                    current = caller;
                }
            }
            callers = tmp;
        }
    }
}

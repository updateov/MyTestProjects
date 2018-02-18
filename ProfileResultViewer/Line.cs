using System;
using System.Collections.Generic;

namespace ProfileResultViewer
{
    public enum LineType
    {
        Caller,
        Callee,
        Root
    }

    //[DebuggerDisplay("{Name}, {Callees.Count} Children")]
    /// <summary>
    /// The processed data of one Line in the csv-File
    /// </summary>
    public class Line
    {
        public string Name { get; private set; }
        public string RootName { get; private set; }
        public double Inclusive { get; private set; }
        public double Exclusive { get; private set; }
        public LineType Type { get; private set; }

        private List<Line> callees;
        /// <summary>
        /// TODO: Use the fact, that there can be several callers
        /// (Currently, Caller is effectively ignored)
        /// </summary>
         public Line Caller { get; private set; }

        public IEnumerable<Line> Callees
        {
            get
            {
                return callees.SortedAccess();
            }
        }

        public Line(string Name, double InclTime, double ExclTime, LineType Type, string RootFunctionName)
        {
            this.Name = Name;
            this.Inclusive = InclTime;
            this.Exclusive = ExclTime;
            this.Type = Type;
            this.RootName = RootFunctionName;
            callees = new List<Line>();
        }

        public override string ToString()
        {
            return String.Format("{0,5:0.00}", Inclusive)
                + String.Format(" ({0,5:0.00})", Exclusive)
                + Name;
        }

        internal void AddChild(Line child)
        {
            callees.Add(child);
            child.Caller = this;
        }
    }
}

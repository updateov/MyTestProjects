using CRIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRIPlugin
{
    public class CRIStateChangedEventArgs : EventArgs
    {
        public String VisitKey { get; set; }
        public ExposedCRIState State { get; set; }

        public CRIStateChangedEventArgs(String visitKey, ExposedCRIState state)
        {
            VisitKey = visitKey;
            State = state;
        }
    }
}

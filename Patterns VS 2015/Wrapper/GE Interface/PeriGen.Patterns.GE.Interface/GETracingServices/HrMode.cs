using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
    internal class HrMode
    {
        public static readonly HrMode OFF = new HrMode(0, "OFF", 7);

        public static readonly HrMode INOP = new HrMode(1, "INOP", 3);

        public static readonly HrMode US = new HrMode(2, "US", 14);

        public static readonly HrMode US2 = new HrMode(3, "US2", 15);

        public static readonly HrMode FECG = new HrMode(4, "FECG", 2);

        public static readonly HrMode MECG = new HrMode(5, "MECG", 5);

        public static readonly HrMode AECG = new HrMode(6, "AECG", 11);

        public static readonly HrMode MAECG = new HrMode(6, "M-AECG", 12);

        public static readonly HrMode EXT_MHR = new HrMode(7, "EXT-MHR", 9);

        public static readonly HrMode PHONO = new HrMode(7, "PHONO", 16);

        public static readonly HrMode UNKNOWN = new HrMode(8, "", 17);

        internal int mode;

        internal string englishName;

        internal int messageID;

        internal HrMode(int paramInt1, string paramString, int paramInt2)
        {
            this.mode = paramInt1;
            this.englishName = paramString;
            this.messageID = paramInt2;
        }

        internal virtual int Mode
        {
            get
            {
                return this.mode;
            }
        }

        public override string ToString()
        {
            return this.englishName;
        }

        public virtual bool isValid
        {
            get
            {
                return (this != OFF) && (this != INOP) && (this != UNKNOWN);
            }
        }

        public virtual bool isMaternal
        {
            get
            {
                return (this == MECG) || (this == MAECG) || (this == EXT_MHR);
            }
        }
    }
}

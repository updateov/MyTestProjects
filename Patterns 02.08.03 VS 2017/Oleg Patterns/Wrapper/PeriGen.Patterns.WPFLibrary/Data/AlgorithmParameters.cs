using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriGen.Patterns.WPFLibrary.Data
{
    public class AlgorithmParameters
    {
        public int MinimalBaselineVariability { get; set; }
        public double MinimalLateDecelConfidence { get; set; }
        public int MinimalAccelerationsAmount { get; set; }
        public int MinimalLateDecelAmount { get; set; }
        public int MinimalLargeAndLongDecelAmount { get; set; }
        public int MinimalLateAndLargeAndLongDecelAmount { get; set; }
        public int MinimalLateAndProlongedDecelAmount { get; set; }
        public int MinimalProlongedDecelHeight { get; set; }
        public int MinimalContractionsAmount { get; set; }
        public int MinimalLongContractionsAmount { get; set; }
        public int CRIStateQualificationWindowSize { get; set; }
        public int MinimalAmountOfDataInQualificationWindow { get; set; }

        public AlgorithmParameters()
        {
        }
    }
}

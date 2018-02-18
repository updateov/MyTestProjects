using CRIAlgorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIAlgorithmUnitTest
{
    public class TestCRICalculator : CRICalculator
    {
        public double LastMeanBaselineVariability { set { m_lastMeanBaselineVariability = value; } }

        public int ContractilityQualificationWindow { get { return CONTRACTILITY_QUALIFY_WINDOW_SIZE; } }
    }
}

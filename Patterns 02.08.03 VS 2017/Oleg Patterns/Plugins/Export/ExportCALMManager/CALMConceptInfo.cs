using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Export.CALMManager
{
    public class CALMConceptInfo
    {
        public string AbreviatedCaption { get; set; }
        public bool CanHaveMultipleValues { get; set; }
        public string CapitalizedCaption { get; set; }
        public string Caption { get; set; }
        public int ConceptNo { get; set; }
        public string DateTimeFormat { get; set; }
        public int DecimalPlaces { get; set; }
        public bool HasIdList { get; set; }
        public bool HasList { get; set; }
        public bool HasRange { get; set; }
        public double IncValue { get; set; }
        public bool IsCalculated { get; set; }
        public bool IsCalculationSource { get; set; }
        public bool IsCheckBox { get; set; }
        public bool IsDateTime { get; set; }
        public bool IsFreeText { get; set; }
        public bool IsPhoneNumber { get; set; }
        public bool IsTextFormatNumerical { get; set; }
        public bool IsTimeSeriesConcept { get; set; }
        public int ListId { get; set; }
        public int MaxChars { get; set; }
        public int MaxCharsOverride { get; set; }
        public double? MaxValue { get; set; }
        public double? MinValue { get; set; }
        public StringCollection RangeList { get; set; }
        public List<int> ResponseIdList { get; set; }
        public List<string> ResponseList { get; set; }
        public TextFormat TextFormatOverride { get; set; }

        public CALMValueType CALMValueType { get; set; }
        public int ObjectOFCare { get; set; }
        public int OrderNumberInGroup { get; set; }

        public List<CALMConceptInfo> SubConcepts { get; set; }
        public CALMConceptType ConceptType { get; set; }

        public CALMConceptInfo()
        {
            SubConcepts = new List<CALMConceptInfo>();
            ConceptType = CALMConceptType.Unknown;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.CALMManager
{
    public enum OOCType
    {
        eMother = 0,
        eFetus = 1,
    }

    public enum TextFormat
    {
        eInvalidTextFormat = -2,
        eAnyChar = -1,
        eAlpha = 0,
        eNumeric = 1,
        eAlphaNumeric = 2,
        eAlphaNumericNoSearchCharacters = 3,
        eAlphaNumericWithSearchCharacters = 4,
        eAlphaWithSearchCharacters = 5,
    }

    public enum CALMValueType
    {
        Abstract = 0,
        Integer = -1,
        Double = -2,
        ShortInteger = -3,
        String = -4,
        Reading = -5,
        Timespan = -7,
        ReadingSet = -8,
        Time = -11,
        Twotimes = -12,
        Fuzzy = -13,
        LpmunitSet = -14,
        ContractionSet = -15,
        FlagString = -16,
        FuzzyString = -17,
        BinaryString = -18,
        EocStatus = -19,
        DischargeReasons = -20,
        MaskString = -21,
        StringAndInteger = -22,
        PatternEvent = -23,
        PatternContraction = -24
    }
    
    public enum CALMResults
    {
        UnKnown,
        IsAuthorised,
        IsNotAuthorised,
        WrongLoginOrPassword,
    }

    public enum CALMConceptType
    {
        Unknown,
        Concept,
        SubConcept,
        String
    }
}

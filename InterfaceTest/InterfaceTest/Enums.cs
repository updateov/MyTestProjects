namespace InterfaceTest
{
    public enum DecelerationCategory
    {
        None = 0,
        Early = 3,
        Variable = 4,
        AtypicalVariable = 5,
        Late = 6,
        NonAccociated = 7,
        Prolonged = 14
    };

    public enum CRIClassification
    {
        Unknown = -1,
        Negative = 0,
        Positive = 1
    };

    public enum EventClassification
    {
        Baseline,
        Acceleration,
        Deceleration
    };

}

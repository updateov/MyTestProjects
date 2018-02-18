using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public enum ContractilityState
    {
        Unknown = -1,
        Normal = 0,
        Alert = 1,
        Danger = 2
    };

    public class ContractilityDisplay : CalculatedObject
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ContractilityState State { get; set; }

        public ContractilityDisplay()
        {
            State = ContractilityState.Unknown;
        }

        public ContractilityDisplay(DateTime start, DateTime end, ContractilityState state)
        {
            StartTime = start;
            EndTime = end;
            State = state;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIPluginDataModel
{
    public class ContractilityModel
    {
        public int ContractilityId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Classification { get; set; }

        public double Variability { get; set; }
        public bool IsVariabilityForStatus { get; set; }
        public long Accels { get; set; }
        public bool IsAccelsForStatus { get; set; }
        public long Contractions { get; set; }
        public bool IsContractionsForStatus { get; set; }
        public long LongContractions { get; set; }
        public bool IsLongContractionsForStatus { get; set; }
        public long LargeDeceles { get; set; }
        public bool IsLargeDecelesForStatus { get; set; }
        public long LateDecels { get; set; }
        public bool IsLateDecelsForStatus { get; set; }
        public long ProlongedDecels { get; set; }
        public bool IsProlongedDecelsForStatus { get; set; }
        public bool IsDirty { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateInserted { get; set; }

        public ContractilityModel()
        {

        }

        public ContractilityModel(ContractilityModel model)
        {
            ContractilityId = model.ContractilityId;
            StartTime = model.StartTime;
            EndTime = model.EndTime;
            Classification = model.Classification;

            Variability = model.Variability;
            IsVariabilityForStatus = model.IsVariabilityForStatus;
            Accels = model.Accels;
            IsAccelsForStatus = model.IsAccelsForStatus;
            Contractions = model.Contractions;
            IsContractionsForStatus = model.IsContractionsForStatus;
            LongContractions = model.LongContractions;
            IsLongContractionsForStatus = model.IsLongContractionsForStatus;
            LargeDeceles = model.LargeDeceles;
            IsLargeDecelesForStatus = model.IsLargeDecelesForStatus;
            LateDecels = model.LateDecels;
            IsLateDecelsForStatus = model.IsLateDecelsForStatus;
            ProlongedDecels = model.ProlongedDecels;
            IsProlongedDecelsForStatus = model.IsProlongedDecelsForStatus;
            IsDirty = model.IsDirty;
            DateUpdated = model.DateUpdated;
            DateInserted = model.DateInserted;
        }
    }
}

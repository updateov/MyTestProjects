using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public class CRIEpisode
    {
        public string VisitKey { get; set; }

        public CRIObject CurrentDisplayCRI { get; set; }
        public List<CRIObject> PositivePastNotYetReviewedCRIs { get; set; }
        public List<ContractilityDisplay> LastHourContractilities { get; set; }

        public CRIEpisode()
        {            
            VisitKey = String.Empty;

            CurrentDisplayCRI = new CRIObject();
            PositivePastNotYetReviewedCRIs = new List<CRIObject>();
            LastHourContractilities = new List<ContractilityDisplay>();
        }
    }
}

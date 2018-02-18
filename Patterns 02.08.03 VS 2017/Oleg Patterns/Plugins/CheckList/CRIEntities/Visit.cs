using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRIEntities
{   
    public class Visit
    {
        public string VisitKey { get; set; }

        public Bed Bed { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Fetuses { get; set; }
        public string GA { get; set; }
        public DateTime? EDD { get; set; }
        public bool IsADT { get; set; }

        public CRIObject CurrentDisplayCRI { get; set; }
        public List<CRIObject> PositivePastNotYetReviewedCRIs { get; set; }
        public List<ContractilityDisplay> LastHourContractilities { get; set; }

        public bool IsCurrentPositive
        {
            get
            {
                return CurrentDisplayCRI.CRIStatus == CRIState.PositiveCurrent || CurrentDisplayCRI.CRIStatus == CRIState.PositiveReviewed;
            }
        }

        public Visit()
        {
            VisitKey = String.Empty;
            Bed = new Bed();
            FirstName = String.Empty;
            LastName = String.Empty;
            Fetuses = 0;
            GA = String.Empty;
            EDD = null;
            IsADT = false;

            CurrentDisplayCRI = new CRIObject();
            PositivePastNotYetReviewedCRIs = new List<CRIObject>();
            LastHourContractilities = new List<ContractilityDisplay>();
        }        
    }
}

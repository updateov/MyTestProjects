using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PeriGen.Patterns.Service
{
    public class EpisodeIdentifier
    {
        public string VisitKey { get; set; }
        public int PatientUniqueID { get; set; }
        public EpisodeIdentifier()
        {
            PatientUniqueID = 0;
        }
    }

    public class MothersMap : Dictionary<string, List<EpisodeIdentifier>>
    {
  
        public MothersMap()
        {
        }

        public void AddVsitKey(string motherID, string visitKey, int patientUniqueID)
        {
            if (!this.ContainsKey(motherID))
            {
                var visits = new List<EpisodeIdentifier>();
                EpisodeIdentifier visit = new EpisodeIdentifier() { VisitKey = visitKey, PatientUniqueID = patientUniqueID };
                visits.Add(visit);
                this.Add(motherID, visits);
            }
            else
            {
                var visits = this[motherID];
                if (null == visits.FirstOrDefault(k => k.VisitKey.Equals(visitKey)))
                {
                    EpisodeIdentifier visit = new EpisodeIdentifier() { VisitKey = visitKey, PatientUniqueID = patientUniqueID };
                    visits.Add(visit);
                }
            }
        }
        public List<EpisodeIdentifier> GetMultiples(string motherID)
        {
            List<EpisodeIdentifier> visits = new List<EpisodeIdentifier>();
            if (this.ContainsKey(motherID))
            {
                visits = this[motherID];
            }
            return visits;
        }

        public XElement GetMultiplesXElement(string motherID)
        {
            XElement multiples = new XElement("multiples");
            multiples.Add(new XAttribute("motherId", motherID));
            var visits = GetMultiples(motherID);
            foreach (var sibling in visits)
            {
                XElement element = new XElement("sibling");
                element.Add(new XAttribute("PatientUniqueId", sibling.PatientUniqueID.ToString(CultureInfo.InvariantCulture)));
                element.Add(new XAttribute("key", sibling.VisitKey));
                multiples.Add(element);
            }
            return multiples;
        }

        public void RemoveVisit(string motherID, string visitKey)
        {
            if (this.ContainsKey(motherID))
            {
                this[motherID].RemoveAll(v => v.VisitKey.Equals(visitKey));
                if(this[motherID].Count == 0)
                    this.Remove(motherID);
            }
        }

        public void UpdatePatientUniqueID(string motherID, string visitKey, int uniqueID)
        {
            if (this.ContainsKey(motherID))
            {
                var visit = this[motherID].FirstOrDefault(v => v.VisitKey.Equals(visitKey));
                if(visit != null && visit.PatientUniqueID != uniqueID)
                {
                    visit.PatientUniqueID = uniqueID;
                }
            }
        }
    }
}


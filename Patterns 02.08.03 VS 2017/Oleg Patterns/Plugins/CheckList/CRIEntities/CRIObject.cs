using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIEntities
{
    public enum CRIState
    {
        Off,
        UnknownNotEnoughTime,
        UnknownGAOrSingletonNotMet,
        UnknownGAOrSingletonMissing,
        PositivePastNotYetReviewed,
        PositiveCurrent,
        PositiveReviewed,
        Negative
    }

    public class CRIObject : CalculatedObject
    {
        public string VisitKey { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }        
        
        public CRIState CRIStatus { get; set; }
        public CRIEvents CRIStatusEvents { get; set; }
        public DateTime CRIStatusTime { get; set; }

        public DateTime ReviewTime { get; set; }

        private String m_duration;
        private String m_displayStartTime;

        public String Duration
        {
            get 
            {
                if (StartTime == DateTime.MinValue || EndTime == DateTime.MinValue)
                    return "0 min";

                var duration = EndTime - StartTime;
                String toOut = String.Format("{0} min", (int)duration.TotalMinutes);
                return toOut;
            }
            set
            {
                m_duration = value;
            }
        }

        public String DisplayStartTime
        {
            get 
            {
                string strFormat = String.Empty;

                if (StartTime.Date < DateTime.Today)
                {
                    strFormat = "{0:MMM dd HH:mm}";
                }
                else
                {
                    strFormat = "{0:HH:mm}";
                }

                return String.Format(strFormat, StartTime); 
            }

            set { m_displayStartTime = value; }
        }

        public String DisplayEndTime
        {
            get { return String.Format("{0:HH:mm}", EndTime); }
        }

        public CRIObject()
        {
            VisitKey = String.Empty;

            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;        
    
            CRIStatus = CRIEntities.CRIState.Off;
            CRIStatusEvents = new CRIEvents();

            ReviewTime = DateTime.MinValue;
        }
    }
}

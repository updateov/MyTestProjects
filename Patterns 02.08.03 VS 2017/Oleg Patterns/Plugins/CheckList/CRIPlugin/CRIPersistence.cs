using CRIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIPlugin
{
    public enum PersistentContractilityState
    {
        Unknown,
        Negative,
        Positive
    }

    public class CRIPersistence : CalculatedObject
    {
        #region Properties & Members

        public static int NextId = -1;

        private DateTime m_startTime;
        public DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set 
            {
                m_startTime = value;
                IsDirty = true;
            }
        }

        private DateTime m_endTime;
        public DateTime EndTime
        {
            get
            {
                return m_endTime;
            }
            set
            {
                m_endTime = value;
                IsDirty = true;
            }
        }

        private PersistentContractilityState m_persistentState;
        public PersistentContractilityState PersistentState
        {
            get
            {
                return m_persistentState;
            }
            set
            {
                m_persistentState = value;
                IsDirty = true;
            }
        }

        private CRIEvents m_persistentStateEvents;
        public CRIEvents PersistentStateEvents
        {
            get
            {
                return m_persistentStateEvents;
            }
            set
            {
                m_persistentStateEvents = value;
                IsDirty = true;
            }
        }
        
        private DateTime m_acknowledgeTime;
        public DateTime AcknowledgeTime
        {
            get
            {
                return m_acknowledgeTime;
            }
            set
            {
                m_acknowledgeTime = value;
                IsDirty = true;
            }
        }

        private string m_acknowledgeUser;
        public string AcknowledgeUser
        {
            get
            {
                return m_acknowledgeUser;
            }
            set
            {
                m_acknowledgeUser = value;
                IsDirty = true;
            }
        }

        public bool IsDirty { get; set; }

        #endregion

        public CRIPersistence()
        {
            ID = ++NextId;

            StartTime = DateTime.MinValue;
            EndTime = DateTime.MinValue;

            PersistentState = PersistentContractilityState.Unknown;
            PersistentStateEvents = new CRIEvents();

            AcknowledgeTime = DateTime.MinValue;
            AcknowledgeUser = String.Empty;

            IsDirty = true;
        }

        public CRIPersistence(PersistentContractilityState state, DateTime start, DateTime end)
        {
            ID = ++NextId;

            StartTime = start;
            EndTime = end;

            PersistentState = state;
            PersistentStateEvents = new CRIEvents();

            AcknowledgeTime = DateTime.MinValue;
            AcknowledgeUser = String.Empty;

            IsDirty = true;
        }
    }
}

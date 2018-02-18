using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeriGen.Patterns.GE.Interface.TracingServices
{
    public class TracingArgs:ITracingArgs
    {
        #region ITracingArgs Members

        string bedId;
        public string BedId
        {
            get { return bedId; }
            set 
            {
                if (string.IsNullOrEmpty(value)) 
                {
                    throw new TracingArgsException("BedId cannot be null or empty");
                }
                bedId = value; 
            }
        }

        string patiendId;
        public string PatientId
        {
            get { return patiendId; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new TracingArgsException("PatientId cannot be null or empty");
                }
                patiendId = value; 
            }
        }

        string hostServiceAddress;
        public string HostServiceAddress
        {
            get { return hostServiceAddress; }
            set 
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new TracingArgsException("HostServiceAddress cannot be null or empty");
                }
                hostServiceAddress = value; 
            }
        }

        int timeAgo;
        /// <summary>
        /// How many seconds in the past are requested
        /// </summary>
        public int TimeAgo
        {
            get { return timeAgo; }
            set
            {
                if (value<0)
                {
                    throw new TracingArgsException("TimeAgo cannot < than 0");
                }
                timeAgo=value;
            }
        }

        private DateTime _StartTimeAgo=DateTime.MaxValue;
        /// <summary>
        /// DateTime to start retrieving historic tracing. By default is now (put DateTime.MaxValue to reset to default).
        /// </summary>
        public DateTime StartTimeAgo
        {
            get
            {
                return _StartTimeAgo;
            }
            set
            {
                _StartTimeAgo = value;
                if (_StartTimeAgo.Millisecond != 0)
                    _StartTimeAgo = _StartTimeAgo.AddMilliseconds(-_StartTimeAgo.Millisecond);
            }
        }

        bool isHistoric;
        public bool IsHistoric 
        {
            get { return isHistoric; }
            set { isHistoric = value; }
        }
        

        #endregion
    }
    [Serializable]
    public class TracingArgsException:Exception
    {
        public TracingArgsException() : base() {}
        public TracingArgsException(String Message):base(Message){}
    }
}

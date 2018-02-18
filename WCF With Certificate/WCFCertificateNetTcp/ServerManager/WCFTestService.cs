using Interfaces;
using System;

namespace ServerManager
{
    public class WCFTestService : ITestInterface
    {
        public StartTimeObject GetStartTime()
        {
            return new StartTimeObject() { StartTime = Manager.Instance.StartTime };
        }
    }
}

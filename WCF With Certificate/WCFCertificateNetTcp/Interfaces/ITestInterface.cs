using System;
using System.ServiceModel;

namespace Interfaces
{
    [ServiceContract]
    public interface ITestInterface
    {
        [OperationContract]
        StartTimeObject GetStartTime();
    }
}

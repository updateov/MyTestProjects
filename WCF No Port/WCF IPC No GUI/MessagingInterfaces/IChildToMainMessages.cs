using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessagingInterfaces
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IChildToMainMessages
    {
        [OperationContract]
        bool RegisterChild(Guid guid);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MessagingInterfaces
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IMainToChildMessages
    {
        [OperationContract]
        int EngineProcessUP(String visitKey, byte[] ups, int position, int blockSize);

        [OperationContract]
        int EngineProcessHR(String visitKey, byte[] ups, int position, int blockSize);

        [OperationContract]
        bool EngineReadResults(String visitKey, StringBuilder data, int bufferSize);

        [OperationContract(IsOneWay = true)]
        void TerminateProcess();

        [OperationContract(IsOneWay = true)]
        void FillArray(int startIndex, int length, double val);

        [OperationContract]
        List<double> GetSubArray(int start, int length);

        [OperationContract]
        String GetStringFromBuffer();
    }
}

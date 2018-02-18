using MessagingResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace MessagingInterfaces
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IPatternsProcessorMessages
    {
        [OperationContract]
        StatusEngineResponse GetStatus();

        [OperationContract]
        EngineResponseBase AddEpisode(String visitKey);

        [OperationContract]
        EngineResponseBase RemoveEpisode(String visitKey);

        [OperationContract]
        EngineResponseBase RemoveAllEpisodes();

        [OperationContract]
        ResultsEngineResponse EngineProcessData(String visitKey, byte[] ups, byte[] hrs, int startIndex, int length);

        [OperationContract(IsOneWay = true)]
        void TerminateProcess();
    }
}

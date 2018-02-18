using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using PatternsAddOnManager;

namespace PatternsAddOnService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPatternsService" in both code and config file together.
    [ServiceContract]
    public interface IPatternsService
    {
        // Root
        [OperationContract]
        [WebGet(UriTemplate = "")]
        ServiceStatus GetServiceStatus();

        // About
        [OperationContract]
        [WebGet(UriTemplate = "/About")]
        About GetServiceAbout();

        // Sessions
        [OperationContract]
        [WebGet(UriTemplate = "/Sessions")]
        SessionsList GetList();

        [OperationContract]
        [WebInvoke(UriTemplate = "/Sessions/{GestationalAge}", Method = "PUT")]
        Session InitSession(String GestationalAge);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Sessions", Method = "DELETE")]
        void PurgeAll();

        // Sessions/{Token}
        [OperationContract]
        [WebGet(UriTemplate = "/Sessions/{Token}")]
        Session GetStatus(String Token);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Sessions/{Token}", Method = "POST")]
        Session CalculatePatterns(String Token, TracingData inData);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Sessions/{Token}", Method = "DELETE")]
        void PurgeToken(String Token);

        // Sessions/{Token}/Artifacts
        [OperationContract]
        [WebGet(UriTemplate = "/Sessions/{Token}/Artifacts")]
        ArtifactsList GetResults(String Token);

    }

}

//REVIEW: 23/03/15
using CRIEntities;
using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace CRIPlugin
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPatternsService" in both code and config file together.
    [ServiceContract]
    interface ICRIPluginHost
    {
        // Root
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "")]
        PluginServiceStatus GetPluginStatus();

        // Visits
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "CRIEpisodes")]
        List<CRIEpisode> GetCRIEpisodes();

        // Get episodes for a specific visit
        [OperationContract(Name = "CRIEpisodesVisitKey")]
        [WebGet(UriTemplate = "CRIEpisodes/{visitKey}")]
        CRIEpisode GetCRIEpisodes(String visitKey);

        // Algorithm Parameters
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "AlgorithmParameters")]
        AlgorithmParameters GetAlgorithmParameters();
   
        // Visits
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "CRIEpisodes/Review/{visitkey}/user/{username}")]
        void SetCRIPositiveStatesToReviewed(String visitkey, String username);
    }
}

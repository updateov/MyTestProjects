using Export.Entities;
using PatternsEntities;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;

namespace Export.Plugin
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPatternsService" in both code and config file together.
    [ServiceContract]
    interface IPluginWebHost
    {
        // Root
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "")]
        PluginServiceStatus GetPluginStatus();

        // Get Interval
        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "Episodes/{episodeId}/Intervals/{intervalId}")]
        string GetIntervalForExport(string episodeId, string intervalId);

        // Export Interval
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "Episodes/{episodeId}/Intervals/")]
        void SaveExportedInterval(string episodeId);

        // Set Interval Exported
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "Visits/{visitkey}/Intervals/")]
        void SetIntervalExported(string visitkey);

        //get user rights
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "Users/")]
        bool CheckUserRights();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "ExportConfig/")]
        string GetExportConfig();
    }
}

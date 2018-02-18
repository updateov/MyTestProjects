using BuildRunnerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;

namespace BuildRunnerService
{
    [ServiceContract]
    public interface IBuilder
    {
        /// <summary>
        /// Get status of available and running builds
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Builds",
            ResponseFormat = WebMessageFormat.Xml,
            RequestFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        ProjectsList GetCompatibleBuilds();

        /// <summary>
        /// Get list of requested builds
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Run/Current",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        LogFile GetCurrentBuildingLog();

        /// <summary>
        /// Get list of requested builds
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Run",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        BuildRequestsList GetRunningBuilds();

        /// <summary>
        /// Request new build
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "Run/{BuildName}",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        BuildRequestsList RequestBuild(String BuildName);

        /// <summary>
        /// Delete build from requested list
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "DELETE",
            UriTemplate = "Run",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        BuildRequestsList DeleteRequest(BuildRequest param);

        /// <summary>
        /// Get list of logs of completed builds
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Completed",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        CompletedBuildsList GetCompletedBuilds();

        /// <summary>
        /// Get log of specific build
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Completed/{ProjectName}/{BuildLog}",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        LogFile GetBuildLog(String ProjectName, String BuildLog);
    }
}

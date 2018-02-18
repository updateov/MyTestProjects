using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PatternsPluginsManager
{
    [ServiceContract]
    public interface ICurveDataFeed : IPatternsDataFeed
    {
        /// <summary>
        /// Retrieve the curve patient's data
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "curve",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        XElement GetCurveData(XElement param);

        /// <summary>
        /// Update some curve related data for a patient
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "updatefields",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        XElement UpdateFields(XElement param);

        /// <summary>
        /// Returns list of snapshots for an episode
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "snapshots",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        XElement GetSnapshotsList(XElement param);

        /// <summary>
        /// Returns decision support information
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "decisionSupport",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]
        XElement GetDecisionSupportInformation(XElement param);
    }
}

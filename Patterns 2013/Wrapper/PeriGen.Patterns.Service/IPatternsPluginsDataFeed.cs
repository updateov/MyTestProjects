using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using PeriGen.Patterns.ActiveXInterface;

namespace PeriGen.Patterns.Service
{
    /// <summary>
    /// REST API extension Web service interface for Patterns plugins interaction with the server
    /// </summary>
    [ServiceContract]
    public interface IPatternsPluginsDataFeed : IPatternsDataFeed
    {
        /// <summary>
        /// Get ArtifactByID 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "data/artifact",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        XElement GetArtifactByID(XElement param);

        /// <summary>
        /// Get ArtifactByID 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "data/period",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        XElement GetPatternsDataByPeriod(XElement param);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Linq;

namespace PatternsPluginsManager
{
    [ServiceContract]
    public interface IPluginsDataFeed : ICurveDataFeed
    {
        /// <summary>
        /// Execute some plugin action on data
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke( Method = "POST", UriTemplate = "pluginaction", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        void PerformPluginAction(XElement param);

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "plugins", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        XElement GetActivePlugins();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "productinfo", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml, ResponseFormat = WebMessageFormat.Xml)]
        XElement GetProductInformation();

    }
}

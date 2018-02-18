using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;


namespace reader2
{
    [ServiceContract]
    public interface IPatterns
	
	{
		/// <summary>
		/// Get the data in incremental updates from the admitted visits in the external CIS data interface
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[OperationContract]
        [WebInvoke(
            Method = "POST",
            UriTemplate = "curve",
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml)]

		XElement GetPatternsData(XElement request);
	}
}

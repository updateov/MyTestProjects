using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System;

namespace PeriGen.Patterns.ActiveXInterface
{
	/// <summary>
	/// The main Web service interface for Patterns interaction with the server
	/// </summary>
	[ServiceContract]
	public interface IPatternsDataFeed
	{
		/// <summary>
		/// Retrieve patient's data
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[OperationContract]
		[WebInvoke(
			Method = "POST",
			UriTemplate = "data",
			BodyStyle = WebMessageBodyStyle.Bare,
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml)]
		XElement GetPatternsData(XElement param);

		/// <summary>
		/// Execute some user action on patient's data
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[OperationContract]
		[WebInvoke(
			Method = "POST",
			UriTemplate = "useraction",
			BodyStyle = WebMessageBodyStyle.Bare,
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml)]
		void PerformUserAction(XElement param);

		/// <summary>
		/// Retrieve patients list
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[OperationContract]
		[WebGet(
			UriTemplate = "patients",
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml,
			BodyStyle = WebMessageBodyStyle.Bare)]
		XElement GetPatientList();

		/// <summary>
		/// Close patient
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[OperationContract]
		[WebInvoke(
			RequestFormat = WebMessageFormat.Xml,
			ResponseFormat = WebMessageFormat.Xml,
			BodyStyle = WebMessageBodyStyle.Bare,
			UriTemplate = "close/{id}",
			Method = "POST")]
		void ClosePatient(string id);

        /// <summary>
        /// Retrieve plugins
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "Plugins",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]        
        XElement GetPlugins();


        /// <summary>
        /// Retrieve product and logo information
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [OperationContract]
        [WebGet(
            UriTemplate = "productinfo",
            RequestFormat = WebMessageFormat.Xml,
            ResponseFormat = WebMessageFormat.Xml,
            BodyStyle = WebMessageBodyStyle.Bare)]
        XElement GetProductInformation();
    }
}

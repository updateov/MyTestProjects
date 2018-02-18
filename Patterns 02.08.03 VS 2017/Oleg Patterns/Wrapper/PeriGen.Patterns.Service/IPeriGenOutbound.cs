using System.ServiceModel;
using System.Xml.Linq;

namespace PeriGen.Patterns.Service
{
	/// <summary>
	/// The actual contract for the outboun data interface
	/// It's only ONE API
	/// </summary>
	[ServiceContract]
	public interface IPeriGenOutbound
	{
		/// <summary>
		/// Get the data in incremental updates from the admitted visits in the external CIS data interface
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[OperationContract]
		XElement GetData(XElement request);
	}
}

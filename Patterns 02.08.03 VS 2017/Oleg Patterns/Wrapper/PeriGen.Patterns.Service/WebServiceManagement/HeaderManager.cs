using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PeriGen.Patterns.WebServiceManagement
{
	/// <summary>
	/// Manage message headers values
	/// </summary>
	public static class HeaderManager
	{
		/// <summary>
		/// Add item to header
		/// </summary>
		/// <param name="headerKey">header key</param>
		/// <param name="value">header value</param>
		public static void AddHeader(String headerKey, object value)
		{
			OperationContext.Current.OutgoingMessageHeaders.Add(MessageHeader.CreateHeader(headerKey, Constants.HeaderNamespace, value));
		}

		/// <summary>
		/// Find specific value in header
		/// </summary>
		/// <typeparam name="T">type of value to retrieve</typeparam>
		/// <param name="headerKey">value's key</param>
		/// <returns></returns>
		public static T GetHeader<T>(String headerKey)
		{
			///find index in the header
			int indexOfHeader = OperationContext.Current.IncomingMessageHeaders.FindHeader(headerKey, Constants.HeaderNamespace);

			//check if key exist
			if (indexOfHeader >= 0)
			{
				//return value
				return OperationContext.Current.IncomingMessageHeaders.GetHeader<T>(indexOfHeader);
			}
			//return null or default value for T
			return default(T);
		}
	}
}

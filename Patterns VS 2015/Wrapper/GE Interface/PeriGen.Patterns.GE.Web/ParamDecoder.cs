using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using System.Configuration;
using PeriGen.Patterns.Settings;

namespace PeriGen.Patterns.GE.Web
{
	/// <summary>
	/// Summary description for ParamDecoder
	/// </summary>
	internal class ParamDecoder
	{
		/// <summary>
		/// Decode url parameters from GE website
		/// </summary>
		/// <param name="data">QueryString encoded in Hex</param>
		/// <returns></returns>
		public static Dictionary<string, string> Decode(string data)
		{
			//dictionary with key values pairs
			Dictionary<string, string> dic = new Dictionary<string, string>();
			if(!string.IsNullOrEmpty(data))
			{
				//checking if it contains full URL
				int index = data.IndexOf("?");
				if(index > -1)
				{
					//full URL: extract query string				
					data = data.Substring(index + 1);
				}

				//try to decode
				try
				{
					string strDecoded = data;
					if (!data.Contains("&string"))
					{
						//key
						string zRC4Key = SettingsManager.GetValue("controlNumber"); ;

						//decode
						strDecoded = Security.RC4.Decrypt(data, zRC4Key);
					}

					//find pairs of values                
					string[] parameters = strDecoded.Split('&');

					foreach(var item in parameters)
					{
						//Check if value is empty
						if(string.IsNullOrEmpty(item))
						{
							continue;
						}

						//split key/values
						if(item.Contains('='))
						{
							string[] values = item.Split('=');
							dic.Add(values[0], values[1]);
						}
					}
				}
				catch(Exception ex)
				{
					//can't decode
					Debug.WriteLine(ex.ToString());
					throw new InvalidOperationException("Cannot decode values", ex);
				}

			}
			return dic;
		}
	}
}
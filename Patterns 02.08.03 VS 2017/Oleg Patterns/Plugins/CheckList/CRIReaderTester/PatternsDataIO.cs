using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using RestSharp;
using CRIEntities;

namespace reader2
{
    class PatternsDataIO
    {
        public static string SendRequest(string uri, string body)
        {

            var client = new RestClient(uri);
            var request = new RestRequest("CRIEpisodes", Method.GET);
            request.Timeout = 3000;

            //var responseBody = client.Execute(request);

            var response = client.Execute<List<Visit>>(request);

            //if (response.StatusCode != System.Net.HttpStatusCode.OK)
            //{
            //    PeriGenLogger.Logger.WriteLogEntry(TraceEventType.Error, Properties.Resources.CRIClient_ModuleName, "Error HttpStatusCode status.");
            //    return null;
            //}

            //return (List<Visit>)response.Data

            var reply = response.Content;

            return reply;
        }
    }
}

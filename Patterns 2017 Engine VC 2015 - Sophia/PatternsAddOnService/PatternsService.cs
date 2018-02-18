using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using PatternsAddOnManager;
using System.Net;
using System.ServiceModel.Web;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using PeriGenSettingsManager;
using CommonLogger;

namespace PatternsAddOnService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PatternsService" in both code and config file together.
    public class PatternsService : IPatternsService
    {
        #region Root resource

        /// <summary>
        /// GET Verb
        /// Returns service status to the requester
        /// </summary>
        /// <returns></returns>
        public ServiceStatus GetServiceStatus()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            String verStr = String.Format("{0:00}.{1:00}.{2:00}", ver.Major, ver.Minor, ver.Build);
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Service Status", "Return the status");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            var upTime = DateTime.Now - AddOnTask.StartTime;
            String hours = upTime.Hours > 9 ? upTime.Hours.ToString() : "0" + upTime.Hours.ToString();
            String minutes = upTime.Minutes > 9 ? upTime.Minutes.ToString() : "0" + upTime.Minutes.ToString();
            String seconds = upTime.Seconds > 9 ? upTime.Seconds.ToString() : "0" + upTime.Seconds.ToString();
            var toRet = new ServiceStatus() 
            { 
                ServiceVersion = "Patterns Add-On, Version: " + verStr, 
                StartTime = AddOnTask.StartTime
            };

            return toRet;
        }

        #endregion

        #region About resource

        public About GetServiceAbout()
        {
            var imagePath = AppDomain.CurrentDomain.BaseDirectory + "About.png";
            var bytes = File.ReadAllBytes(imagePath);
            String base64Image = Convert.ToBase64String(bytes);
            About toRet = new About()
            {
                AboutImage = base64Image
            };

            return toRet;
        }

        #endregion

        #region Sessions resource

        /// <summary>
        /// GET Verb
        /// Returns list of active (live) sessions to the requester
        /// </summary>
        /// <returns></returns>
        public SessionsList GetList()
        {
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, List Tokens", "Create list of Tokens");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            return SessionsList.GetList();
        }

        /// <summary>
        /// PUT Verb
        /// Initializes new session (request for new GUID create)
        /// GestationalAge is required, in case GA is less/more than accepted minimum/maximum 406 is returned
        /// </summary>
        /// <returns></returns>
        public Session InitSession(String GestationalAge)
        {
            int ga;
            bool bSucc = Int32.TryParse(GestationalAge, out ga);
            if (!bSucc || ga < 36 || ga > 42)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotAcceptable;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Init Session", "Gestational age doesn't meet the requrements");
                return new Session();
            }

            var guidHndlr = GUIDHandler.Init();
            var guidStr = guidHndlr.GenerateGUID(ga);
            if (guidStr.Equals(String.Empty))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NoContent;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Init Session", "Failed to create Token");
                return null;
            }

            var curGuid = new Guid(guidStr);
            var engineHandle = guidHndlr.GetEngineHandle(curGuid);
            var session = new Session() { TokenId = guidStr, EngineHandle = engineHandle, GestationalAge = ga };
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Init Session", "Token created successfully");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Created;
            return session;
        }

        /// <summary>
        /// DELETE Verb
        /// Remove all sessions
        /// </summary>
        public void PurgeAll()
        {
            var guidHndlr = GUIDHandler.Init();
            bool bSucc = guidHndlr.PurgeAll();
            Logger.WriteLogEntry(bSucc ? TraceEventType.Information : TraceEventType.Error, 
                                "Patterns Add On Service, Patterns Service, Init Session", 
                                bSucc ? "Tokens deleted successfully" : "Failed to delete all Tokens");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
        }

        #endregion

        #region Sessions/{Token} resource

        /// <summary>
        /// GET Verb
        /// Get status of session, if GUID exists - returns the Token ID with status 200, 
        /// otherwise empty Token ID with status 204
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Session GetStatus(String Token)
        {
            var guidHndlr = GUIDHandler.Init();
            var curGuid = new Guid(Token);
            bool bExist = guidHndlr.FindGuid(curGuid);
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Get Token Status", 
                                    bExist ? "Token " + curGuid.ToString() + " found" : "Token not found");
            WebOperationContext.Current.OutgoingResponse.StatusCode = bExist ? HttpStatusCode.OK : HttpStatusCode.NoContent;
            return new Session() { TokenId = bExist ? curGuid.ToString() : String.Empty };
        }

        /// <summary>
        /// POST Verb
        /// Request for Patterns calculation.
        /// If Token found - returns Token ID with 202, otherwise no Toke with 204.
        /// </summary>
        /// <param name="Token">Token ID</param>
        /// <param name="inData">Input tracing data</param>
        /// <returns>TokenID</returns>
        public Session CalculatePatterns(String Token, TracingData inData)
        {
            var guidHndlr = GUIDHandler.Init();
            var curGuid = new Guid(Token);
            bool bExist = guidHndlr.FindGuid(curGuid);
            if (!bExist)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Calculate Patterns", "GUID not found");
                return new Session() { TokenId = String.Empty };
            }

            var session = guidHndlr.GetSession(curGuid);
            if (session == null)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NoContent;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Calculate Patterns", "Couldn't find session");
                return new Session() { TokenId = curGuid.ToString() };
            }

            // Check that results aren't stored for more than half an hour after last results request.
            var resultsLength = DateTime.Now - session.LastResultRequest;

            //System.Diagnostics.Trace.WriteLine("Last result request: " + resultsLength.TotalMinutes.ToString() + " ago");
            if (resultsLength.TotalMinutes > Settings_MaxAmountOfResults)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
                Logger.WriteLogEntry(TraceEventType.Critical, "Patterns Add On Service, Patterns Service, Calculate Patterns", "Maximum size of results exceeded");
                return new Session() { TokenId = curGuid.ToString() };
            }

            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Service, Patterns Service, Calculate Patterns", "Session found for GUID: " + curGuid.ToString());

            if (!session.AppendRequest(inData))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NoContent;
                Logger.WriteLogEntry(TraceEventType.Critical, "Patterns Add On Service, Patterns Service, Calculate Patterns", "Input slice length exceeded");
                return new Session() { TokenId = curGuid.ToString() };
            }

            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Calculate Patterns", "Tracings accepted");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
            return new Session() { TokenId = curGuid.ToString() };
        }

        /// <summary>
        /// DELETE Verb
        /// Closes specific session
        /// </summary>
        /// <param name="Token"></param>
        public void PurgeToken(String Token)
        {
            var guidHndlr = GUIDHandler.Init();
            var curGuid = new Guid(Token);
            guidHndlr.DeleteGuid(curGuid);
            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Delete Token", "Token deleted");
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
        }

        #endregion

        #region Sessions/{Token}/Artifacts resource

        /// <summary>
        /// GET Verb
        /// Poll results
        /// </summary>
        /// <param name="Token"></param>
        public ArtifactsList GetResults(String Token)
        {
            var guidHndlr = GUIDHandler.Init();
            var curGuid = new Guid(Token);
            bool bExist = guidHndlr.FindGuid(curGuid);
            if (!bExist)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Get Results", "GUID not found");
                return ArtifactsList.GetEmptyList();
            }

            var session = guidHndlr.GetSession(curGuid);
            if (session == null)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.NoContent;
                Logger.WriteLogEntry(TraceEventType.Error, "Patterns Add On Service, Patterns Service, Get Results", "Couldn't find session");
                return ArtifactsList.GetEmptyList();
            }

            session.LastResultRequest = DateTime.Now;
            var listToRet = ArtifactsList.GetList(session);

            Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Service, Patterns Service, Get Results", "Results sent");
            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Service, Patterns Service, Get Results", "Results sent for GUID: " + curGuid.ToString());
       
            return listToRet;
        }

        #endregion

        static int Settings_MaxAmountOfResults = AppSettingsMngr.GetSettingsIntValue("MaxAmountOfResults");
    }
}

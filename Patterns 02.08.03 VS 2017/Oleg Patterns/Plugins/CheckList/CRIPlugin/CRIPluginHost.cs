using CommonLogger;
using CRIEntities;
using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace CRIPlugin
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CRIPluginService" in both code and config file together.
    public class CRIPluginHost : ICRIPluginHost
    {
        /// <summary>
        /// GET Verb
        /// Returns plugin service status to the requester
        /// </summary>
        /// <returns></returns>
        public PluginServiceStatus GetPluginStatus()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            String strVersion = String.Format("{0:00}.{1:00}.{2:00}", version.Major, version.Minor, version.Build);

            PluginServiceStatus toRet = new PluginServiceStatus()
            {
                PluginServiceName = "CheckListPluginTask",
                Version = strVersion,
                StartTime = CRIPluginTask.StartTime
            };

            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", "Return the service status: " + strVersion);
            return toRet;
        }

        /// <summary>
        /// GET Verb
        /// Returns list of active visits to the requester
        /// </summary>
        /// <returns></returns>
        public List<CRIEpisode> GetCRIEpisodes()
        {
            List<CRIEpisode> result = new List<CRIEpisode>();

            try
            {
                CRIPluginLaborer criPluginLaborer = CRIPluginLaborer.Instance;
                var episodes = criPluginLaborer.Episodes;

                foreach (var criPluginEpisode in episodes.Values)
                {
                    CRIEpisode criEpisode = GetEpisodeFromPluginEpisode(criPluginEpisode);
                    result.Add(criEpisode);
                }

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", String.Format("GetCRIEpisodes returned {0} visits", episodes.Count));
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", String.Format("Error on GetCRIEpisodes"), ex);
            }

            return result;
        }

        /// <summary>
        /// GET Verb
        /// Returns the episode for a specific visit key
        /// </summary>
        /// <returns></returns>
        public CRIEpisode GetCRIEpisodes(String visitKey)
        {
            CRIEpisode result = null;

            try
            {
                CRIPluginLaborer criPluginLaborer = CRIPluginLaborer.Instance;
                var episodes = criPluginLaborer.Episodes;
                if (episodes.Values != null && episodes.Values.Count > 0)
                {
                    CRIPluginEpisode criPluginEpisode = episodes.Values.FirstOrDefault(episode => episode.VisitKey == visitKey);
                    if (criPluginEpisode != null)
                        result = GetEpisodeFromPluginEpisode(criPluginEpisode);
                }

                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                if (result != null)
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", "GetCRIEpisodes(visitKey) returned one visit");
                else
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", "GetCRIEpisodes(visitKey) returned no visit");
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", String.Format("Error on GetCRIEpisodes(visitKey"), ex);
            }

            return result;
        }

        private static CRIEpisode GetEpisodeFromPluginEpisode(CRIPluginEpisode criPluginEpisode)
        {
            CRIEpisode criEpisode = new CRIEpisode();
            criEpisode.VisitKey = criPluginEpisode.VisitKey;
            criEpisode.CurrentDisplayCRI = criPluginEpisode.CurrentCRIStatus;

            criEpisode.LastHourContractilities = criPluginEpisode.GetLastHourContractilities();

            if (criEpisode.CurrentDisplayCRI.CRIStatus == CRIState.PositivePastNotYetReviewed || criEpisode.CurrentDisplayCRI.CRIStatus == CRIState.PositiveCurrent)
            {
                criEpisode.PositivePastNotYetReviewedCRIs = criPluginEpisode.GetPositivetNotYetReviewed();
                if (criEpisode.CurrentDisplayCRI.CRIStatus == CRIState.PositiveCurrent)
                {
                    criEpisode.PositivePastNotYetReviewedCRIs.RemoveAt(criEpisode.PositivePastNotYetReviewedCRIs.Count - 1);
                }
            }

            return criEpisode;
        }

        /// <summary>
        /// GET Verb
        /// Returns algorithm parameters
        /// </summary>
        /// <returns></returns>
        public AlgorithmParameters GetAlgorithmParameters()
        {
            AlgorithmParameters algParams = new AlgorithmParameters();

            try
            {
                algParams.MinimalBaselineVariability = CRIPluginSettings.Instance.MinimalBaselineVariability;
                algParams.MinimalLateDecelConfidence = CRIPluginSettings.Instance.MinimalLateDecelConfidence;
                algParams.MinimalAccelerationsAmount = CRIPluginSettings.Instance.MinimalAccelerationsAmount;
                algParams.MinimalLateDecelAmount = CRIPluginSettings.Instance.MinimalLateDecelAmount;
                algParams.MinimalLargeAndLongDecelAmount = CRIPluginSettings.Instance.MinimalLargeAndLongDecelAmount;
                algParams.MinimalLateAndLargeAndLongDecelAmount = CRIPluginSettings.Instance.MinimalLateAndLargeAndLongDecelAmount;
                algParams.MinimalLateAndProlongedDecelAmount = CRIPluginSettings.Instance.MinimalLateAndProlongedDecelAmount;
                algParams.MinimalProlongedDecelHeight = CRIPluginSettings.Instance.MinimalProlongedDecelHeight;
                algParams.MinimalContractionsAmount = CRIPluginSettings.Instance.MinimalContractionsAmount;
                algParams.MinimalLongContractionsAmount = CRIPluginSettings.Instance.MinimalLongContractionsAmount;
                algParams.CRIStateQualificationWindowSize = CRIPluginSettings.Instance.CRIStateQualificationWindowSize;
                algParams.MinimalAmountOfDataInQualificationWindow = CRIPluginSettings.Instance.MinimalAmountOfDataInQualificationWindow;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Verbose, "GetAlgorithmParameters", String.Format("Error on GetAlgorithmParameters"), ex);
            }

            return algParams;
        }

        /// <summary>
        /// POST verb
        /// set CRI positive states to positive reviewed for episodeKey
        /// </summary>
        /// <param name="episodeKey"></param>
        /// <returns></returns>
        public void SetCRIPositiveStatesToReviewed(string visitkey, string username)
        {
            try
            {
                HttpStatusCode statusCode = HttpStatusCode.BadRequest;
                CRIPluginEpisode episode = CRIPluginLaborer.Instance.Episodes.Values.FirstOrDefault(e => e.VisitKey == visitkey);
                if (episode != null)
                {
                    bool bSucc = episode.SetPersistenciesToAck(username, DateTime.UtcNow);
                    statusCode = bSucc ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                }

                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", String.Format("SetCRIPositiveStatesToReviewed was called for episode {0}, status {1}", visitkey, statusCode));
                WebOperationContext.Current.OutgoingResponse.StatusCode = statusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginService", String.Format("Error on SetCRIPositiveStatesToReviewed"), ex);
            }
        }
    }
}

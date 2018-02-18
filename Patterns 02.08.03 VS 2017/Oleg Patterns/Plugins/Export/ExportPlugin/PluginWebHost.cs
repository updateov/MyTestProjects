//Review: 27/12/2015
using Export.Entities;
using PatternsEntities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Xml.Linq;
using PatternsPluginsCommon;
using CommonLogger;
using Export.Entities.ExportControlConfig;
using MMSInterfaces;

namespace Export.Plugin
{
    public class PluginWebHost : IPluginWebHost
    {
        XmlSerializer m_intervalSerializer = new XmlSerializer(typeof(Interval));
        XmlSerializer m_userDetailsSerializer = new XmlSerializer(typeof(UserDetails));

        public PluginServiceStatus GetPluginStatus()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string strVersion = String.Format("{0:00}.{1:00}.{2:00}.{3:00}", version.Major, version.Minor, version.Build, version.Revision);

            PluginServiceStatus toRet = new PluginServiceStatus()
            {
                PluginServiceName = "ExportPluginTask",
                Version = strVersion,
                StartTime = PluginTask.StartTime
            };

            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
            Logger.WriteLogEntry(TraceEventType.Verbose, "ExportPluginWebHost", "Return the host status: " + strVersion);
            return toRet;
        }

        public string GetIntervalForExport(string episodeIdStr, string intervalIdStr)
        {
            string result = null;
            try
            {
                int episodeId = -1;
                bool bSuccEpisodeId = Int32.TryParse(episodeIdStr, out episodeId);

                int intervalId = -1;
                bool bSuccIntervalId = Int32.TryParse(intervalIdStr, out intervalId);

                if (episodeId >= 0 && intervalId >= 0)
                {
                    PluginEpisode episode = PluginLaborer.Instance.Episodes[episodeId];
                    Interval interval = episode.GetIntervalForExport(intervalId);

                    using (var stream = new MemoryStream())
                    {
                        m_intervalSerializer.Serialize(stream, interval);
                        stream.Position = 0;

                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            XElement serializedResult = XElement.Load(reader);
                            result = EncDec.RijndaelEncrypt(serializedResult.ToString(), PluginSettings.Instance.EncryptionPassword);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, "ExportPluginWebHost", "Error on GetIntervalForExport", ex);
                throw ex;
            }

            return result;
        }

        public void SaveExportedInterval(string episodeIdStr)
        {
            try
            {
                var requestMessage = OperationContext.Current.RequestContext.RequestMessage;
                string encryptedString = String.Empty;
                using (var xmlReader = OperationContext.Current.RequestContext.RequestMessage.GetReaderAtBodyContents())
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    while (xmlReader.Read())
                        stringBuilder.AppendLine(xmlReader.Value);

                    string base64String = stringBuilder.ToString();
                    var byteArray = Convert.FromBase64String(base64String);
                    encryptedString = System.Text.Encoding.UTF8.GetString(byteArray);
                }

                string decryptedString = EncDec.RijndaelDecrypt(encryptedString, PluginSettings.Instance.EncryptionPassword);
                StringReader decryptedStringReader = new StringReader(decryptedString);

                Interval exportedInterval = m_intervalSerializer.Deserialize(decryptedStringReader) as Interval;

                int episodeId = -1;
                bool bSuccEpisodeId = Int32.TryParse(episodeIdStr, out episodeId);

                bool bSucc = false;
                if (episodeId >= 0)
                {
                    PluginEpisode episode = PluginLaborer.Instance.Episodes[episodeId];
                    bSucc = episode.SaveExportedInterval(exportedInterval, false);
                }

                if (!bSucc)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Conflict;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, "ExportPluginWebHost", "Error on SaveExportedInterval", ex);

                throw ex;
            }
        }

        public void SetIntervalExported(string visitkey)
        {
            try
            {
                var requestMessage = OperationContext.Current.RequestContext.RequestMessage;
                string encryptedString = String.Empty;
                using (var xmlReader = OperationContext.Current.RequestContext.RequestMessage.GetReaderAtBodyContents())
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    while (xmlReader.Read())
                        stringBuilder.AppendLine(xmlReader.Value);

                    string base64String = stringBuilder.ToString();
                    var byteArray = Convert.FromBase64String(base64String);
                    encryptedString = System.Text.Encoding.UTF8.GetString(byteArray);
                }

                string decryptedString = EncDec.RijndaelDecrypt(encryptedString, PluginSettings.Instance.EncryptionPassword);
                StringReader decryptedStringReader = new StringReader(decryptedString);

                Interval exportedInterval = m_intervalSerializer.Deserialize(decryptedStringReader) as Interval;
  
                bool bSucc = false;
                if (!String.IsNullOrEmpty(visitkey))
                {
                    System.Collections.Generic.List<PluginEpisode> episodes = (from kvp in PluginLaborer.Instance.Episodes where kvp.Value.VisitKey == visitkey select kvp.Value).ToList();
                    if (episodes != null && episodes.Count > 0)
                    {
                        PluginEpisode episode = episodes.First();
                        bSucc = episode.SaveExportedInterval(exportedInterval, true);
                    }
                }

                if (!bSucc)
                {
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Conflict;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, "ExportPluginWebHost", "Error on SaveExportedInterval", ex);

                throw ex;
            }

        }
        public bool CheckUserRights()
        {
            bool bRes = false;
            try
            {
                var requestMessage = OperationContext.Current.RequestContext.RequestMessage;
                string encryptedString = String.Empty;
                using (var xmlReader = OperationContext.Current.RequestContext.RequestMessage.GetReaderAtBodyContents())
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    while (xmlReader.Read())
                        stringBuilder.AppendLine(xmlReader.Value);

                    string base64String = stringBuilder.ToString();
                    var byteArray = Convert.FromBase64String(base64String);
                    encryptedString = System.Text.Encoding.UTF8.GetString(byteArray);
                }

                string decryptedString = EncDec.RijndaelDecrypt(encryptedString, PluginSettings.Instance.EncryptionPassword);
                StringReader decryptedStringReader = new StringReader(decryptedString);
                                
                UserDetails userDetails = m_userDetailsSerializer.Deserialize(decryptedStringReader) as UserDetails;

                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                String url = PluginSettings.Instance.CALMServerLink;
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                IMMService conn = chn.CreateChannel();
                MMSUserRightsResponse res = conn.CheckUserRights(userDetails.UserName, userDetails.Password, userDetails.Function, userDetails.Action);
                

                switch (res.UserRights)
                {
                    case MMAuthorizationResult.IsAuthorised:
                        bRes = true;
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK; break;
                    case MMAuthorizationResult.IsNotAuthorised:
                        bRes = false;
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized; break;
                    case MMAuthorizationResult.WrongLoginOrPassword:
                        bRes = false;
                        WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Conflict; break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Error, "ExportPluginWebHost", "Error on CheckUserRights", ex);
                throw ex;
            }

            return bRes;
        }

        public string GetExportConfig()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportConfig));
            string exportConfig = String.Empty;

            using (var stream = new MemoryStream())
            {
                xmlSerializer.Serialize(stream, ExportManager.Instance.ExportDataConfig);
                stream.Position = 0;

                using (XmlReader reader = XmlReader.Create(stream))
                {
                    XElement serializedResult = XElement.Load(reader);
                    exportConfig = serializedResult.ToString();
                }
            }
                  
            return exportConfig;
        }
    }
}

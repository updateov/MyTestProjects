using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using PeriGenSettingsManager;
using CommonLogger;

namespace PatternsAddOnManager
{
    public class GUIDHandler
    {
        #region Singleton functionality

        private GUIDHandler()
        {
            Guid2SessionData = new Dictionary<Guid, PatternsSessionData>();
            if (Settings_TokenTTL < 0)
                Settings_TokenTTL = 60;

            TTLToken = new Timer(TokenTimeToLive);
            TTLToken.Change(0, 60000);
        }

        public static GUIDHandler Init()
        {
            if (s_GUIDHandler == null)
            {
                lock (s_lockObject)
                {
                    if (s_GUIDHandler == null)
                        s_GUIDHandler = new GUIDHandler();
                }
            }

            return s_GUIDHandler;
        }

        #endregion

        #region Timer TTL Token

        private static void TokenTimeToLive(object data)
        {
            lock (s_lockObject)
            {
                var guidHndlr = Init();
                var guids = from c in guidHndlr.Guid2SessionData
                            where ((DateTime.Now - c.Value.LastRequest).TotalMinutes > Settings_TokenTTL)
                            select c.Key;

                Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, GUID Handler, Token Time To Live", "Clean unused tokens");
                guidHndlr.Guid2SessionData.RemoveRange(guids);
            }
        }

        #endregion

        public String GenerateGUID(int gestationalAge = 0)
        {
            if (Guid2SessionData.Count >= 100)
            {
                Logger.WriteLogEntry(TraceEventType.Warning, "Patterns Add On Manager, GUID Handler, Generate GUID", "A limit of 100 tokens reached on this machine");
                return String.Empty;
            }

            lock (s_lockObject)
            {
                var curGuid = Guid.NewGuid();
                int cnt = 100;
                while (--cnt > 0)
                {
                    if (curGuid.CompareTo(Guid.Empty) == 0)
                        continue;

                    var sameGUID = from c in Guid2SessionData.Keys
                                   where c.CompareTo(curGuid) == 0
                                   select c;

                    if (sameGUID == null || sameGUID.Count() == 0)
                        break;

                    curGuid = Guid.NewGuid();
                }

                if (cnt <= 0)
                    return String.Empty;

                Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, GUID Handler, Generate GUID", "Generate new Token");
                var session = new PatternsSessionData(curGuid.ToString())
                {
                    GestationalAge = gestationalAge
                };

                if (session.Engine == null)
                    return String.Empty;

                Guid2SessionData[curGuid] = session;
                return curGuid.ToString();
            }
        }

        /// <summary>
        /// For test purposes only, comment after successful debug
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public String GetEngineHandle(Guid guid)
        {
            lock (s_lockObject)
            {
                return Guid2SessionData[guid].Engine != null ? guid.ToString() : "NULL";
            }
        }

        public bool PurgeAll()
        {
            lock (s_lockObject)
            {
                Guid2SessionData.Clear(true);
                return Guid2SessionData.Count == 0;
            }
        }

        public bool FindGuid(Guid guid)
        {
            lock (s_lockObject)
            {
                if (Guid2SessionData.ContainsKey(guid))
                    return true;

                return false;
            }
        }

        public PatternsSessionData GetSession(Guid guid)
        {
            lock (s_lockObject)
            {
                if (FindGuid(guid))
                    return Guid2SessionData[guid];
                else
                    return null;
            }       
        }

        public void DeleteGuid(Guid guid)
        {
            lock (s_lockObject)
            {
                if (Guid2SessionData.ContainsKey(guid))
                {
                    var session = Guid2SessionData[guid];
                    if (session != null)
                        session.Dispose();
                }

                Guid2SessionData.Remove(guid);
            }
        }

        static int Settings_TokenTTL = AppSettingsMngr.GetSettingsIntValue("TokenTTL");

        public Dictionary<Guid, PatternsSessionData> Guid2SessionData { get; set; }
        private static GUIDHandler s_GUIDHandler = null;
        private static Object s_lockObject = new Object();
        private Timer TTLToken { get; set; }
    }
}

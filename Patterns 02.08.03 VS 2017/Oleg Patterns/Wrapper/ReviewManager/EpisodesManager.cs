using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReviewManager
{
    public class EpisodesManager
    {
        private static Object s_lock = new Object();

        private Dictionary<String, List<FetusEpisode>> m_fileName2FetusList = null;

        #region Singleton

        private static EpisodesManager s_instance = null;

        private EpisodesManager()
        {
            m_fileName2FetusList = new Dictionary<String, List<FetusEpisode>>();
        }

        public static EpisodesManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new EpisodesManager();
                    }
                }

                return s_instance;
            }
        }

        #endregion

        public void ProcessRetrospective(String fileName)
        {
            List<TracingBlock> tracings = TracingFileReader.Read(fileName);
        }
    }
}

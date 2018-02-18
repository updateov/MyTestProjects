using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace BuildRunnerManager
{
    public class BuildManager
    {
        private static object s_lock = new object();
        public Dictionary<String, String> BuildName2Path { get; set; }

                #region Singleton

        private static BuildManager s_instance;

        private BuildManager()
        {
            BuildName2Path = new Dictionary<String, String>();
            LoadAvailableBuilds();
        }

        public static BuildManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lock)
                    {
                        if (s_instance == null)
                            s_instance = new BuildManager();
                    }
                }

                return s_instance;
            }
        }

        #endregion

        private void LoadAvailableBuilds()
        {
            foreach (var item in ConfigurationManager.AppSettings.AllKeys)
            {
                BuildName2Path[item] = ConfigurationManager.AppSettings[item];
            }
        }
    }
}

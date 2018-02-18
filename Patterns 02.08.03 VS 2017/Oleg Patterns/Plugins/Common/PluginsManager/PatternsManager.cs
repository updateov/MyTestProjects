//Review: 17/02/15
//Review: 23/03/15
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.ServiceModel.Web;
using PatternsPluginsCommon;
using System.Configuration;
using System.Reflection;
using CommonLogger;

namespace PatternsPluginsManager
{
    public class PatternsManager
    {
        #region Properties & Members

        private List<IPluginTask> m_plugins;
        private WebServiceHost m_pluginsDataFeedWebHost;                 
       
        #endregion

        #region Singleton functionality

        private static PatternsManager s_patternsManager = null;
        private static Object s_lockObject = new Object();

        private PatternsManager()
        {
            var laborer = PluginsLaborer.Instance;
            m_plugins = new List<IPluginTask>();
            LoadPlugins();           
        }

        public static PatternsManager Instance
        {
            get
            {
                if (s_patternsManager == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_patternsManager == null)
                        {
                            s_patternsManager = new PatternsManager();
                        }
                    }
                }

                return s_patternsManager;
            }
        }

        #endregion

        private void LoadPlugins()
        {
            string location = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            foreach (string configurationKey in ConfigurationManager.AppSettings.AllKeys)
            {
                try
                {
                    if (configurationKey.ToLower().Equals("config"))
                    {
                        continue;
                    }

                    string[] configurationValues = ConfigurationManager.AppSettings[configurationKey].Split(';');
                    string pluginPath = configurationValues[0];
                    string pluginType = configurationValues[1];

                    Assembly assembly = Assembly.LoadFrom(location + "\\" + pluginPath);
                    Type type = assembly.GetType(pluginType);
                    IPluginTask plugin = Activator.CreateInstance(type) as IPluginTask;

                    if (plugin != null)
                    {
                        plugin.Init(PluginsManagerSettings.Instance.PluginsDataFeed);
                        m_plugins.Add(plugin);
                        Logger.WriteLogEntry(TraceEventType.Information, "PatternsManager", String.Format("Succeed to initialize plugin \"{0}\".", pluginType));
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Warning, "PatternsManager", String.Format("Failed to Read plugin \"{0}\".", pluginType));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error while initializing plugin \"{0}\"", configurationKey), ex);
                }
            }
        }

        public void Start()
        {
            StartPlugins();
            
            InitPluginsDataFeedWebHost();
        }

        private void StartPlugins()
        {
            foreach (var plugin in m_plugins)
            {
                try
                {
                    bool bSucc = plugin.Start();
                    if (bSucc)
                    {
                        Logger.WriteLogEntry(TraceEventType.Information, "PatternsManager", String.Format("Succeed to start plugin \"{0}\".", plugin.Name()));
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "PatternsManager", String.Format("Failed to start plugin \"{0}\".", plugin.Name()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error while starting plugin \"{0}\". Error: {1}", plugin.Name(), ex.ToString()));
                }
            }
        }

        private bool InitPluginsDataFeedWebHost()
        {
            bool bRes = false;
            try
            {
                m_pluginsDataFeedWebHost = new WebServiceHost(typeof(PluginsDataFeedService), new Uri(PluginsManagerSettings.Instance.PluginsDataFeed));
                m_pluginsDataFeedWebHost.Open();

                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "PatternsManager", "Web host initialized");
                bRes = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Critical, "PatternsManager", "Web host failed to initialize, please verify a valid url address in settings tool", ex);
                throw ex;
            }

            return bRes;
        }

        public void Stop()
        {
            if(m_pluginsDataFeedWebHost != null)
            {
                m_pluginsDataFeedWebHost.Close();
            }

            StopPlugins();
        }

        private void StopPlugins()
        {
            foreach (var plugin in m_plugins)
            {
                try
                {
                    bool bSucc = plugin.Stop();
                    if (bSucc)
                    {
                        Logger.WriteLogEntry(TraceEventType.Information, "PatternsManager", String.Format("Succeed to stop plugin \"{0}\".", plugin.Name()));
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "PatternsManager", String.Format("Failed to stop plugin \"{0}\".", plugin.Name()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error while stopping plugin \"{0}\". Error: {1}", plugin.Name(), ex.ToString()));
                }
            }
        }

        public void UpdatePlugins(XElement request, string requestName, XElement result)
        {
            foreach (var plugin in m_plugins)
            {
                try
                {
                    bool bSucc = plugin.Apply(request, requestName, result);
                    if (bSucc)
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "PatternsManager", String.Format("Succeed to update plugin \"{0}\".", plugin.Name()));
                    }
                    else
                    {
                        Logger.WriteLogEntry(TraceEventType.Error, "PatternsManager", String.Format("Failed to update plugin \"{0}\".", plugin.Name()));
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error while updating plugin \"{0}\". Error: {1}", plugin.Name(), ex.ToString()));
                }
            }
        }

        public void UpdatePluginsDowntime(bool isInDownTime, bool isAfterDownTime)
        {
            foreach (var plugin in m_plugins)
            {
                try
                {
                    plugin.UpdateDowntime(isInDownTime, isAfterDownTime);
                    Logger.WriteLogEntry(TraceEventType.Verbose, "PatternsManager", String.Format("Implemented plugin UpdatePluginsDowntime \"{0}\".", plugin.Name()));
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error while implementing plugin UpdatePluginsDowntime \"{0}\". Error: {1}", plugin.Name(), ex.ToString()));
                }
            }
        }

        public IEnumerable<string> GetActivePluginsNames()
        {
            try
            {
                return m_plugins.Where(c => c.IsEnabled()).Select(p => p.Name());
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "PatternsManager", String.Format("Error in GetActivePluginsNames. Error: {0}", ex.ToString()));
            }
            return new List<string>();
        }
    }
}

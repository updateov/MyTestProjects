using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.Model;
using PanelSW.Installer.JetBA.Util;
using System;

namespace PatternsBA.Util
{
    public class EngineVariableEx : EngineVariable
    {
        public EngineVariableEx(JetBootstrapperApplication ba, MainModel model, string name)
            : base(ba, model, name)
        {
        }

        public string PaddedVersion
        {
            get
            {
                Version ver = Version;
                string padded = $"{ver.Major:00}.{ver.Minor:00}";
                if (ver.Build < 0)
                {
                    return padded;
                }
                padded += $".{ver.Build:00}";
                /* Ignoring 4th figure
                if (ver.Revision<0)
                {
                    return padded;
                }
                padded += $".{ver.Revision:00}";
                */
                return padded;
            }
            set
            {
                Version ver = Version.Parse(value);
                Version = ver;
                OnPropertyChanged("PaddedVersion");
            }
        }
    }
}

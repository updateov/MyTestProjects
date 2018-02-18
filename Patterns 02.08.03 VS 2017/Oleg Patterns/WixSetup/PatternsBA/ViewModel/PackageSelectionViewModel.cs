using PanelSW.Installer.JetBA.Model;
using PanelSW.Installer.JetBA.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatternsBA.ViewModel
{
    public class PackageSelectionViewModel : PanelSW.Installer.JetBA.ViewModel.ViewModelBase
    {
        private dynamic variablesViewModel_;

        public PackageSelectionViewModel(PatternsBAJetBA ba, MainModel model, PanelSW.Installer.JetBA.ViewModel.VariablesViewModel vars)
            : base(ba, model)
        {
            variablesViewModel_ = vars;

            EngineVariable v = variablesViewModel_.InstallChecklistPlugin;
            v.PropertyChanged += InstallChecklistPlugin_PropertyChanged;
        }

        private void InstallChecklistPlugin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!variablesViewModel_.InstallChecklistPlugin.Boolean)
            {
                variablesViewModel_.InstallCentralStationClient.Boolean = false;
            }
        }
    }
}

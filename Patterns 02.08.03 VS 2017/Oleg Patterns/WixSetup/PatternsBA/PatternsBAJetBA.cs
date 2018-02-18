using PanelSW.Installer.JetBA;
using PanelSW.Installer.JetBA.Util;
using System.Diagnostics;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using Ninject;
using PanelSW.Installer.JetBA.ViewModel;
using PanelSW.Installer.JetBA.Model;
using Microsoft.Win32;
using System;

namespace PatternsBA
{
    public class PatternsBAJetBA : JetBootstrapperApplication
    {
        private PatternsBABinder binder_ = null;
        protected override NInjectBinder Binder
        {
            get
            {
                if (binder_ == null)
                {
                    binder_ = new PatternsBABinder(this);
                }
                return binder_;
            }
        }

        protected override void Run()
        {
            if (Engine.StringVariables.Contains("BaLaunchDebugger") && Engine.BooleanVariable("BaLaunchDebugger"))
            {
                Debugger.Launch();
            }
            base.Run();
        }

        protected override void OnDetectBegin(DetectBeginEventArgs args)
        {
            // Workaround for illegal version string '01.04.01.018.21062017'- 
            try
            {
                using (RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey k = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{EF7D18CA-9FF9-4B34-9A17-D11D80CEE304}", true))
                    {
                        if (k != null)
                        {
                            string displayVersion = k.GetValue("DisplayVersion") as string;
                            if (displayVersion.Equals("01.04.01.018.21062017"))
                            {
                                Engine.Log(LogLevel.Standard, "Overwriting previous version to '01.04.01.018'");
                                k.SetValue("DisplayVersion", "01.04.01.018");
                                k.Close();
                            }
                        }
                    }
                }

                using (RegistryKey rk = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (RegistryKey k = rk.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\AC81D7FE9FF943B4A9711DD108EC3E40\InstallProperties", true))
                    {
                        if (k != null)
                        {
                            string displayVersion = k.GetValue("DisplayVersion") as string;
                            if (displayVersion.Equals("01.04.01.018.21062017"))
                            {
                                Engine.Log(LogLevel.Standard, "Overwriting previous version to '01.04.01.018'");
                                k.SetValue("DisplayVersion", "01.04.01.018");
                                k.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Engine.Log(LogLevel.Error, $"Failed overwriting previous version to '01.04.01.018': {ex.Message}. Ignoring error");
            }

            Kernel.Get<ApplyViewModel>().PlanAfterReboot = true;
            base.OnDetectBegin(args);
        }

        protected override void OnDetectComplete(DetectCompleteEventArgs e)
        {
            base.OnDetectComplete(e);

            dynamic vars = Kernel.Get<VariablesViewModel>();
            MainModel model = Kernel.Get<MainModel>();
            vars.BUNDLE_PRODUCT_CODE.String = model.BootstrapperApplicationData.BundleAttributes.Id;
        }

        protected override void OnPlanMsiFeature(PlanMsiFeatureEventArgs args)
        {
            base.OnPlanMsiFeature(args);
            dynamic vars = Kernel.Get<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel>();

            if (args.PackageId.Equals("PatternsMsi.msi"))
            {
                switch (args.FeatureId)
                {
                    case "Core":
                        args.State = FeatureState.Local;
                        break;

                    case "PatternsPluginsService":
                        args.State = vars.InstallChecklistPlugin.Boolean || vars.InstallExportPlugin.Boolean ? FeatureState.Local : FeatureState.Absent;
                        break;

                    default:
                        Engine.Log(LogLevel.Error, "Unknown feature '{0}' in PatternsMsi.msi", args.FeatureId);
                        args.Result = Result.Error;
                        break;
                }
            }

            if (args.PackageId.Equals("PluginsMsi.msi"))
            {
                string bundleName = vars.WixBundleName.String;

                switch (args.FeatureId)
                {
                    case "Core":
                    case "CheckListClient":
                        args.State = FeatureState.Local;
                        break;

                    case "CheckListPlugin":
                        args.State = vars.InstallChecklistPlugin.Boolean ? FeatureState.Local : FeatureState.Absent;
                        break;

                    case "CheckListClient_PatternsOnly":
                        args.State = bundleName.StartsWith("PeriCALM Patterns") && vars.InstallChecklistPlugin.Boolean ? FeatureState.Local : FeatureState.Absent;
                        break;

                    case "ExportPlugin":
                        args.State = vars.InstallExportPlugin.Boolean ? FeatureState.Local : FeatureState.Absent;
                        break;

                    case "ExportPlugin_PatternsOnly":
                        args.State = bundleName.StartsWith("PeriCALM Patterns") && vars.InstallExportPlugin.Boolean ? FeatureState.Local : FeatureState.Absent;
                        break;

                    default:
                        Engine.Log(LogLevel.Error, "Unknown feature '{0}' in PluginsMsi.msi", args.FeatureId);
                        args.Result = Result.Error;
                        break;
                }
            }
        }

        // If list of files in use is empty, terminate the unnamed apps.
        protected override void OnExecuteFilesInUse(ExecuteFilesInUseEventArgs args)
        {
            bool hasData = false;
            int cnt = args.Files.Count;
            for (int i = 1; i < cnt; i += 2)
            {
                string name = args.Files[i];
                if (!string.IsNullOrWhiteSpace(name))
                {
                    hasData = true;
                    break;
                }
            }

            if (hasData)
            {
                base.OnExecuteFilesInUse(args);
            }
            else
            {
                args.Result = Result.Ok;
            }
        }
    }
}
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA;
using System.Windows;

namespace PatternsBA
{
    class PatternsBABinder : NInjectBinder
    {
        public PatternsBABinder(PatternsBAJetBA ba)
            : base(ba)
        {
        }

        public override void Load()
        {
            Bind<BootstrapperApplication, JetBootstrapperApplication, PatternsBAJetBA>().ToConstant(ba_ as PatternsBAJetBA);
            Bind<PanelSW.Installer.JetBA.Model.MainModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.Localization.Resources>().ToSelf().InSingletonScope();

            // ViewModel
            Bind<PanelSW.Installer.JetBA.ViewModel.NavigationViewModel, ViewModel.NavigationViewModel>().To<ViewModel.NavigationViewModel>().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.PopupViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.ProgressViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.FinishViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.UtilViewModel>().ToSelf().InSingletonScope();
            Bind<PanelSW.Installer.JetBA.ViewModel.InputValidationsViewModel, ViewModel.InputValidationsViewModel>().To<ViewModel.InputValidationsViewModel>().InSingletonScope();
            Bind<ViewModel.PackageSelectionViewModel>().ToSelf().InSingletonScope();

            // Util
            Bind<PanelSW.Installer.JetBA.Util.EngineVariable, Util.EngineVariableEx>().To<Util.EngineVariableEx>();
            Bind<PanelSW.Installer.JetBA.Util.FileInUse>().ToSelf();

            // View
            Bind<View.InstallLocationView>().ToSelf().InSingletonScope();
            Bind<View.RepairView>().ToSelf().InSingletonScope();
            Bind<View.ProgressView>().ToSelf().InSingletonScope();
            Bind<View.FinishView>().ToSelf().InSingletonScope();
            Bind<View.HelpView>().ToSelf().InSingletonScope();
            Bind<View.SummaryView>().ToSelf().InSingletonScope();
            Bind<View.PrepareView>().ToSelf().InSingletonScope();
            Bind<View.PackageSelectionView>().ToSelf().InSingletonScope();

            Bind<Window>().To<View.RootView>().InSingletonScope();
        }
    }
}

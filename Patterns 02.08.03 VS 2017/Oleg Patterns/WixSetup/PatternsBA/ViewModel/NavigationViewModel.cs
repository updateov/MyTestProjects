using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using PanelSW.Installer.JetBA;
using PatternsBA.View;
using System;

namespace PatternsBA.ViewModel
{
    public enum Pages
    {
        Unknown,
        Prepare,
        InstallLocation,
        PackageSelection,
        Summary,
        Progress,
        Finish,
        Repair,
        Help
    }

    public class NavigationViewModel : PanelSW.Installer.JetBA.ViewModel.NavigationViewModel
    {
        private readonly Lazy<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel> applyViewModel_;
        private readonly Lazy<dynamic> variablesViewModel_;

        public NavigationViewModel(PatternsBAJetBA ba
            , PanelSW.Installer.JetBA.Model.MainModel model
            , Lazy<PanelSW.Installer.JetBA.ViewModel.ApplyViewModel> applyViewModel
            , Lazy<PanelSW.Installer.JetBA.ViewModel.PopupViewModel> popupViewModel
            , Lazy<PanelSW.Installer.JetBA.ViewModel.InputValidationsViewModel> validationsViewModel
            , Lazy<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel> variablesViewModel
            , Lazy<PanelSW.Installer.JetBA.Localization.Resources> localization
            , Lazy<InstallLocationView> installLocationView
            , Lazy<RepairView> repairView
            , Lazy<ProgressView> progressView
            , Lazy<HelpView> helpView
            , Lazy<FinishView> finishView
            , Lazy<SummaryView> summaryView
            , Lazy<PrepareView> prepareView
            , Lazy<PackageSelectionView> packagesView
            )
            : base(ba, model, applyViewModel, popupViewModel, validationsViewModel, localization)
        {
            applyViewModel_ = applyViewModel;
            variablesViewModel_ = new Lazy<dynamic>(() => variablesViewModel.Value);

            BA.DetectComplete += BA_DetectComplete;
            BA.PlanBegin += BA_PlanBegin;
            BA.ApplyComplete += BA_ApplyComplete;

            AddPage(Pages.Finish, new Lazy<object>(() => finishView.Value));
            AddPage(Pages.Help, new Lazy<object>(() => helpView.Value));
            AddPage(Pages.InstallLocation, new Lazy<object>(() => installLocationView.Value));
            AddPage(Pages.PackageSelection, new Lazy<object>(() => packagesView.Value));
            AddPage(Pages.Progress, new Lazy<object>(() => progressView.Value));
            AddPage(Pages.Repair, new Lazy<object>(() => repairView.Value));
            AddPage(Pages.Summary, new Lazy<object>(() => summaryView.Value));
            AddPage(Pages.Prepare, new Lazy<object>(() => prepareView.Value));
        }

        private void BA_DetectComplete(object sender, DetectCompleteEventArgs e)
        {
            SetStartPage();
        }

        #region Event-based navigations

        public override void Initialize()
        {
            base.Initialize();
            SetStartPage();
        }

        private void SetStartPage()
        {
            if (applyViewModel_.Value.InstallState == InstallationState.Detecting)
            {
                Page = Pages.Prepare;
                return;
            }

            if (BA.Command.Action == LaunchAction.Help)
            {
                Page = Pages.Help;
                return;
            }

            switch (applyViewModel_.Value.DetectState)
            {
                case DetectionState.Absent:
                case DetectionState.Newer:
                case DetectionState.Older:
                    Page = Pages.InstallLocation;
                    break;

                case DetectionState.Present:
                    Page = Pages.Repair;
                    break;
            }
        }

        private void BA_ApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            ClearHistory();
            Page = Pages.Finish;
        }

        private void BA_PlanBegin(object sender, PlanBeginEventArgs e)
        {
            ClearHistory();
            Page = Pages.Progress;
        }

        #endregion

        protected override object QueryNextPage()
        {
            Pages nextPage = Pages.Unknown;
            switch ((Pages)Page)
            {
                case Pages.InstallLocation:
                    string bundleName = variablesViewModel_.Value.WixBundleName.String;
                    if (bundleName.StartsWith("PeriWatch Cues "))
                    {
                        nextPage = Pages.Summary;
                    }
                    else
                    {
                        nextPage = Pages.PackageSelection;
                    }
                    break;

                case Pages.PackageSelection:
                    nextPage = Pages.Summary;
                    break;

                default:
                    throw new InvalidProgramException("Page");
            }

            return nextPage;
        }
    }
}

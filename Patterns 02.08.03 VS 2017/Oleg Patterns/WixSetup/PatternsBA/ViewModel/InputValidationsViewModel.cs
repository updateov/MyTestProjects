using System;
using PatternsBA;
using PanelSW.Installer.JetBA.Model;
using System.IO;

namespace PatternsBA.ViewModel
{
    /// <summary>
    /// Perform input validations on page transitions.
    /// For silent and passive operations, perform on all inputs at one go.
    /// The default implementation doesn't perform any validation.
    /// <para>
    /// Override 'ValidatePage' to validate a single page's inputs.
    /// Override 'ValidateAll' to validate all inputs in silent and passive operations.
    /// </para>
    /// </summary>
    public class InputValidationsViewModel : PanelSW.Installer.JetBA.ViewModel.InputValidationsViewModel
    {
        private Lazy<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel> variablesViewModel_;

        /// <summary>
        /// Utility view model
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="model"></param>
        /// <param name="vars"></param>
        public InputValidationsViewModel(PatternsBAJetBA ba, MainModel model, Lazy<PanelSW.Installer.JetBA.ViewModel.VariablesViewModel> vars)
            : base(ba, model)
        {
            variablesViewModel_ = vars;
        }

        /// <summary>
        /// Validate page inputs before going 'Next' from a page.
        /// </summary>
        /// <param name="pageId">Page key</param>
        /// <exception cref="ArgumentException">
        /// Validation failure message.
        /// <see cref="ArgumentException.ParamName"/> will be used for pop-up message caption line.
        /// <see cref="ArgumentException.Message"/> will be used for pop-up message text.
        /// </exception>
        public override void ValidatePage(object pageId)
        {
            Pages page = (Pages)pageId;

            switch (page)
            {
                case Pages.InstallLocation:
                    ValidateTargetFolder();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Validate all inputs in one go.
        /// Called on silent and passive operations only on <see cref="PatternsBAJetBA.OnDetectComplete(Microsoft.Tools.WindowsInstallerXml.Bootstrapper.DetectCompleteEventArgs)"/> event.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Validation failure message.
        /// <see cref="ArgumentException.ParamName"/> will be used for pop-up message caption line.
        /// <see cref="ArgumentException.Message"/> will be used for pop-up message text.
        /// </exception>
        public override void ValidateAll()
        {
            ValidateTargetFolder();
        }

        private void ValidateTargetFolder()
        {
            string installFolder = ((dynamic)variablesViewModel_.Value).InstallFolder.String;

            if (string.IsNullOrWhiteSpace(installFolder) || (installFolder.IndexOfAny(Path.GetInvalidPathChars()) >= 0))
            {
                throw new ArgumentException(
                    string.Format("'{0}' is not a legal folder name", installFolder)
                    , "Target Folder"
                    );
            }
        }
    }
}

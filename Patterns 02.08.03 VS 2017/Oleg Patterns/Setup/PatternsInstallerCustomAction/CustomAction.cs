using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using PeriGenSettingsManager;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace PatternsInstallerCustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult UpdateVersionInSettingsAction(Session session)
        {
            session.Log("Begin UpdateVersionInSettingsAction");
            // retrieving the value for the CustomActionData property
            string[] actionData = session.CustomActionData.ToString().Split(';');

            string filePath = Path.Combine(actionData[0], "PeriGen.Patterns.Service.psf");
            string version = actionData[1];

            AppSettingsMngr.SaveSetting(filePath, "VersionPatterns", version);
            AppSettingsMngr.SaveSetting(filePath, "VersionCurve", version);

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult MergeSettingsFilesAction(Session session)
        {
            session.Log("Begin MergeSettingsFiles");
            bool bRes = true;

            // retrieving the value for the CustomActionData property
            string[] actionData = session.CustomActionData.ToString().Split(';');
            string installPath = actionData[0];
            
            string existingFile = String.Empty;
            if (File.Exists(Path.Combine(installPath, "PeriGen.Patterns.Service.psf")))
            {
                existingFile = "PeriGen.Patterns.Service.psf";            
            }
            else if (File.Exists(Path.Combine(installPath, "PeriGen.Patterns.Service.xml")))
            {
                existingFile = "PeriGen.Patterns.Service.xml";                
            }

            string sourceNewPath = Path.Combine(installPath, "Default Config\\PeriGen.Patterns.Service.psf");            
            string destinationPath = Path.Combine(installPath, "PeriGen.Patterns.Service.psf");            
            
            if (!String.IsNullOrEmpty(existingFile))
            {
                string sourceExistingPath = Path.Combine(installPath, existingFile);                
                SettingsUpdater settingsUpdater = new SettingsUpdater();
                bRes = settingsUpdater.MergeSettings(sourceExistingPath, sourceNewPath, destinationPath);
            }
            else
            {
                File.Copy(sourceNewPath, destinationPath);
                bRes = File.Exists(destinationPath);              
            }
            
            return bRes ? ActionResult.Success : ActionResult.Failure;
        }
    }
}

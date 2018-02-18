using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using PeriGenSettingsManager;
using System.IO;

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
    }
}

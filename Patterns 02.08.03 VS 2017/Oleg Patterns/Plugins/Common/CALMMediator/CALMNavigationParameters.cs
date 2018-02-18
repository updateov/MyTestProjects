using PatternsPluginsCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace PatternsCALMMediator
{
    public class CALMNavigationParameters
    {
        public string SourceUrl { get; set; }
        public string TargetFrameName { get; set; }
        public byte[] PostData { get; set; }
        public string AdditionalHeaders { get; set; }

        public CALMNavigationParameters(string visitKey)
        {
            TargetFrameName = String.Empty;
            AdditionalHeaders = "Content-Type: application/xml; charset=utf-8";

            LMS.Data.SiteData site = LMS.Data.SiteData.Instance();
            if (site != null)
            {
                SourceUrl = site.SiteConfig.URLWEBScreenBottom1;
                PostData = GetBuildParameters(site, visitKey, IntPtr.Zero);
            }  
        }

        public CALMNavigationParameters(string visitKey, IntPtr windowHandle)
        {
            TargetFrameName = String.Empty;
            AdditionalHeaders = "Content-Type: application/xml; charset=utf-8";

            LMS.Data.SiteData site = LMS.Data.SiteData.Instance();
            if (site != null)
            {
                SourceUrl = site.SiteConfig.URLWEBScreenBottom1;
                PostData = GetBuildParameters(site, visitKey, windowHandle);
            }            
        }

        private Byte[] GetBuildParameters(LMS.Data.SiteData site, string visitKey, IntPtr windowHandle)
        {
            string buildParameters = CreateBuildParameters(site, visitKey, windowHandle);

            //encrypt puildParameters if needed
            string encryptionPassword = site.SiteConfig.KeyWEBScreenBottom1;
            if (!String.IsNullOrEmpty(encryptionPassword))
            {
                buildParameters = EncDec.RijndaelEncrypt(buildParameters, encryptionPassword);
            }

            Byte[] result = Encoding.UTF8.GetBytes(buildParameters);

            return result;
        }

        /// <summary>
        /// Build the XML string that contains the parameters for the web page
        /// </summary>
        /// <returns></returns>
        private string CreateBuildParameters(LMS.Data.SiteData site, string visitKey, IntPtr windowHandle)
        {
            LMS.Data.Transient.User user = site.CurrentUser;
            if (user == null)
            {
                return null;
            }

            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("parameters");
            xmlDoc.AppendChild(root);
            string usernameAddition = String.Empty;
            if (windowHandle != IntPtr.Zero) 
                 usernameAddition = "@%" + windowHandle.ToString();
            if (user.IsSSOUser)
            {
                root.SetAttribute("user_key", site.SSOUserID);
                root.SetAttribute("user_name", site.SSOUserName + usernameAddition);
                root.SetAttribute("user_id", site.SSOUserLogon);
                root.SetAttribute("is_default_user", false.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                root.SetAttribute("user_key", string.Format(CultureInfo.InvariantCulture, "{0}-{1}", user.UserKey.SiteNo, user.UserKey.UserNo));
                root.SetAttribute("user_name", user.DisplayName + usernameAddition);
                root.SetAttribute("user_id", user.UserID);
                root.SetAttribute("is_default_user", user.IsDefaultUser.ToString(CultureInfo.InvariantCulture));
            }

            
            //root.SetAttribute("CRIWindowHandle", windowHandle.ToString());

            root.SetAttribute("workstation", site.WorkStationName);
            root.SetAttribute("domain", site.DomainName);

            string msg = string.Empty;
            bool canModify = CALMMediatorSettings.Instance.CanExecutePatternsActions; // CheckUserGroupRights(4000600, 1071010 /*USERGROUP_A_MODIFY*/, ref msg, false).ToString(CultureInfo.InvariantCulture)
            root.SetAttribute("can_modify", canModify.ToString());
            bool canPrint = CALMMediatorSettings.Instance.CanExecutePatternsActions; // CheckUserGroupRights(4000600, 1071005 /*USERGROUP_A_PRINT*/, ref msg, false).ToString(CultureInfo.InvariantCulture)
            root.SetAttribute("can_print", canPrint.ToString());            
            root.SetAttribute("visit_key", visitKey);

            // Done
            return xmlDoc.InnerXml;
        }

        /// <summary>
        /// Check if the user right can perform the requested action
        /// If not, and if logIfPossible is True and if the current user
        /// is a default user, then a logon windows is display.
        /// </summary>
        /// <param name="nFeature"></param>
        /// <param name="nAction"></param>
        /// <param name="strDeniedError"></param>
        /// <param name="logIfPossible"></param>
        /// <returns></returns>
        public bool CheckUserGroupRights(int nFeature, int nAction, ref string strDeniedError, bool logIfPossible)
        {
            if (!LMS.Data.SiteData.Instance().ExistRight(nFeature, nAction))
                return false;

            strDeniedError = string.Empty;

            return LMS.Data.SiteData.Instance().CheckUserGroupRights(nFeature, nAction, ref strDeniedError);
        }
    }
}

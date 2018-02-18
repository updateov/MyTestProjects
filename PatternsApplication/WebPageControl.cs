using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Diagnostics;
using System.Xml;
using System.Security.Cryptography;
using System.IO;
using NavigationData;

namespace PatternsApplication
{
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public partial class WebPageControl : UserControl
    {
        private String m_currentParameters;

        public WebPageControl()
        {
            InitializeComponent();

            /// Go to the page !
            Navigate();
        }

        /// <summary>
        /// Navigate to the proper page
        /// </summary>
        private void Navigate()
        {
            Navigate(Navigation.BuildParameters());
        }

        /// <summary>
        /// Navigate to the proper page
        /// </summary>
        /// <param name="parameters"></param>
        private void Navigate(String parameters)
        {
            try
            {
                m_currentParameters = parameters;
                if (m_currentParameters.Equals(String.Empty))
                {
                    webBrowserTracingPatterns.Visible = false;
                    webBrowserTracingPatterns.Navigate("about:blank");
                    return;
                }

                if (!webBrowserTracingPatterns.Visible)
                    webBrowserTracingPatterns.Visible = true;

                // If there is a password configured for the page, then encrypt the parameters
                String pwd = Navigation.EncryptionPassword;
                if (!String.IsNullOrEmpty(pwd))
                    parameters = EncDec.RijndaelEncrypt(parameters, pwd, Navigation.s_IV, Navigation.s_Salt, Navigation.s_KeySize);

                webBrowserTracingPatterns.Navigate(Navigation.ApplicationURL, String.Empty, Encoding.UTF8.GetBytes(parameters), "Content-Type: application/xml; charset=utf-8");
            }
            catch (Exception e)
            {
                webBrowserTracingPatterns.Visible = false;
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NavigationData
{
    public static class Navigation
    {
        public static byte[] s_IV = { 2, 123, 89, 12, 99, 23, 23, 12, 32, 221, 93, 45, 32, 12, 33, 99, 12, 32, 199 };
        public static string s_Salt = "A Touch Of Salt";
        public static int s_KeySize = 256;

        /// <summary>
        /// The id of the page it's linked to
        /// </summary>
        private static int s_pageId;

        /// <summary>
        /// Indicate if the page is linked to a patient
        /// </summary>
        private static bool PatientSpecific
        {
            get
            {
                //switch (m_pageId)
                //{
                //    case 4000600: /* s_nWebScreen_Bottom_1_ID */
                //    case 4000601: /* s_nWebScreen_Bottom_2_ID */
                //    case 4000602: /* s_nWebScreen_Bottom_3_ID */
                //    case 4000603: /* s_nWebScreen_Bottom_4_ID */
                //    case 4000604: /* s_nWebScreen_Bottom_5_ID */
                //        return true;

                //    default:
                //        return false;
                //}
                return true;
            }
        }

        public static String EncryptionPassword
        {
            get
            {
                //switch (m_pageId)
                //{
                //    case 4000600: /* s_nWebScreen_Bottom_1_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenBottom1;
                //    case 4000601: /* s_nWebScreen_Bottom_2_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenBottom2;
                //    case 4000602: /* s_nWebScreen_Bottom_3_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenBottom3;
                //    case 4000603: /* s_nWebScreen_Bottom_4_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenBottom4;
                //    case 4000604: /* s_nWebScreen_Bottom_5_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenBottom5;

                //    case 4000500: /* s_nWebScreen_Left_1_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenLeft1;
                //    case 4000501: /* s_nWebScreen_Left_2_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenLeft2;
                //    case 4000502: /* s_nWebScreen_Left_3_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenLeft3;
                //    case 4000503: /* s_nWebScreen_Left_4_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenLeft4;
                //    case 4000504: /* s_nWebScreen_Left_5_ID */ return LMS.Data.SiteData.Instance().SiteConfig.KeyWEBScreenLeft5;

                //    default:
                //        Debug.Assert(false);
                //        return null;
                //}
                return "12345";
            }
        }

        /// <summary>
        /// Build the XML string that contains the parameters for the web page
        /// </summary>
        /// <returns></returns>
        public static String BuildParameters()
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement root = xmlDoc.CreateElement("parameters");
            xmlDoc.AppendChild(root);
            //root.SetAttribute("user_key", "666-9");
            root.SetAttribute("user_name", "User default");
            root.SetAttribute("user_id", "default");
            //root.SetAttribute("is_default_user", "True");
            String machineName = Environment.MachineName;
            //root.SetAttribute("workstation", machineName);
            //root.SetAttribute("domain", "E_AND_C");

            root.SetAttribute("can_modify", "True");
            //root.SetAttribute("can_print", "True");
            root.SetAttribute("visit_key", "666-20-1-1");

            return xmlDoc.InnerXml;
        }

        /// <summary>
        /// Return the URL configured for the given page
        /// </summary>
        public static Uri ApplicationURL
        {
            get
            {
                return new Uri("http://192.168.173.40:91/common/patterns.aspx");
            }
        }

    }
}

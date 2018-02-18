using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PatternsCRIClient.Screens
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
        private const string m_helpFile = @".\Help\PeriCALM CheckList User Guide.pdf";

        #region Properties

        public string AboutBoxManufacturedBy
        {
            get
            {
                return "Manufactured by";
            }
        }

        public string Aboutbox_intended_use
        {
            get
            {
                return @"PeriCALM® CheckList™ is intended for use as an adjunct to qualified clinical decision-making during antepartum or intrapartum obstetrical monitoring at ≥ 36 weeks gestation to obtain annotation of the FHR for baseline, accelerations and decelerations.

WARNING: Evaluation of FHR during labor and patient management decisions should not be based solely on PeriCALM® CheckList™ annotations.";
            }
        }

        public String AboutBoxUDITitle
        {
            get
            {
                return "UDI Number:";
            }
        }

        public String AboutBoxUDINumber
        {
            get
            {
                return "*+B087PATCL01021/$$7010201$*";
            }
        }

        public string Aboutbox_product_group
        {
            get
            {
                return "Product Information, Copyrights and Patents";
            }
        }

        public string Aboutbox_product_info
        {
            get
            {
                return "PeriCALM® CheckList™ - Version " + ProductVer + " Build " + FileVer;
            }
        }

        public String AboutBoxCopyright
        {
            get
            {
                return CopyrightHolder;
            }
        }

        public String AboutBoxStatementInstructions
        {
            get
            {
                return @"Please refer to the User Guide prior to the first use.
Rx only";
            }
        }

        public string Aboutbox_product_patent
        {
            get
            {
                return @"Various aspects of the PeriCALM® software suite are subject to issued and pending patents in several jurisdictions.
Issued patents include:

USA 6,907,284
USA 7,113,819
USA 6,423,016
European Patent 1,505,903
European Patent 1,289,416
Canada 2,311,029
Canada 2,384,516
Canada 2,379,733";
            }
        }

        public string AboutBoxSupportAndInquiries
        {
            get
            {
                return "Support and Inquiries";
            }
        }

        public string AboutBoxManufactured
        {
            get
            {
                return @"PeriGen Solutions Ltd.
4 Negev St., Airport City,
POB 176 Ben-Gurion Airport
7010000 Israel";
            }
        }

        public String AboutBoxSupportPhone
        {
            get
            {
                return "Support phone:";
            }
        }


        public String AboutBoxSupportPhoneNumber
        {
            get
            {
                return "(+1)-866-321-6788\n(+1)-888-866-5339";
            }
        }

        public String AboutBoxSupportMail
        {
            get
            {
                return "Support mail:";
            }
        }

        public String AboutBoxSupportMailAddress
        {
            get
            {
                return "support@perigen.com";
            }
        }

        public String AboutBoxContactPhone
        {
            get
            {
                return "Info:";
            }
        }


        public String AboutBoxContactPhoneNumber
        {
            get
            {
                return "(+1)-877-700-4755";
            }
        }

        public String AboutBoxContactFax
        {
            get
            {
                return "Fax:";
            }
        }


        public String AboutBoxContactFaxNumber
        {
            get
            {
                return "(+1)-609-395-6734";
            }
        }

        public String AboutBoxContactMail
        {
            get
            {
                return "Mail:";
            }
        }

        public String AboutBoxContactMailAddress
        {
            get
            {
                return "perigen@perigen.com";
            }
        }

        public String AboutBoxWebsite
        {
            get
            {
                return "Website:";
            }
        }

        public String AboutBoxWebsiteUrl
        {
            get
            {
                return "www.perigen.com";
            }
        }

        public static string ProductVer
        {
            get;
            set;
        }

        public static string FileVer
        {
            get;
            set;
        }

        public static string CopyrightHolder
        {
            get;
            set;
        }

        #endregion

        public AboutWindow()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            ProductVer = fileVersionInfo.ProductMajorPart.ToString("D2") + "." + fileVersionInfo.ProductMinorPart.ToString("D2") + "." + fileVersionInfo.ProductBuildPart.ToString("D2");
            FileVer = fileVersionInfo.Comments.ToString();
            CopyrightHolder = fileVersionInfo.LegalCopyright;

            InitializeComponent();

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            contentGrid.MouseLeftButtonDown += delegate { this.DragMove(); };
            canvasLogo.MouseLeftButtonDown += delegate { this.DragMove(); };
        }

        private void txtHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(m_helpFile))
            {
                Process.Start(m_helpFile);
            }
            else
            {
                MessageBox.Show("File not found.");
            }
        }

        private void txtAlgorithm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //this.Topmost = false;
            //this.Opacity = 0;
            m_gridFadeOutStoryBoard.Begin();

            AboutAlgorithmWindow about = new AboutAlgorithmWindow(true);
            about.ShowDialog();

            m_gridFadeInStoryBoard.Begin();
            //this.Topmost = true;
        }

        private void aboutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();
        }

        void FadeOutStoryBoard_Completed(object sender, EventArgs e)
        {
            m_gridFadeOutStoryBoard.Completed -= FadeOutStoryBoard_Completed;
            this.Close();
        }

        private void aboutWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                m_gridFadeOutStoryBoard.Completed += FadeOutStoryBoard_Completed;
                m_gridFadeOutStoryBoard.Begin();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Completed += FadeOutStoryBoard_Completed;
            m_gridFadeOutStoryBoard.Begin();
        }
    }
}

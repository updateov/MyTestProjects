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

namespace PeriGen.Patterns.WPFLibrary.Screens
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;
        private string m_helpFile = @".\Help\PeriWatch Cues User Guide.pdf";
        private const string m_patternsHelpFile = @"\Help\PeriCALM PATTERNS User Guide.pdf";
        private const string m_cuesHelpFile = @"\Help\PeriWatch Cues User Guide.pdf";
        private const string m_checklistHelpFile = @"\Help\PeriCALM CheckList User Guide.pdf";

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
                return CompanyName + " " + AppName + @" is intended for use as an adjunct to qualified clinical decision-making during antepartum or intrapartum obstetrical monitoring at ≥ 36 weeks gestation to obtain annotation of the FHR for baseline, accelerations and decelerations.

WARNING: Evaluation of FHR during labor and patient management decisions should not be based solely on " + CompanyName + " " + AppName + " annotations.";
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
                return UDINumber; //"*+B087PATT02081/$$70208005*"; //" * +B087PATCL01021/$$7010201$*";
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
                return CompanyName + " " + AppName + " - Version " + ProductVer + " Build " + FileVer;
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
                return @"Various aspects of the " + CompanyName + @" software suite are subject to issued and pending patents in several jurisdictions.
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
Sderot Nim 2,
PO Box 110,
Rishon LeTziyon, 7510002,
Israel";
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

        public string PluginURL
        {
            get;
            set;
        }

        public bool IsCheckListExists
        {
            get;
            set;
        }

        public bool IsPeriGen
        {
            get;
            set;
        }

        public string CompanyName
        {
            get;
            set;
        }

        public string AppName
        {
            get;
            set;
        }

        public string UDINumber
        {
            get;
            set;
        }
        #endregion

        public AboutWindow(string pluginURL, bool isCheckListExists, bool isPeriGen, string udi, bool isChecklistApp)
        {
            PluginURL = pluginURL;
            IsCheckListExists = isCheckListExists;
            IsPeriGen = isPeriGen;
            UDINumber = udi;

            CompanyName = isPeriGen ? "PeriCALM®" : "PeriWatch™";
            AppName = isPeriGen ? "Patterns™" : "Cues™";

            Assembly assembly = Assembly.GetExecutingAssembly();

            string assemblyPath = assembly.Location;
            var directory = System.IO.Path.GetDirectoryName(assemblyPath);

            String helpFile;
            if (isPeriGen)
            {
                helpFile = isChecklistApp ? m_checklistHelpFile : m_patternsHelpFile;
            }
            else
            {
                helpFile = m_cuesHelpFile;
            }
            m_helpFile = string.Format("{0}{1}", directory, helpFile);
            
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

            ProductVer = fileVersionInfo.ProductMajorPart.ToString("D2") + "." + fileVersionInfo.ProductMinorPart.ToString("D2") + "." + fileVersionInfo.ProductBuildPart.ToString("D2");
            FileVer = fileVersionInfo.Comments.ToString();
            CopyrightHolder = fileVersionInfo.LegalCopyright;

            InitializeComponent();

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("aboutBoxFadeIn");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("aboutBoxFadeOut");

            contentGrid.MouseLeftButtonDown += delegate { this.DragMove(); };

 
            canvasPeriWatchLogo.Visibility = Visibility.Visible;
            canvasPeriWatchLogo.MouseLeftButtonDown += delegate { this.DragMove(); };

            canvasPerigenLogo.Visibility = Visibility.Collapsed;
            

            if (IsCheckListExists == false)
            {
                txtAlgorithm.Visibility = Visibility.Collapsed;
            }
        }

        private void txtHelp_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(m_helpFile))
            {
                Process.Start(m_helpFile);
            }
            else
            { 
                //MessageBox.Show("File not found.");
                MessageBox.Show(string.Format("File {0} not found", m_helpFile));
            }
        }

        private void txtAlgorithm_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            AboutAlgorithmWindow about = new AboutAlgorithmWindow(true, PluginURL, IsPeriGen);
            about.ShowDialog();

            m_gridFadeInStoryBoard.Begin();
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

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Completed += FadeOutStoryBoard_Completed;
            m_gridFadeOutStoryBoard.Begin();
        }
    }
}

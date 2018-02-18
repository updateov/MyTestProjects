using CRIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for AboutAlgorithmWindow.xaml
    /// </summary>
    public partial class AboutAlgorithmWindow : Window
    {
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;

        public string MinimalBaselineVariability { get; set; }
        public string MinimalLateDecelConfidence { get; set; }
        public string MinimalLateDecelConfidenceValue { get; set; }
        public string MinimalAccelerationsAmount { get; set; }
        public string MinimalLateDecelAmount { get; set; }
        public string MinimalLargeAndLongDecelAmount { get; set; }
        public string MinimalLateAndLargeAndLongDecelAmount { get; set; }
        public string MinimalLateAndProlongedDecelAmount { get; set; }
        public string MinimalProlongedDecelHeight { get; set; }
        public string MinimalContractionsAmount { get; set; }
        public string MinimalLongContractionsAmount { get; set; }
        public string CRIStateQualificationWindowSize { get; set; }
        public string MinimalAmountOfDataInQualificationWindow { get; set; }

        public void FillData (AlgorithmParameters parameters)
        {
            if(parameters == null)
            {
                MinimalBaselineVariability = "?";
                MinimalLateDecelConfidence = "?";
                MinimalLateDecelConfidenceValue = "?";
                MinimalAccelerationsAmount = "?";
                MinimalLateDecelAmount = "?";
                MinimalLargeAndLongDecelAmount = "?";
                MinimalLateAndLargeAndLongDecelAmount = "?";
                MinimalLateAndProlongedDecelAmount = "?";
                MinimalProlongedDecelHeight = "?";
                MinimalContractionsAmount = "?";
                MinimalLongContractionsAmount = "?";
                CRIStateQualificationWindowSize = "?";
                MinimalAmountOfDataInQualificationWindow = "?";
            }
            else
            {
                MinimalBaselineVariability = parameters.MinimalBaselineVariability.ToString("F1");
                MinimalLateDecelConfidence = (parameters.MinimalLateDecelConfidence * 100).ToString("F1");
                MinimalAccelerationsAmount = parameters.MinimalAccelerationsAmount.ToString("F1"); 
                MinimalLateDecelAmount = parameters.MinimalLateDecelAmount.ToString();
                MinimalLargeAndLongDecelAmount = parameters.MinimalLargeAndLongDecelAmount.ToString();
                MinimalLateAndLargeAndLongDecelAmount = parameters.MinimalLateAndLargeAndLongDecelAmount.ToString();
                MinimalLateAndProlongedDecelAmount = parameters.MinimalLateAndProlongedDecelAmount.ToString();
                MinimalProlongedDecelHeight = parameters.MinimalProlongedDecelHeight.ToString();
                MinimalContractionsAmount = parameters.MinimalContractionsAmount.ToString();
                MinimalLongContractionsAmount = parameters.MinimalLongContractionsAmount.ToString();
                CRIStateQualificationWindowSize = parameters.CRIStateQualificationWindowSize.ToString();
                MinimalAmountOfDataInQualificationWindow = parameters.MinimalAmountOfDataInQualificationWindow.ToString();

                MinimalLateDecelConfidenceValue = ((parameters.MinimalLateDecelConfidence * 100) / 10 - 5).ToString("F1");
            }
        }
        
        public AboutAlgorithmWindow(bool openOnCenter)
        {
            AlgorithmParameters parameters = App.ClientManager.GetAlgorithmParameters();
            FillData(parameters);

            InitializeComponent();

            if(openOnCenter)
            {
                this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            }
            else
            {
                SetWindowToBottomRightOfScreen();
            }

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            m_gridFadeOutStoryBoard.Completed += FadeOutStoryBoard_Completed;

            contentGrid.MouseLeftButtonDown += delegate { this.DragMove(); };
            canvasTitle.MouseLeftButtonDown += delegate { this.DragMove(); };
        }

        void FadeOutStoryBoard_Completed(object sender, EventArgs e)
        {
            m_gridFadeOutStoryBoard.Completed -= FadeOutStoryBoard_Completed;
            //this.DialogResult = true;
            this.Close();
        }

        private void SetWindowToBottomRightOfScreen()
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height;
        }

        private void aboutAlgorithmWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();
        }

        private void aboutAlgorithmWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            //m_gridFadeOutStoryBoard.Begin();
        }

        private void btnClose_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.MouseLeave -= aboutAlgorithmWindow_MouseLeave;
            m_gridFadeOutStoryBoard.Begin();
        }

        private void aboutAlgorithmWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                m_gridFadeOutStoryBoard.Completed += FadeOutStoryBoard_Completed;
                m_gridFadeOutStoryBoard.Begin();
            }
        }
    }
}

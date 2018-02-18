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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Perigen.Patterns.NnetControls.DataEntities;
using System.Windows.Media.Animation;

namespace Perigen.Patterns.NnetControls.Screens
{
    /// <summary>
    /// Interaction logic for ExportToEMRWindow.xaml
    /// </summary>
    public partial class ExportToEMRWindow : Window
    {
        private DateTime m_from;
        private DateTime m_to;
        private Storyboard m_gridFadeInStoryBoard;
        private Storyboard m_gridFadeOutStoryBoard;

        public ExportInterval Interval { get; set; }

        #region Dependency Property


        public double Bottom
        {
            get { return (double)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }
        public static readonly DependencyProperty BottomProperty =
            DependencyProperty.Register("Bottom", typeof(double), typeof(ExportToEMRWindow), new UIPropertyMetadata(0.0));

        
        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(ExportToEMRWindow), new UIPropertyMetadata(false));

        #region Params Visibility

        public Visibility MVUVisibility
        {
            get { return (Visibility)GetValue(MVUVisibilityProperty); }
            set { SetValue(MVUVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MVUVisibilityProperty =
            DependencyProperty.Register("MVUVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility EarlyDecelerationsVisibility
        {
            get { return (Visibility)GetValue(EarlyDecelerationsVisibilityProperty); }
            set { SetValue(EarlyDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty EarlyDecelerationsVisibilityProperty =
            DependencyProperty.Register("EarlyDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility VariableDecelerationsVisibility
        {
            get { return (Visibility)GetValue(VariableDecelerationsVisibilityProperty); }
            set { SetValue(VariableDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty VariableDecelerationsVisibilityProperty =
            DependencyProperty.Register("VariableDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility LateDecelerationsVisibility
        {
            get { return (Visibility)GetValue(LateDecelerationsVisibilityProperty); }
            set { SetValue(LateDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty LateDecelerationsVisibilityProperty =
            DependencyProperty.Register("LateDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility ProlongedDecelerationsVisibility
        {
            get { return (Visibility)GetValue(ProlongedDecelerationsVisibilityProperty); }
            set { SetValue(ProlongedDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ProlongedDecelerationsVisibilityProperty =
            DependencyProperty.Register("ProlongedDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility OtherDecelerationsVisibility
        {
            get { return (Visibility)GetValue(OtherDecelerationsVisibilityProperty); }
            set { SetValue(OtherDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty OtherDecelerationsVisibilityProperty =
            DependencyProperty.Register("OtherDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility MeanContractionIntervalVisibility
        {
            get { return (Visibility)GetValue(MeanContractionIntervalVisibilityProperty); }
            set { SetValue(MeanContractionIntervalVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MeanContractionIntervalVisibilityProperty =
            DependencyProperty.Register("MeanContractionIntervalVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility NumOfContractionsVisibility
        {
            get { return (Visibility)GetValue(NumOfContractionsVisibilityProperty); }
            set { SetValue(NumOfContractionsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NumOfContractionsVisibilityProperty =
            DependencyProperty.Register("NumOfContractionsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility NumOfLongContractionsVisibility
        {
            get { return (Visibility)GetValue(NumOfLongContractionsVisibilityProperty); }
            set { SetValue(NumOfLongContractionsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NumOfLongContractionsVisibilityProperty =
            DependencyProperty.Register("NumOfLongContractionsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility MeanBaselineVisibility
        {
            get { return (Visibility)GetValue(MeanBaselineVisibilityProperty); }
            set { SetValue(MeanBaselineVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MeanBaselineVisibilityProperty =
            DependencyProperty.Register("MeanBaselineVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility MeanBaselineVariabilityVisibility
        {
            get { return (Visibility)GetValue(MeanBaselineVariabilityVisibilityProperty); }
            set { SetValue(MeanBaselineVariabilityVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MeanBaselineVariabilityVisibilityProperty =
            DependencyProperty.Register("MeanBaselineVariabilityVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility NumOfAccelerationsVisibility
        {
            get { return (Visibility)GetValue(NumOfAccelerationsVisibilityProperty); }
            set { SetValue(NumOfAccelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NumOfAccelerationsVisibilityProperty =
            DependencyProperty.Register("NumOfAccelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility NumOfDecelerationsVisibility
        {
            get { return (Visibility)GetValue(NumOfDecelerationsVisibilityProperty); }
            set { SetValue(NumOfDecelerationsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty NumOfDecelerationsVisibilityProperty =
            DependencyProperty.Register("NumOfDecelerationsVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));

        
        public Visibility CategoryVisibility
        {
            get { return (Visibility)GetValue(CategoryVisibilityProperty); }
            set { SetValue(CategoryVisibilityProperty, value); }
        }
        public static readonly DependencyProperty CategoryVisibilityProperty =
            DependencyProperty.Register("CategoryVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));


        public Visibility CommentVisibility
        {
            get { return (Visibility)GetValue(CommentVisibilityProperty); }
            set { SetValue(CommentVisibilityProperty, value); }
        }
        public static readonly DependencyProperty CommentVisibilityProperty =
            DependencyProperty.Register("CommentVisibility", typeof(Visibility), typeof(ExportToEMRWindow), new UIPropertyMetadata(Visibility.Collapsed));

        #endregion

        #region Params Values

        public double? MeanContractionInterval 
        {
            get { return (double?)GetValue(MeanContractionIntervalProperty); }
            set { SetValue(MeanContractionIntervalProperty, value); }
        }
        public static readonly DependencyProperty MeanContractionIntervalProperty =
            DependencyProperty.Register("MeanContractionInterval ", typeof(double?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));

 
        public int? NumOfContractions
        {
            get { return (int?)GetValue(NumOfContractionsProperty); }
            set { SetValue(NumOfContractionsProperty, value); }
        }
        public static readonly DependencyProperty NumOfContractionsProperty =
            DependencyProperty.Register("NumOfContractions", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfLongContractions
        {
            get { return (int?)GetValue(NumOfLongContractionsProperty); }
            set { SetValue(NumOfLongContractionsProperty, value); }
        }
        public static readonly DependencyProperty NumOfLongContractionsProperty =
            DependencyProperty.Register("NumOfLongContractions", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? MontevideoUnits
        {
            get { return (int?)GetValue(MontevideoUnitsProperty); }
            set { SetValue(MontevideoUnitsProperty, value); }
        }
        public static readonly DependencyProperty MontevideoUnitsProperty =
            DependencyProperty.Register("MontevideoUnits", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? MeanBaseline
        {
            get { return (int?)GetValue(MeanBaselineProperty); }
            set { SetValue(MeanBaselineProperty, value); }
        }
        public static readonly DependencyProperty MeanBaselineProperty =
            DependencyProperty.Register("MeanBaseline", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public string MeanBaselineVariability
        {
            get { return (string)GetValue(MeanBaselineVariabilityProperty); }
            set { SetValue(MeanBaselineVariabilityProperty, value); }
        }
        public static readonly DependencyProperty MeanBaselineVariabilityProperty =
            DependencyProperty.Register("MeanBaselineVariability", typeof(string), typeof(ExportToEMRWindow), new UIPropertyMetadata("0.0"));


        public int? NumOfAccelerations
        {
            get { return (int?)GetValue(NumOfAccelerationsProperty); }
            set { SetValue(NumOfAccelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfAccelerationsProperty =
            DependencyProperty.Register("NumOfAccelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfDecelerations
        {
            get { return (int?)GetValue(NumOfDecelerationsProperty); }
            set { SetValue(NumOfDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfDecelerationsProperty =
            DependencyProperty.Register("NumOfDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfEarlyDecelerations
        {
            get { return (int?)GetValue(NumOfEarlyDecelerationsProperty); }
            set { SetValue(NumOfEarlyDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfEarlyDecelerationsProperty =
            DependencyProperty.Register("NumOfEarlyDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfVariableDecelerations
        {
            get { return (int?)GetValue(NumOfVariableDecelerationsProperty); }
            set { SetValue(NumOfVariableDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfVariableDecelerationsProperty =
            DependencyProperty.Register("NumOfVariableDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfLateDecelerations
        {
            get { return (int?)GetValue(NumOfLateDecelerationsProperty); }
            set { SetValue(NumOfLateDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfLateDecelerationsProperty =
            DependencyProperty.Register("NumOfLateDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfProlongedDecelerations
        {
            get { return (int?)GetValue(NumOfProlongedDecelerationsProperty); }
            set { SetValue(NumOfProlongedDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfProlongedDecelerationsProperty =
            DependencyProperty.Register("NumOfProlongedDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public int? NumOfOtherDecelerations
        {
            get { return (int?)GetValue(NumOfOtherDecelerationsProperty); }
            set { SetValue(NumOfOtherDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfOtherDecelerationsProperty =
            DependencyProperty.Register("NumOfOtherDecelerations", typeof(int?), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public List<string> Categories
        {
            get { return (List<string>)GetValue(CategoriesProperty); }
            set { SetValue(CategoriesProperty, value); }
        }
        public static readonly DependencyProperty CategoriesProperty =
            DependencyProperty.Register("Categories", typeof(List<string>), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(string), typeof(ExportToEMRWindow), new UIPropertyMetadata(String.Empty));


        public string CategoryVal
        {
            get { return (string)GetValue(CategoryValProperty); }
            set { SetValue(CategoryValProperty, value); }
        }
        public static readonly DependencyProperty CategoryValProperty =
            DependencyProperty.Register("CategoryVal", typeof(string), typeof(ExportToEMRWindow), new UIPropertyMetadata(String.Empty));

                
        #endregion

        #endregion

        public ExportToEMRWindow(DateTime from, DateTime to, ExportInterval interval)
        {
            m_from = from;
            m_to = to;
            Interval = interval;

            SetParamsValues();

            InitializeComponent();
        
            txtTimeRange.Text = String.Format("{0:HH:mm} - {1:HH:mm}", m_from, m_to);
            mainGrid.MouseLeftButtonDown += delegate { this.DragMove(); };

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            SetControlsVisibility();
        }

        private void SetParamsValues()
        {
            if (PatternsUIManager.Instance.IsMontevideoVisible == false)
            {
                BaseExportEntity mUnits = Interval.Concepts.FirstOrDefault(t => t.Name == "MeanMontevideoUnits");
                if (mUnits != null)
                {
                    Interval.Concepts.Remove(mUnits);
                }
            }

            foreach (BaseExportEntity entity in Interval.Concepts)
            {
                switch (entity.Name)
                {
                    case "MeanContractionInterval":
                        MeanContractionInterval = ((ExportDoubleEntity)entity).Value.HasValue ? ((ExportDoubleEntity)entity).Value : null;
                        break;

                    case "NumOfContractions":
                        NumOfContractions = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfLongContractions":
                        NumOfLongContractions = ((ExportIntEntity)entity).Value;
                        break;

                    case "MeanMontevideoUnits":
                        MontevideoUnits = ((ExportIntEntity)entity).Value;
                        break;

                    case "MeanBaseline":
                        MeanBaseline = ((ExportIntEntity)entity).Value;
                        break;

                    case "MeanBaselineVariability":
                        MeanBaselineVariability = ((ExportDoubleEntity)entity).Value.ToString();
                        break;

                    case "NumOfAccelerations":                            
                        NumOfAccelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfDecelerations":
                        NumOfDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfEarlyDecelerations":
                        NumOfEarlyDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfVariableDecelerations":
                        NumOfVariableDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfLateDecelerations":
                        NumOfLateDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfProlongedDecelerations":
                        NumOfProlongedDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "NumOfOtherDecelerations":
                        NumOfOtherDecelerations = ((ExportIntEntity)entity).Value;
                        break;

                    case "Comment":
                        Comment = ((ExportStringEntity)entity).Value;
                        break;

                    case "Category":
                        {
                            CategoryVal = ((ExportComboEntity)entity).Value;

                            Categories = new List<string>();
                            Categories.AddRange(((ExportComboEntity)entity).Categories);                           
                        }
                        break;
                }
            }

            if ((MeanBaseline == null || MeanBaseline == -1) && (MeanBaselineVariability == null || MeanBaselineVariability == "-1"))
            {
                NumOfAccelerations = null;
            }
        }

        private void SetControlsVisibility()
        {       
            foreach (BaseExportEntity entity in Interval.Concepts)
            {
                switch (entity.Name)
                {
                    case "MeanContractionInterval":
                        ctrlMeanContractionInterval.MinValue = ((ExportDoubleEntity)entity).Min;
                        ctrlMeanContractionInterval.MaxValue = ((ExportDoubleEntity)entity).Max;
                        ctrlMeanContractionInterval.OriginValue = ((ExportDoubleEntity)entity).OriginValue;
                        MeanContractionIntervalVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfContractions":
                        ctrlNumOfContractions.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfContractions.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfContractions.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        NumOfContractionsVisibility = System.Windows.Visibility.Visible;
                       break;

                    case "NumOfLongContractions":
                        ctrlNumOfLongContractions.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfLongContractions.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfLongContractions.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        NumOfLongContractionsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "MeanMontevideoUnits":
                        ctrlMeanMontevideoUnits.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlMeanMontevideoUnits.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlMeanMontevideoUnits.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        MVUVisibility = PatternsUIManager.Instance.IsMontevideoVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        break;

                    case "MeanBaseline":
                        ctrlMeanBaseline.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlMeanBaseline.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlMeanBaseline.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        MeanBaselineVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "MeanBaselineVariability":
                        ctrlMeanBaselineVariability.MinValue = ((ExportDoubleEntity)entity).Min;
                        ctrlMeanBaselineVariability.MaxValue = ((ExportDoubleEntity)entity).Max;
                        ctrlMeanBaselineVariability.OriginValue = ((ExportDoubleEntity)entity).OriginValue;
                        MeanBaselineVariabilityVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfAccelerations":
                        ctrlNumOfAccelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfAccelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfAccelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        NumOfAccelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfDecelerations":
                        ctrlNumOfDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        NumOfDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfEarlyDecelerations":
                        ctrlNumOfEarlyDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfEarlyDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfEarlyDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        EarlyDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfVariableDecelerations":
                        ctrlNumOfVariableDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfVariableDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfVariableDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        VariableDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfLateDecelerations":
                        ctrlNumOfLateDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfLateDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfLateDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        LateDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfProlongedDecelerations":
                        ctrlNumOfProlongedDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfProlongedDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfProlongedDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        ProlongedDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "NumOfOtherDecelerations":
                        ctrlNumOfOtherDecelerations.MinValue = ((ExportIntEntity)entity).Min;
                        ctrlNumOfOtherDecelerations.MaxValue = ((ExportIntEntity)entity).Max;
                        ctrlNumOfOtherDecelerations.OriginValue = ((ExportIntEntity)entity).OriginValue;
                        OtherDecelerationsVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "Comment":
                        CommentVisibility = System.Windows.Visibility.Visible;
                        break;

                    case "Category":
                        CategoryVisibility = System.Windows.Visibility.Visible;
                        break;
                }
            }      
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.IsCanceled = true;
            this.DialogResult = false;
            this.Close();
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            UpdateInterval();
            this.DialogResult = true;
        }

        private void UpdateInterval()
        {
            foreach (BaseExportEntity entity in Interval.Concepts)
            {
                switch (entity.Name)
                {
                    case "MeanContractionInterval":
                        ((ExportDoubleEntity)entity).Value = MeanContractionInterval.HasValue ? MeanContractionInterval : null;
                        break;

                    case "NumOfContractions":
                        ((ExportIntEntity)entity).Value = NumOfContractions.HasValue ? NumOfContractions : null;
                        break;

                    case "NumOfLongContractions":
                        ((ExportIntEntity)entity).Value = NumOfLongContractions.HasValue ? NumOfLongContractions : null;
                        break;

                    case "MeanMontevideoUnits":
                        ((ExportIntEntity)entity).Value = MontevideoUnits.HasValue ? MontevideoUnits : null;
                        break;

                    case "MeanBaseline":
                        ((ExportIntEntity)entity).Value = MeanBaseline.HasValue ? MeanBaseline : null;
                        break;

                    case "MeanBaselineVariability":
                        if (String.IsNullOrEmpty(MeanBaselineVariability) == false)
                            ((ExportDoubleEntity)entity).Value = Double.Parse(MeanBaselineVariability);
                        else
                            ((ExportDoubleEntity)entity).Value = null;
                        break;

                    case "NumOfAccelerations":
                        ((ExportIntEntity)entity).Value = NumOfAccelerations.HasValue ? NumOfAccelerations : null;
                        break;

                    case "NumOfDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfDecelerations.HasValue ? NumOfDecelerations : null;
                        break;

                    case "NumOfEarlyDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfEarlyDecelerations.HasValue ? NumOfEarlyDecelerations : null;
                        break;

                    case "NumOfVariableDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfVariableDecelerations.HasValue ? NumOfVariableDecelerations : null;
                        break;

                    case "NumOfLateDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfLateDecelerations.HasValue ? NumOfLateDecelerations : null;
                        break;

                    case "NumOfProlongedDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfProlongedDecelerations.HasValue ? NumOfProlongedDecelerations : null;
                        break;

                    case "NumOfOtherDecelerations":
                        ((ExportIntEntity)entity).Value = NumOfOtherDecelerations.HasValue ? NumOfOtherDecelerations : null;
                        break;

                    case "Comment":
                        ((ExportStringEntity)entity).Value = Comment;
                        break;

                    case "Category":
                        ((ExportComboEntity)entity).Value = CategoryVal;
                        break;
                }
            }      
        }

        private void CloseButton_Click(object sender, MouseButtonEventArgs e)
        {
            m_gridFadeOutStoryBoard.Begin();

            this.IsCanceled = true;
            this.DialogResult = false;
            this.Close();
        } 

        private void exportToEMRWindow_Loaded(object sender, RoutedEventArgs e)
        {
            m_gridFadeInStoryBoard.Begin();       

            if (this.IsVisible == true)
            {
                this.Top = this.Bottom - this.ActualHeight;
            }
        }
    }
}

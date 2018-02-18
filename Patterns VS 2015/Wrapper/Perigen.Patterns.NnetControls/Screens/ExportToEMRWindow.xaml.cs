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
using Perigen.Patterns.NnetControls.Controls;
using Perigen.Patterns.NnetControls.Converters;

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

  
        #region Params Values

        public ExportDoubleEntity MeanContractionInterval 
        {
            get { return (ExportDoubleEntity)GetValue(MeanContractionIntervalProperty); }
            set { SetValue(MeanContractionIntervalProperty, value); }
        }
        public static readonly DependencyProperty MeanContractionIntervalProperty =
            DependencyProperty.Register("MeanContractionInterval ", typeof(ExportDoubleEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));

 
        public ExportIntEntity NumOfContractions
        {
            get { return (ExportIntEntity)GetValue(NumOfContractionsProperty); }
            set { SetValue(NumOfContractionsProperty, value); }
        }
        public static readonly DependencyProperty NumOfContractionsProperty =
            DependencyProperty.Register("NumOfContractions", typeof(ExportIntEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportIntEntity NumOfLongContractions
        {
            get { return (ExportIntEntity)GetValue(NumOfLongContractionsProperty); }
            set { SetValue(NumOfLongContractionsProperty, value); }
        }
        public static readonly DependencyProperty NumOfLongContractionsProperty =
            DependencyProperty.Register("NumOfLongContractions", typeof(ExportIntEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportIntEntity MontevideoUnits
        {
            get { return (ExportIntEntity)GetValue(MontevideoUnitsProperty); }
            set { SetValue(MontevideoUnitsProperty, value); }
        }
        public static readonly DependencyProperty MontevideoUnitsProperty =
            DependencyProperty.Register("MontevideoUnits", typeof(ExportIntEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportIntEntity BaselineFHR
        {
            get { return (ExportIntEntity)GetValue(BaselineFHRProperty); }
            set { SetValue(BaselineFHRProperty, value); }
        }
        public static readonly DependencyProperty BaselineFHRProperty =
            DependencyProperty.Register("BaselineFHR", typeof(ExportIntEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportComboEntity BaselineClass
        {
            get { return (ExportComboEntity)GetValue(BaselineClassProperty); }
            set { SetValue(BaselineClassProperty, value); }
        }
        public static readonly DependencyProperty BaselineClassProperty =
            DependencyProperty.Register("BaselineClass", typeof(ExportComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportComboEntity FHRRhythm
        {
            get { return (ExportComboEntity)GetValue(FHRRhythmProperty); }
            set { SetValue(FHRRhythmProperty, value); }
        }
        public static readonly DependencyProperty FHRRhythmProperty =
            DependencyProperty.Register("FHRRhythm", typeof(ExportComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportCalculatedComboEntity Variability
        {
            get { return (ExportCalculatedComboEntity)GetValue(VariabilityProperty); }
            set { SetValue(VariabilityProperty, value); }
        }
        public static readonly DependencyProperty VariabilityProperty =
            DependencyProperty.Register("Variability", typeof(ExportCalculatedComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportCalculatedComboEntity Accels
        {
            get { return (ExportCalculatedComboEntity)GetValue(AccelsProperty); }
            set { SetValue(AccelsProperty, value); }
        }
        public static readonly DependencyProperty AccelsProperty =
            DependencyProperty.Register("Accels", typeof(ExportCalculatedComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportCalculatedCheckboxGroupEntity Decels
        {
            get { return (ExportCalculatedCheckboxGroupEntity)GetValue(DecelsProperty); }
            set { SetValue(DecelsProperty, value); }
        }
        public static readonly DependencyProperty DecelsProperty =
            DependencyProperty.Register("Decels", typeof(ExportCalculatedCheckboxGroupEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NoneDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NoneDecelerationsProperty); }
            set { SetValue(NoneDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NoneDecelerationsProperty =
            DependencyProperty.Register("NoneDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NumOfEarlyDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NumOfEarlyDecelerationsProperty); }
            set { SetValue(NumOfEarlyDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfEarlyDecelerationsProperty =
            DependencyProperty.Register("NumOfEarlyDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NumOfVariableDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NumOfVariableDecelerationsProperty); }
            set { SetValue(NumOfVariableDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfVariableDecelerationsProperty =
            DependencyProperty.Register("NumOfVariableDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NumOfLateDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NumOfLateDecelerationsProperty); }
            set { SetValue(NumOfLateDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfLateDecelerationsProperty =
            DependencyProperty.Register("NumOfLateDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NumOfProlongedDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NumOfProlongedDecelerationsProperty); }
            set { SetValue(NumOfProlongedDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfProlongedDecelerationsProperty =
            DependencyProperty.Register("NumOfProlongedDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public CalculatedCheckboxGroupConceptItem NumOfOtherDecelerations
        {
            get { return (CalculatedCheckboxGroupConceptItem)GetValue(NumOfOtherDecelerationsProperty); }
            set { SetValue(NumOfOtherDecelerationsProperty, value); }
        }
        public static readonly DependencyProperty NumOfOtherDecelerationsProperty =
            DependencyProperty.Register("NumOfOtherDecelerations", typeof(CalculatedCheckboxGroupConceptItem), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportComboEntity Recurrence
        {
            get { return (ExportComboEntity)GetValue(RecurrenceProperty); }
            set { SetValue(RecurrenceProperty, value); }
        }
        public static readonly DependencyProperty RecurrenceProperty =
            DependencyProperty.Register("Recurrence", typeof(ExportComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportComboEntity Categories
        {
            get { return (ExportComboEntity)GetValue(CategoriesProperty); }
            set { SetValue(CategoriesProperty, value); }
        }
        public static readonly DependencyProperty CategoriesProperty =
            DependencyProperty.Register("Categories", typeof(ExportComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportComboEntity Checklist
        {
            get { return (ExportComboEntity)GetValue(ChecklistProperty); }
            set { SetValue(ChecklistProperty, value); }
        }
        public static readonly DependencyProperty ChecklistProperty =
            DependencyProperty.Register("Checklist", typeof(ExportComboEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));


        public ExportStringEntity Comment
        {
            get { return (ExportStringEntity)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(ExportStringEntity), typeof(ExportToEMRWindow), new UIPropertyMetadata(null));

                
        #endregion

        #endregion

        public ExportToEMRWindow(DateTime from, DateTime to, ExportInterval interval)
        {
            m_from = from;
            m_to = to;
            Interval = interval;

            NumberToResolutionConverter conv = new NumberToResolutionConverter();
            Width = Double.Parse(conv.Convert(null, null, 370, null).ToString());

            SetParamsValues();

            InitializeComponent();

            txtTimeRange.Text = String.Format("{0:HH:mm} - {1:HH:mm}", m_from, m_to);
            mainGrid.MouseLeftButtonDown += delegate { this.DragMove(); };

            m_gridFadeInStoryBoard = (Storyboard)this.TryFindResource("gridFadeInMessage");
            m_gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutMessage");

            BindControls();
        }

        private void BindControls()
        {
            /*** Baseline Class ***/
            if (BaselineClass != null)
            {
                ctrlMeanBaselineVariability.ItemsSource = BaselineClass.Items;
                ctrlMeanBaselineVariability.SelectedItem = BaselineClass.Value;
                BaselineClass.OriginalValue = ctrlMeanBaselineVariability.SelectedItem;
            }

            /*** FHR Rhythm ***/
            if (FHRRhythm != null)
            {
                ctrlFHRRhythm.ItemsSource = FHRRhythm.Items;
                ctrlFHRRhythm.SelectedItem = FHRRhythm.Value;
                FHRRhythm.OriginalValue = ctrlFHRRhythm.SelectedItem;
            }

            /*** Variability ***/
            if (Variability != null)
            {
                List<string> variabilityItems = new List<string>();
                double variabilityValue = -10; // impossible value
                string variabilitySelectedValue = String.Empty;

                if (Variability.Value != null)
                {
                    Double.TryParse(Variability.Value, out variabilityValue);
                }

                foreach (CalculatedComboConceptItem item in Variability.Items)
                {
                    variabilityItems.Add(item.Value);

                    if (item.Min <= variabilityValue && item.Max >= variabilityValue)
                    {
                        variabilitySelectedValue = item.Value;
                    }
                }

                ctrlVariability.ItemsSource = variabilityItems;
                ctrlVariability.SelectedItem = variabilitySelectedValue;
                Variability.OriginalValue = ctrlVariability.SelectedItem.ToString();
            }

            /*** Accels ***/
            if (Accels != null)
            {
                List<string> accelsItems = new List<string>();
                double accelsValue = -10; // impossible value
                string accelsSelectedValue = String.Empty;

                if (Accels.Value != null)
                {
                    Double.TryParse(Accels.Value, out accelsValue);
                }

                foreach (CalculatedComboConceptItem item in Accels.Items)
                {
                    accelsItems.Add(item.Value);

                    if (item.Min <= accelsValue && item.Max >= accelsValue)
                    {
                        accelsSelectedValue = item.Value;
                    }
                }

                ctrlAccels.ItemsSource = accelsItems;
                ctrlAccels.SelectedItem = accelsSelectedValue;
                Accels.OriginalValue = ctrlAccels.SelectedItem.ToString();
            }
         
            /*** Recurrence ***/
            if (Recurrence != null)
            {
                ctrlRecurrence.ItemsSource = Recurrence.Items;
                ctrlRecurrence.SelectedItem = Recurrence.Value;
                Recurrence.OriginalValue = ctrlRecurrence.SelectedItem;
            }

            /*** Category ***/
            if (Categories != null)
            {
                ctrlCategory.ItemsSource = Categories.Items;
                ctrlCategory.SelectedItem = Categories.Value;
                Categories.OriginalValue = ctrlCategory.SelectedItem;
            }

            /*** Checklist ***/
            if (Checklist != null)
            {
                ctrlChecklist.ItemsSource = Checklist.Items;
                ctrlChecklist.SelectedItem = Checklist.Value;
                Checklist.OriginalValue = ctrlChecklist.SelectedItem;
            }
        }

        private void SetParamsValues()
        {
            if (PatternsUIManager.Instance.IsMontevideoVisible == false)
            {
                BaseExportEntity mUnits = Interval.Concepts.FirstOrDefault(t => t.Id == -101395);
                if (mUnits != null)
                {
                    Interval.Concepts.Remove(mUnits);
                }
            }

            foreach (BaseExportEntity entity in Interval.Concepts)
            {
                switch (entity.Id)
                {
                    case -102101:
                        MeanContractionInterval = (ExportDoubleEntity)entity;
                        break;

                    case -102102:
                        NumOfContractions = (ExportIntEntity)entity;
                        break;

                    case -102103:
                        NumOfLongContractions = (ExportIntEntity)entity;
                        break;

                    case -101395:
                        MontevideoUnits = (ExportIntEntity)entity;
                        break;

                    case -101385: //"Baseline FHR"
                        BaselineFHR = (ExportIntEntity)entity;
                        break;

                    case -102122: //Baseline Class
                        BaselineClass = (ExportComboEntity)entity;
                        break;

                    case -102123: //FHR Rhythm
                        FHRRhythm = (ExportComboEntity)entity;
                        break;

                    case -101420: //MeanBaselineVariability
                        Variability = (ExportCalculatedComboEntity)entity;
                        break;

                    case -101417: // "NumOfAccelerations"                          
                        Accels = (ExportCalculatedComboEntity)entity;
                        break;

                    case -101418: //"NumOfDecelerations"
                        {
                            Decels = (ExportCalculatedCheckboxGroupEntity)entity;
                            int decelsCount = 0;
                            string originValue = String.Empty;

                            foreach (CalculatedCheckboxGroupConceptItem item in Decels.Items)
                            {
                                switch (item.Id)
                                {
                                    case 9900021:
                                        NoneDecelerations = item;
                                        break;

                                    case 9901382:
                                        NumOfEarlyDecelerations = item;
                                        break;

                                    case 9900798:
                                        NumOfVariableDecelerations = item;
                                        break;

                                    case 9901383:
                                        NumOfLateDecelerations = item;
                                        break;

                                    case 9901381:
                                        NumOfProlongedDecelerations = item;
                                        break;

                                    case -100451:
                                        NumOfOtherDecelerations = item;
                                        break;
                                }

                                if (String.IsNullOrEmpty(item.CalculatedValue) == false)
                                {
                                    int calcVal = Int32.Parse(item.CalculatedValue);
                                    decelsCount += calcVal;

                                    if (calcVal > 0)
                                    {
                                        originValue += item.Value + ";";
                                    }
                                }
                            }

                            if (decelsCount == 0)
                            {
                                // None CheckBox should be checked
                                NoneDecelerations.CalculatedValue = "1";
                                Decels.OriginalValue = NoneDecelerations.Value;
                            }
                            else
                            {
                                Decels.OriginalValue = originValue.TrimEnd(';');
                            }
                        }
                        break;

                    case -102124:
                        Recurrence = (ExportComboEntity)entity;
                        break;           
                    
                    case -102013: //"Category"
                        Categories = (ExportComboEntity)entity;
                        break;

                    case -102125: //Checklist
                        Checklist = (ExportComboEntity)entity;
                        break;

                    case -102114: //Comment
                        Comment = (ExportStringEntity)entity;
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
                switch (entity.Id)
                {
                    case -102101:
                        ((ExportDoubleEntity)entity).Value = MeanContractionInterval.Value;
                        break;

                    case -102102:
                        ((ExportIntEntity)entity).Value = NumOfContractions.Value;
                        break;

                    case -102103:
                        ((ExportIntEntity)entity).Value = NumOfLongContractions.Value;
                        break;

                    case -101395:
                        ((ExportIntEntity)entity).Value = MontevideoUnits.Value;
                        break;

                    case -101385: //"Baseline FHR"
                        ((ExportIntEntity)entity).Value = BaselineFHR.Value;
                        break;

                    case -102122: //Baseline Class
                        ((ExportComboEntity)entity).Value = ctrlMeanBaselineVariability.SelectedValue.ToString();
                        ((ExportComboEntity)entity).OriginalValue = BaselineClass.OriginalValue;
                        break;

                    case -102123: //FHR Rhythm
                        ((ExportComboEntity)entity).Value = ctrlFHRRhythm.SelectedValue.ToString();
                        ((ExportComboEntity)entity).OriginalValue = FHRRhythm.OriginalValue;
                        break;

                    case -101420: //MeanBaselineVariability
                        ((ExportCalculatedComboEntity)entity).Value = ctrlVariability.SelectedValue.ToString();
                        ((ExportCalculatedComboEntity)entity).OriginalValue = Variability.OriginalValue;
                        break;

                    case -101417: // "NumOfAccelerations"                          
                        ((ExportCalculatedComboEntity)entity).Value = ctrlAccels.SelectedValue.ToString();
                        ((ExportCalculatedComboEntity)entity).OriginalValue = Accels.OriginalValue;
                        break;

                    case -101418: //"NumOfDecelerations"
                        {
                            //Decels = (ExportCalculatedCheckboxGroupEntity)entity;

                            if (m_chkNone.IsChecked == true)
                            {
                                ((ExportCalculatedCheckboxGroupEntity)entity).Value = NoneDecelerations.Value;
                            }
                            else
                            {
                                string decelsValue = String.Empty;

                                if (m_chkEarly.IsChecked == true)
                                {
                                    decelsValue = NumOfEarlyDecelerations.Value + ";";
                                }

                                if (m_chkVariable.IsChecked == true)
                                {
                                    decelsValue += NumOfVariableDecelerations.Value + ";";
                                }

                                if (m_chkLate.IsChecked == true)
                                {
                                    decelsValue += NumOfLateDecelerations.Value + ";";
                                }

                                if (m_chkProlonged.IsChecked == true)
                                {
                                    decelsValue += NumOfProlongedDecelerations.Value + ";";
                                }

                                if (m_chkOther.IsChecked == true)
                                {
                                    decelsValue += NumOfOtherDecelerations.Value + ";";
                                }

                                ((ExportCalculatedCheckboxGroupEntity)entity).Value = decelsValue.TrimEnd(';');
                            }                         
                        }
                        break;

                    case -102124:
                        ((ExportComboEntity)entity).Value = ctrlRecurrence.SelectedValue.ToString();
                        ((ExportComboEntity)entity).OriginalValue = Recurrence.OriginalValue.ToString();
                        break;

                    case -102013: //"Category"
                        ((ExportComboEntity)entity).Value = ctrlCategory.SelectedValue.ToString();
                        ((ExportComboEntity)entity).OriginalValue = Categories.OriginalValue;
                       break;

                    case -102125: //Checklist
                        ((ExportComboEntity)entity).Value = ctrlChecklist.SelectedValue.ToString();
                        ((ExportComboEntity)entity).OriginalValue = Checklist.OriginalValue;
                        break;

                    case -102114: //Comment
                        ((ExportStringEntity)entity).Value = Comment.Value;
                        ((ExportStringEntity)entity).OriginalValue = Comment.OriginalValue;
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

        void chkNone_Checked(object sender, RoutedEventArgs e)
        {
            if (m_chkNone.IsChecked == true)
            {
                m_chkEarly.IsChecked = false;
                m_chkVariable.IsChecked = false;
                m_chkLate.IsChecked = false;
                m_chkProlonged.IsChecked = false;
                m_chkOther.IsChecked = false;
            }
        }

        void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBoxWithValueControl ctrl = sender as CheckBoxWithValueControl;

            if (ctrl.IsChecked == true)
            {
                m_chkNone.IsChecked = false;
            }
        }

        private void exportToEMRWindow_LayoutUpdated(object sender, EventArgs e)
        {
           
        }

        private void exportToEMRWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.IsVisible == true)
            {
                this.Top = this.Bottom - this.ActualHeight;
            }
        }  
    }
}

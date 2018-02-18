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
using System.Windows.Media.Animation;
using Perigen.Patterns.NnetControls.Controls;
using Perigen.Patterns.NnetControls.Converters;
using Export.Entities;
using Export.Entities.ExportControlConfig;

namespace Perigen.Patterns.NnetControls.Screens
{
    /// <summary>
    /// Interaction logic for ExportToEMRWindow.xaml
    /// </summary>
    public partial class ExportToEMRWindow : Window
    {
        private const string Tag30Min = "30";
        private const string Tag15Min = "15";
        private const string TagLeft15Min = "15Lt";
        private const string TagRight15Min = "15Rt";

        private int m_TabOrder = 1;
        private bool m_isFirst = true;

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(ExportToEMRWindow), new UIPropertyMetadata(false));

        private PatternsUIManager UiManager { get; set; }

        private Interval Interval { get; set; }

        private List<RadioButton> m_ViewOptions = new List<RadioButton>();
        private string m_LastViewOptionTag;
        private List<string> TabsToReview { get; set; }
        private Dictionary<string, UIElement> TabFocusControls { get; set; }

        public ExportToEMRWindow(PatternsUIManager uiManager, DateTime from, DateTime to, Interval interval)
        {
            UiManager = uiManager;
            Interval = interval;
            TabsToReview = new List<string>();
            TabFocusControls = new Dictionary<string, UIElement>();

            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            UiManager.StopExportHolder.Clear();
            UiManager.AfterExportClosed(Interval.StartTime, Interval.IntervalDuration);
            this.IsCanceled = true;
            this.DialogResult = false;
            this.Close();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabsToReview.Count == 0)
            {
                bool allowExport = true;

                foreach(IValidatableControl ctrl in UiManager.StopExportHolder)
                {
                    if(ctrl.IsValidValue() == false)
                    {
                        allowExport = false;
                        break;
                    }
                }

                if (allowExport == true)
                {
                    UiManager.StopExportHolder.Clear();
                    UiManager.SaveExportIntervalData(Interval);
                    UiManager.AfterExportClosed(Interval.StartTime, Interval.IntervalDuration);
                    this.Close();
                }
                else
                {
                    MessageBoxWindow msgBox = new MessageBoxWindow();
                    msgBox.MessageTitle.Text = "Warning";
                    msgBox.MessageDescription.Text = "A documentation error was identified and must be fixed before \nperforming Export.";
                    msgBox.ShowDialog();
                }
            }
            else
            {
                CompleteReview();
            }
        }


        private void exportToEMRWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CreateExportControls();
        }

        private void CreateExportControls()
        {
            m_ViewOptions = new List<RadioButton>();
            foreach (var tab in UiManager.ExportDataConfig.Tabs)
                CreateTab(tab);
        }

        private void CreateTab(ExportTab tabConfig)
        {
            TabItem tabItem = new TabItem()
            {
                Header = tabConfig.TabTitle,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsTabStop = false
            };

            tabItem.Content = CreateTabContent(tabConfig);
            m_exportTabControl.Items.Add(tabItem);
        }

        private GridLength GetGridLength(string strLength)
        {
            GridLengthConverter converter = new GridLengthConverter();

            return (GridLength)converter.ConvertFromString(strLength);
        }

        private object CreateTabContent(ExportTab tabConfig)
        {
            m_TabOrder = 10;
            m_isFirst = true;

            StackPanel tabPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            UIElement intervalSummaryPanel = CreateIntervalSummaryPanel();
            tabPanel.Children.Add(intervalSummaryPanel);

            Grid gridColumns = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            foreach (ExportColumn col in tabConfig.Columns)
            {
                ColumnDefinition gridColumn = new ColumnDefinition();
                gridColumn.Width = GetGridLength(col.Width);
                gridColumns.ColumnDefinitions.Add(gridColumn);

                StackPanel column = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };

                foreach (ExportEntity entity in col.Entities)
                {
                    m_TabOrder++;

                    BaseConcept concept = Interval.Concepts.FirstOrDefault(c => c.Id == entity.ConceptId);
                    bool isTabToReview = false;

                    if (concept != null)
                    {
                        if (concept.Value != null && concept.Value.ToString() != String.Empty)
                        {
                            isTabToReview = true;
                        }

                        UIElement control = CreateExportControl(entity, concept);
                        ((UserControl)control).TabIndex = m_TabOrder;
                        ((UserControl)control).IsTabStop = false;

                        if (entity.ControlType == ExportControlTypes.CalculatedCheckboxGroup)
                        {
                            m_TabOrder += ((CalculatedCheckboxGroupConcept)concept).Items.Count;
                        }

                        if (entity.IsMVU == true)
                        {
                            if (UiManager.IsMontevideoVisible == true)
                            {
                                column.Children.Add(control);

                                if (m_isFirst == true)
                                {
                                    TabFocusControls.Add(tabConfig.TabTitle, control);
                                    m_isFirst = false;
                                }
                            }
                            else
                            {
                                Interval.Concepts.Remove(concept);
                                isTabToReview = false;
                            }
                        }
                        else
                        {
                            column.Children.Add(control);

                            if (m_isFirst == true)
                            {
                                TabFocusControls.Add(tabConfig.TabTitle, control);
                                m_isFirst = false;
                            }
                        }

                        if (isTabToReview == true && TabsToReview.Contains(tabConfig.TabTitle) == false)
                        {
                            TabsToReview.Add(tabConfig.TabTitle);
                        }
                    }
                }

                Grid.SetColumn(column, tabConfig.Columns.IndexOf(col));

                gridColumns.Children.Add(column);
            }


            tabPanel.Children.Add(gridColumns);

            return tabPanel;
        }

        private UIElement CreateIntervalSummaryPanel()
        {
            Grid grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            ColumnDefinition column = new ColumnDefinition();
            column.Width = GetGridLength("Auto");
            grid.ColumnDefinitions.Add(column);

            column = new ColumnDefinition();
            column.Width = GetGridLength("*");
            grid.ColumnDefinitions.Add(column);

            column = new ColumnDefinition();
            column.Width = GetGridLength("Auto");
            grid.ColumnDefinitions.Add(column);

            StackPanel panel = CreateIntervalInfoPanel();
            Grid.SetColumn(panel, 0);
            grid.Children.Add(panel);

            panel = CreateViewOptionsPanel();
            Grid.SetColumn(panel, 1);
            grid.Children.Add(panel);

            panel = CreateIntervalInfoPanel();
            panel.Visibility = Visibility.Hidden;
            Grid.SetColumn(panel, 2);
            grid.Children.Add(panel);

            return grid;
        }

        private StackPanel CreateIntervalInfoPanel()
        {
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
            };

            TextBlock intervalLabel = new TextBlock()
            {
                Style = (Style)FindResource("TitleTextStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Export Time Range:",
                Margin = new Thickness(15, 5, 0, 10)
            };

            TextBlock intervalTime = new TextBlock()
            {
                Style = (Style)FindResource("TimeTextStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Text = string.Format("{0:HH:mm} - {1:HH:mm}", Interval.StartTime.ToLocalTime(),
                                                              Interval.StartTime.ToLocalTime().AddMinutes(Interval.IntervalDuration)),
                Margin = new Thickness(10, 5, 0, 10)
            };

            panel.Children.Add(intervalLabel);
            panel.Children.Add(intervalTime);
            return panel;
        }

        private StackPanel CreateViewOptionsPanel()
        {
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock viewLabel = new TextBlock()
            {
                Style = (Style)FindResource("TitleTextStyle"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Adjust Tracing Display:",
                Margin = new Thickness(15, 5, 0, 10)
            };

            List<RadioButtonContainer> viewOptions = CreateViewRadioButtons();

            panel.Children.Add(viewLabel);
            foreach (RadioButtonContainer rb in viewOptions)
            {
                panel.Children.Add(rb);
                m_ViewOptions.Add(rb.Control);
            }

            return panel;
        }

        private List<RadioButtonContainer> CreateViewRadioButtons()
        {
            List<RadioButtonContainer> toRet = new List<RadioButtonContainer>();

            for (int i = 1; i <= 3; i++)
            {
                string tag, content;
                bool isChecked;

                if (Interval.IntervalDuration == 30)
                {
                    // 30 minute interval
                    if (i == 1)
                    {
                        tag = Tag30Min;
                        content = "30min";
                        isChecked = !UiManager.Is15MinView;
                    }
                    else if (i == 2)
                    {
                        tag = TagLeft15Min;
                        content = "Lt 15min";
                        isChecked = UiManager.Is15MinView && UiManager.Is15MinLeftView;
                    }
                    else
                    {
                        tag = TagRight15Min;
                        content = "Rt 15min";
                        isChecked = UiManager.Is15MinView && !UiManager.Is15MinLeftView;
                    }
                }
                else
                {
                    // 15 minute interval
                    if (i == 1)
                    {
                        tag = Tag30Min;
                        content = "30min";
                        isChecked = !UiManager.Is15MinView;
                    }
                    else if (i == 2)
                    {
                        tag = Tag15Min;
                        content = "15min";
                        isChecked = UiManager.Is15MinView;
                    }
                    else
                    {
                        continue;
                    }
                }

                RadioButtonContainer rb = new RadioButtonContainer();
                rb.Control.Tag = tag;
                rb.Control.Content = content;
                rb.Control.Margin = new Thickness(5, 5, 0, 10);
                rb.Control.HorizontalAlignment = HorizontalAlignment.Left;
                rb.Control.VerticalAlignment = VerticalAlignment.Bottom;
                rb.Control.IsChecked = isChecked;
                rb.Control.Checked += rbView_Checked;
                rb.Control.TabIndex = i;

                if (rb.Control.IsChecked == true)
                    m_LastViewOptionTag = (string)rb.Control.Tag;

                toRet.Add(rb);
            }

            return toRet;
        }

        private void rbView_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null)
            {
                string tag = rb.Tag as string;

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    SetPatternsView(m_LastViewOptionTag, tag);
                    UpdateAllViewRadioButtons(rb, tag);
                }
            }
        }

        //private void SetPatternsView(string previousTag, string newTag)
        //{
        //    if (previousTag == Tag30Min && newTag != Tag30Min)
        //        UiManager.RaiseViewSwitchEvent(true);

        //    if (previousTag != Tag30Min && newTag == Tag30Min)
        //        UiManager.RaiseViewSwitchEvent(false);

        //    if (Interval.IntervalDuration == 30)
        //    {
        //        if (newTag == TagRight15Min)
        //            UiManager.RaiseToggleSwitchEvent(true);

        //        if (newTag == TagLeft15Min)
        //            UiManager.RaiseToggleSwitchEvent(false);
        //    }

        //    m_LastViewOptionTag = newTag;
        //}
        private void SetPatternsView(string previousTag, string newTag)
        {
            if (previousTag != Tag30Min && newTag == Tag30Min)
                UiManager.RaiseViewSwitchEvent(false);
            else if (Interval.IntervalDuration == 15)
            {
                if (previousTag == Tag30Min && newTag != Tag30Min)
                    UiManager.RaiseViewSwitchEvent(true);
            }
            else if (Interval.IntervalDuration == 30)
            {
                if (newTag == TagRight15Min)
                    UiManager.RaiseViewSwitchToRight15MinEvent();

                if (newTag == TagLeft15Min)
                    UiManager.RaiseViewSwitchToLeft15MinEvent();
            }

            m_LastViewOptionTag = newTag;
        }
        private void UpdateAllViewRadioButtons(RadioButton changedRadioButton, string tag)
        {
            if (m_ViewOptions.Count > 0)
                foreach (RadioButton item in m_ViewOptions)
                {
                    string itemTag = item.Tag as string;
                    if (itemTag == tag && item.IsChecked == false)
                    {
                        item.Checked -= rbView_Checked;
                        item.IsChecked = true;
                        item.Checked += rbView_Checked;
                    }
                    if (itemTag != tag && item.IsChecked == true)
                    {
                        item.Checked -= rbView_Checked;
                        item.IsChecked = false;
                        item.Checked += rbView_Checked;
                    }
                }
        }

        private UIElement CreateExportControl(ExportEntity entity, BaseConcept data)
        {
            UIElement control = null;

            switch (entity.ControlType)
            {
                case ExportControlTypes.Int:
                    control = new IntControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (IntConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.Double:
                    control = new DoubleControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (DoubleConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.String:
                    control = new StringControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (StringConcept)data,
                        IsCanceled = this.IsCanceled,
                        RowsNum = entity.MaxSelect.HasValue && entity.MaxSelect.Value > 0 ? entity.MaxSelect.Value : 1
                    };
                    break;

                case ExportControlTypes.Combo:
                    control = new ComboBoxControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (ComboConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.ComboMultiValue:
                    control = new ComboMultiValue()
                    {
                        UiManager = this.UiManager,
                        ControlData = (ComboMultiValueConcept)data,
                        IsCanceled = this.IsCanceled,
                        MaxSelect = entity.MaxSelect.HasValue ? (int)entity.MaxSelect.Value : 1
                    };
                    break;

                case ExportControlTypes.CalculatedCombo:
                    control = new CalculatedComboBoxControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (CalculatedComboConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.CalculatedCheckboxGroup:
                    control = new DecelsUserControl()
                    {
                        UiManager = this.UiManager,
                        ControlData = (CalculatedCheckboxGroupConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.Rounding:
                    control = new RoundingControlContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (IntConcept)data,
                        IsCanceled = this.IsCanceled,
                        RoundBy = entity.RoundBy.HasValue ? (int)entity.RoundBy.Value : 1
                    };
                    break;

                case ExportControlTypes.CheckBox:
                    control = new CheckBoxContainer()
                    {
                        UiManager = this.UiManager,
                        ControlData = (IntConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.Range:
                    control = new RangeControl()
                    {
                        UiManager = this.UiManager,
                        ControlData = (StringConcept)data,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                case ExportControlTypes.RangeDoubleConcept:
                    BaseConcept readOnlyData = Interval.Concepts.FirstOrDefault(c => c.Id == entity.ReadOnlyId);
                    control = new RangeDoubleConceptControl()
                    {
                        UiManager = this.UiManager,
                        ControlData = (StringConcept)data,
                        ReadOnlyData = (StringConcept)readOnlyData,
                        IsCanceled = this.IsCanceled
                    };
                    break;

                default:
                    break;
            }

            if(entity.IsHidden == true)
            {
                control.Visibility = Visibility.Collapsed;
            }

            return control;
        }

        private void CompleteReview()
        {
            string msg = String.Format("Review the {0} tab before completing the Export", TabsToReview[0]);

            MessageBoxWindow msgBox = new MessageBoxWindow();
            msgBox.MessageTitle.Text = "Warning";
            msgBox.MessageDescription.Text = msg;
            msgBox.ShowDialog();

            foreach (TabItem tab in m_exportTabControl.Items)
            {
                if (tab.Header.ToString() == TabsToReview[0])
                {
                    m_exportTabControl.SelectedItem = tab;
                    break;
                }
            }
        }
        private void ExportTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                TabControl tabControl = (TabControl)e.Source;
                TabItem tab = (TabItem)tabControl.SelectedItem;

                if (TabsToReview.Contains(tab.Header.ToString()))
                {
                    TabsToReview.Remove(tab.Header.ToString());
                }

                IBaseExportControl baseCtrl = (IBaseExportControl)TabFocusControls[tab.Header.ToString()];

                if (baseCtrl != null)
                    baseCtrl.SetFocus();
            }
        }
    }
}

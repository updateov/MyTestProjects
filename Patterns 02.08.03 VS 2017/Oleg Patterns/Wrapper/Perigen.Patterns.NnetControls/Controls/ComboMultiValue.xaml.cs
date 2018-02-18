using Export.Entities;
using Perigen.Patterns.NnetControls.Screens;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Perigen.Patterns.NnetControls.Controls
{
    public partial class ComboMultiValue : UserControl, IBaseExportControl
    {
        #region Data

        public ComboMultiValueConcept ControlData
        {
            get { return (ComboMultiValueConcept)GetValue(ControlDataProperty); }
            set { SetValue(ControlDataProperty, value); }
        }
        public static readonly DependencyProperty ControlDataProperty =
            DependencyProperty.Register("ControlData", typeof(ComboMultiValueConcept), typeof(ComboMultiValue), new PropertyMetadata(null));


        public PatternsUIManager UiManager
        {
            get { return (PatternsUIManager)GetValue(UiManagerProperty); }
            set { SetValue(UiManagerProperty, value); }
        }
        public static readonly DependencyProperty UiManagerProperty =
            DependencyProperty.Register("UiManager", typeof(PatternsUIManager), typeof(ComboMultiValue), new PropertyMetadata(null));

        public bool IsCanceled
        {
            get { return (bool)GetValue(IsCanceledProperty); }
            set { SetValue(IsCanceledProperty, value); }
        }
        public static readonly DependencyProperty IsCanceledProperty =
            DependencyProperty.Register("IsCanceled", typeof(bool), typeof(ComboMultiValue), new UIPropertyMetadata(false));


        private int m_MaxSelect = 1;
        public int MaxSelect
        {
            get { return m_MaxSelect; }
            set
            {
                m_MaxSelect = value;

                ObservableCollection<ValueToDisplay> newDisplayedValues = new ObservableCollection<ValueToDisplay>();
                for (int i = 0; i < m_MaxSelect; i++)
                    newDisplayedValues.Add(new ValueToDisplay(null, null));
                ValuesToDisplay = newDisplayedValues;
            }
        }

        public double MaxDropDownHeight { get; set; }

        private ObservableCollection<ValueToDisplay> m_ValuesToDisplay = new ObservableCollection<ValueToDisplay>();
        public ObservableCollection<ValueToDisplay> ValuesToDisplay
        {
            get { return m_ValuesToDisplay; }
            set { m_ValuesToDisplay = value; }
        }

        private ListBox listValues = null;
        private ListBox listValuesToDisplay = null;
        private bool isInitialSelection = false;
        private bool initialSelectionSet = false;

        #endregion

        #region Constructors

        public ComboMultiValue()
        {
            InitializeComponent();
            cbMultiSelect.TabIndex = this.TabIndex;
        }

        #endregion

        #region ComboBox control

        private void cbMultiSelect_Loaded(object sender, RoutedEventArgs e)
        {
            cbMultiSelect.TabIndex = this.TabIndex;
            this.DataContext = ControlData;
            if (MaxDropDownHeight != 0)
                cbMultiSelect.MaxDropDownHeight = MaxDropDownHeight;
            if (listValues != null && listValuesToDisplay != null)
            {
                listValues.SelectionMode = MaxSelect > 1 ? SelectionMode.Multiple : SelectionMode.Single;
                if (!initialSelectionSet)
                {
                    SetInitialComboSelection();
                    initialSelectionSet = true;
                }
            }
        }

        private void listValues_Initialized(object sender, EventArgs e)
        {
            listValues = (ListBox)sender;
        }

        private void listValues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listValues.SelectedItems.Count <= MaxSelect)
            {
                if (e.RemovedItems.Count > 0)
                    foreach (object obj in e.RemovedItems)
                    {
                        ComboMultiValueConceptItem item = (ComboMultiValueConceptItem)obj;
                        if (!isInitialSelection)
                            RemoveControlValue(item);
                    }
                if (e.AddedItems.Count > 0)
                    foreach (object obj in e.AddedItems)
                    {
                        ComboMultiValueConceptItem item = (ComboMultiValueConceptItem)obj;
                        if (!isInitialSelection)
                            AddControlValue(item);
                    }
                UpdateValuesToDisplay();
                if (listValues.SelectionMode == SelectionMode.Single)
                    CloseDropDown();
            }
            else
            {
                listValues.SelectedItems.Remove(e.AddedItems[0]);

                errorBorder.BorderThickness = new Thickness(1);

                MessageBoxWindow msgBox = new MessageBoxWindow();
                msgBox.MessageTitle.Text = "Warning";
                msgBox.MessageDescription.Text = string.Format("You cannot select more than {0} items.", MaxSelect);
                msgBox.ShowDialog();

                errorBorder.BorderThickness = new Thickness(0);

                OpenDropDown();
            }
        }

        private void listValuesToDisplay_Initialized(object sender, EventArgs e)
        {
            listValuesToDisplay = (ListBox)sender;
        }

        private void listValuesToDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && ((ValueToDisplay)e.AddedItems[0]).DisplayValue == null)
                listValuesToDisplay.SelectedItem = null;
        }

        private void tbDisplayedValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tbDisplayedValue = sender as TextBlock;
            ValueToDisplay selectedValueToDisplay = tbDisplayedValue.DataContext as ValueToDisplay;
            cbMultiSelect.IsDropDownOpen = !cbMultiSelect.IsDropDownOpen;
        }

        private void tbDropDownValue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (listValues.SelectionMode == SelectionMode.Single)
                CloseDropDown();
        }

        private void btnCloseDropDown_Click(object sender, RoutedEventArgs e)
        {
            CloseDropDown();
        }

        private void PART_Popup_Closed(object sender, EventArgs e)
        {
            if (listValues.SelectedItems.Count >= 0)
            {
                List<string> selectedValues = listValues.SelectedItems.Cast<ComboMultiValueConceptItem>().Select(item => item.Value).ToList();
                if (listValues.SelectionMode == SelectionMode.Multiple && selectedValues.Contains(string.Empty))
                {
                    List<ComboMultiValueConceptItem> itemsToRemove = new List<ComboMultiValueConceptItem>();
                    foreach (object item in listValues.SelectedItems)
                    {
                        ComboMultiValueConceptItem conceptItem = (ComboMultiValueConceptItem)item;
                        if (conceptItem.Value != string.Empty)
                            itemsToRemove.Add(conceptItem);
                    }
                    foreach (ComboMultiValueConceptItem conceptItem in itemsToRemove)
                        listValues.SelectedItems.Remove(conceptItem);
                }
            }
            listValuesToDisplay.UnselectAll();

            Dispatcher.BeginInvoke(new Action(() => SetFocusOnFirstItem(listValuesToDisplay)), DispatcherPriority.Input);
        }

        private void OpenDropDown()
        {
            if (!cbMultiSelect.IsDropDownOpen)
                cbMultiSelect.IsDropDownOpen = true;
        }

        private void CloseDropDown()
        {
            if (cbMultiSelect.IsDropDownOpen)
                cbMultiSelect.IsDropDownOpen = false;
        }

        private void SetInitialComboSelection()
        {
            try
            {
                isInitialSelection = true;

                if (ControlData != null)
                {
                    string controlValue = ControlData.Value as string;
                    if (controlValue != null)
                    {
                        string[] controlValues = controlValue.Split(new char[] { ';' }, StringSplitOptions.None);
                        foreach (ComboMultiValueConceptItem item in ControlData.Items)
                            foreach (string value in controlValues)
                            {
                                if (item.Value == value)
                                {
                                    if (listValues.SelectionMode == SelectionMode.Multiple)
                                        listValues.SelectedItems.Add(item);
                                    else
                                    {
                                        listValues.SelectedItem = item;
                                        break;
                                    }
                                }
                            }
                    }
                }
            }
            finally
            {
                isInitialSelection = false;
            }
        }

        private string GetValueToDisplay(string conceptValue)
        {
            string currentControlValue = (string)ControlData.Value;
            if (string.IsNullOrWhiteSpace(currentControlValue))
                return conceptValue;

            string[] controlValues = currentControlValue.Split(new char[] { ';' }, StringSplitOptions.None);
            foreach (string controlValue in controlValues)
            {
                if (controlValue == conceptValue)
                    return controlValue;
            }
            return null;
        }

        private void UpdateValuesToDisplay()
        {
            if (listValues.SelectedItems.Count == 0)
            {
                for (int i = 0; i < ValuesToDisplay.Count; i++)
                    ValuesToDisplay[i] = new ValueToDisplay(null, null);
            }
            else
            {
                List<ComboMultiValueConceptItem> selectedValues = listValues.SelectedItems.Cast<ComboMultiValueConceptItem>().ToList();
                List<ComboMultiValueConceptItem> availableValues = ControlData.Items.Cast<ComboMultiValueConceptItem>().ToList();

                int index = -1;
                foreach (ComboMultiValueConceptItem availableItem in availableValues)
                {
                    ComboMultiValueConceptItem item1 = selectedValues.FirstOrDefault(item => item.Value == availableItem.Value);
                    if (item1 != null)
                    {
                        index++;
                        ValuesToDisplay[index] = new ValueToDisplay(availableItem.Value, availableItem.Value);
                    }
                }

                for (int i = index + 1; i < ValuesToDisplay.Count; i++)
                    ValuesToDisplay[i] = new ValueToDisplay(null, null);
            }

            listValuesToDisplay.ToolTip = GetControlValuesToolTip();
        }

        private string GetControlValuesToolTip()
        {
            string controlValue = ControlData.Value as string;
            if (string.IsNullOrWhiteSpace(controlValue))
                return null;

            string[] values = controlValue.Split(new char[] { ';' }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, values);
        }

        private void AddControlValue(ComboMultiValueConceptItem conceptItemToAdd)
        {
            string controlValue = ControlData.Value as string;
            if (string.IsNullOrWhiteSpace(controlValue))
            {
                ControlData.Value = conceptItemToAdd.Value;
            }
            else
            {
                if (listValues.SelectionMode == SelectionMode.Multiple)
                {
                    List<string> currentValues = controlValue.Split(new char[] { ';' }, StringSplitOptions.None).ToList();

                    StringBuilder sb = new StringBuilder();
                    bool isFirst = true;
                    foreach (ComboMultiValueConceptItem availableItem in ControlData.Items)
                    {
                        string valueToAdd = null;
                        if (availableItem.Value == conceptItemToAdd.Value)
                        {
                            valueToAdd = conceptItemToAdd.Value;
                        }
                        else
                        {
                            if (currentValues.Contains(availableItem.Value))
                                valueToAdd = availableItem.Value;
                        }

                        if (valueToAdd != null)
                        {
                            if (isFirst)
                                sb.Append(valueToAdd);
                            else
                                sb.Append(string.Format(";{0}", valueToAdd));

                            isFirst = false;
                        }
                    }

                    ControlData.Value = sb.ToString();
                }
                else
                    ControlData.Value = conceptItemToAdd.Value;
            }
        }

        private void RemoveControlValue(ComboMultiValueConceptItem conceptItemToRemove)
        {
            string controlValue = ControlData.Value as string;
            if (string.IsNullOrWhiteSpace(controlValue))
            {
                if (conceptItemToRemove.Value == string.Empty)
                    ControlData.Value = null;
            }
            else
            {
                List<string> currentValues = controlValue.Split(new char[] { ';' }, StringSplitOptions.None).ToList();
                if (currentValues.Contains(conceptItemToRemove.Value))
                    currentValues.Remove(conceptItemToRemove.Value);
                ControlData.Value = string.Join(new string(new char[] { ';' }), currentValues);
            }
        }

        private void listValuesToDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Dispatcher.BeginInvoke(new Action(() => OpenDropDown()), DispatcherPriority.Input);
                Dispatcher.BeginInvoke(new Action(() => SetFocusOnFirstItem(listValues)), DispatcherPriority.Input);
            }
        }

        private void SetFocusOnFirstItem(ListBox listBox)
        {
            if (listBox != null && listBox.Items.Count > 0)
            {
                var listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(listBox.Items[0]);
                if (listBoxItem != null)
                    listBoxItem.Focus();
            }
        }

        public void SetFocus()
        {
            Dispatcher.BeginInvoke(new Action(() => SetFocusOnFirstItem(listValuesToDisplay)), DispatcherPriority.ContextIdle);
        }

        #endregion
    }

    public class ValueToDisplay
    {
        public string ConceptValue { get; set; }

        public string DisplayValue { get; set; }

        public ValueToDisplay(string conceptValue, string displayValue)
        {
            ConceptValue = conceptValue;
            DisplayValue = displayValue;
        }
    }
}

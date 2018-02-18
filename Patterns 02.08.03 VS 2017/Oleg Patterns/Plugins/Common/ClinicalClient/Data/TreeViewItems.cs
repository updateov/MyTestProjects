using CRIEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace PatternsCRIClient.Data
{
    public class TreeViewItems : DependencyObject, INotifyPropertyChanged
    {
        private ObservableCollection<PatientData> m_level2Items = new ObservableCollection<PatientData>();
        private int m_count;
        public string GroupName { get; set; }
        public CRIState CRIStatus { get; set; }
        public bool IsExpandedGroup { get; set; }
        
        public int Count
        {
            get
            {
                return m_count;
            }
            set 
            {
                m_count = value;
                RaisePropertyChanged("Count");
            }
        }

        public ObservableCollection<PatientData> SecondLevelItems
        {
            get
            {
                return m_level2Items;
            }

            private set
            {
                if (m_level2Items != value)
                {
                    m_level2Items = value;
                    RaisePropertyChanged("SecondLevelItems");
                }
            }
        }

        public TreeViewItems()
        {
            IsExpandedGroup = true;
        }

        public TreeViewItems(List<PatientData> visits)
        {
            ConvertVisits(visits);
        }

        public void UpdateData(List<PatientData> visits)
        {
            //m_level2Items.Clear();
            ConvertVisits(visits);
        }

        public void SetSelectedPatient(string visitKey)
        {
            ResetSelection();

            if (String.IsNullOrEmpty(visitKey) == false)
            {
                PatientData patData = m_level2Items.FirstOrDefault(t => t.Key == visitKey);

                if (patData != null)
                {
                    patData.IsSelectedPatient = true;
                }
            }
        }

        private void ResetSelection()
        {
            PatientData data = m_level2Items.FirstOrDefault(t => t.IsSelectedPatient == true);

            if (data != null)
            {
                data.IsSelectedPatient = false;
            }
        }

        private void ConvertVisits(List<PatientData> dataHolder)
        {
            List<PatientData> items = new List<PatientData>();

            dataHolder.ForEach(patient =>
            {
                patient.IsLast = false;
                items.Add(patient);
            });

            this.SecondLevelItems.MergePatients(items);

            if (m_level2Items.Count > 0)
            {
                m_level2Items.ElementAt(items.Count - 1).IsLast = true;
            }

            this.Count = m_level2Items.Count;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}

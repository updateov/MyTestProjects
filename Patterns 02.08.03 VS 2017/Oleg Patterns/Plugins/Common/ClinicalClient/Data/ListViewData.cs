//Review: 17/02/15
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CRIEntities;
using System.ComponentModel;
using System.Collections;
using PatternsCRIClient.Screens;

namespace PatternsCRIClient.Data
{
    public class ListViewData : INotifyPropertyChanged
    {
        private ObservableCollection<PatientData> m_patients = new ObservableCollection<PatientData>();

        public ObservableCollection<PatientData> PatientsList
        {
            get
            {          
                return m_patients;
            }

            private set
            {
                if (m_patients != value)
                {
                    m_patients = value;
                    RaisePropertyChanged("PatientsList");
                }
            }
        }

        public ListViewData(List<PatientData> patientList, SortType sortType)
        {
            UpdateData(patientList, sortType);
        }

        public void SetSelectedPatient(string visitKey)
        {
            ResetSelection();
           
            if (String.IsNullOrEmpty(visitKey) == false)
            {
                PatientData patData = m_patients.FirstOrDefault(t => t.Key == visitKey);

                if (patData != null)
                {
                    patData.IsSelectedPatient = true;
                }
            }
        }

        public void UpdateData(List<PatientData> patientList, SortType sortType)
        {
            List<PatientData> sortList = new List<PatientData>();
            patientList.ForEach(patient =>
            {
                PatientData data = new PatientData();
                data.CopyData(patient);
                sortList.Add(data);
            });

            switch (sortType)
            {
                case SortType.Names_AZ:
                    sortList.Sort((a, b) => a.SortDisplayNameIndex.CompareTo(b.SortDisplayNameIndex));
                    break;

                case SortType.Names_ZA:
                    sortList.Sort((a, b) => b.SortDisplayNameIndex.CompareTo(a.SortDisplayNameIndex));
                    break;

                case SortType.BedName_Asc:
                    sortList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
                    break;

                case SortType.BedName_Desc:
                    sortList.Sort((a, b) => b.SortBedNameIndex.CompareTo(a.SortBedNameIndex));
                    break;
            }

            ConvertVisits(sortList);
        }

        private void ResetSelection()
        {
            PatientData data = m_patients.FirstOrDefault(t => t.IsSelectedPatient == true);

            if (data != null)
            {
                data.IsSelectedPatient = false;
            }
        }

        private void ConvertVisits(List<PatientData> dataHolder)
        {
            //List<PatientData> items = new List<PatientData>();

            //dataHolder.ForEach(patient =>
            //{
            //    items.Add(patient);
            //});

            this.PatientsList.MergePatients(dataHolder);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CRIEntities;

namespace PatternsCRIClient.Data
{
    public class TreeViewData : ObservableCollection<TreeViewItems>
    {
        private TreeViewItems m_itemPositive;
        private TreeViewItems m_itemUnknown;
        private TreeViewItems m_itemNegative;

        public TreeViewData(List<PatientData> visits)
        {
            m_itemPositive = new TreeViewItems();
            m_itemPositive.CRIStatus = CRIState.PositiveCurrent;
            m_itemPositive.GroupName = (string)Application.Current.FindResource("Group_CRI_Positive") + "\n" + 
                                       (string)Application.Current.FindResource("Group_Current_or_Past");
            Add(m_itemPositive);

            m_itemUnknown = new TreeViewItems();
            m_itemUnknown.CRIStatus = CRIState.Negative;
            m_itemUnknown.GroupName = (string)Application.Current.FindResource("Group_CRI_Unknown");
            m_itemUnknown.IsExpandedGroup = false;
            Add(m_itemUnknown);

            m_itemNegative = new TreeViewItems();
            m_itemNegative.CRIStatus = CRIState.Negative;
            m_itemNegative.GroupName = (string)Application.Current.FindResource("Group_CRI_Negative");
            m_itemNegative.IsExpandedGroup = false;
            Add(m_itemNegative);

            LoadData(visits);
        }

        public void UpdateData(List<PatientData> visits)
        {
            LoadData(visits);
        }

        public void SetSelectedPatient(string visitKey)
        {
            var topLevelData = this.GetEnumerator();

            while (topLevelData.MoveNext())
            {
                topLevelData.Current.SetSelectedPatient(visitKey);
            }
        }

        private void LoadData(List<PatientData> visits)
        {
            List<PatientData> dataHolder = new List<PatientData>();

            visits.ForEach(patient =>
            {
                PatientData data = new PatientData();
                data.CopyData(patient);
                dataHolder.Add(data);
            });

            List<PatientData> positiveAllStatuses = new List<PatientData>();

            List<PatientData> positiveList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.PositiveCurrent));
            positiveList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            positiveAllStatuses.AddRange(positiveList);

            positiveList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.PositiveReviewed));
            positiveList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            positiveAllStatuses.AddRange(positiveList);

            positiveList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.PositivePastNotYetReviewed));
            positiveList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            positiveAllStatuses.AddRange(positiveList);

            m_itemPositive.UpdateData(positiveAllStatuses);


            List<PatientData> unknownAllStatuses = new List<PatientData>();

            List<PatientData> unknownList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.UnknownNotEnoughTime));
            unknownList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            unknownAllStatuses.AddRange(unknownList);

            unknownList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.UnknownGAOrSingletonMissing));
            unknownList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            unknownAllStatuses.AddRange(unknownList);

            unknownList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.UnknownGAOrSingletonNotMet));
            unknownList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            unknownAllStatuses.AddRange(unknownList);

            unknownList = dataHolder.FindAll(t => (t.CRIStatus == CRIState.Off));
            unknownList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));
            unknownAllStatuses.AddRange(unknownList);

            m_itemUnknown.UpdateData(unknownAllStatuses);


            List<PatientData> negativeList = dataHolder.FindAll(t => t.CRIStatus == CRIState.Negative);
            negativeList.Sort((a, b) => a.SortBedNameIndex.CompareTo(b.SortBedNameIndex));

            m_itemNegative.UpdateData(negativeList);
        }
    }
}

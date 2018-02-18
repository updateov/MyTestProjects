using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CRIEntities;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using PatternsCRIClient.Data;
using System.Diagnostics;
using CommonLogger;

namespace PatternsCRIClient
{
    internal static class ExtensionMethods  
    {
        public static void FillFromVisits(this List<PatientData> patientList, List<Visit> visits)
        {
            patientList.Clear();

            visits.ForEach(visit =>
            {
                patientList.Add(new PatientData(visit));
            });    
        }

        public static bool SetSelectedPatient(this List<PatientData> patientList, PatientData selectedPatient)
        {
            if(patientList.Exists(t => t.Key == selectedPatient.Key))
            {
                patientList.First(t => t.Key == selectedPatient.Key).IsSelectedPatient = true;
                return true;
            }
            return false;
        }

        public static string GetDisplayName(this Visit visit)
        {
            string fName = String.Empty;
            string lName = String.Empty;

            if (visit.FirstName != null)
            {
                fName = visit.FirstName.Length >= 3 ? visit.FirstName.Substring(0, 3) : visit.FirstName;
            }
            if (visit.LastName != null)
            {
                lName = visit.LastName.Length >= 3 ? visit.LastName.Substring(0, 3) : visit.LastName;
            }

            return (fName + " " + lName).Trim(); ;
        }

        public static void GetSortedRange(this List<PatientData> patientList, List<Visit> visits)
        {
            patientList.Clear();

            visits.ForEach(visit =>
            {
                patientList.Add(new PatientData(visit));
            });

            int sortIndex = 0;

            List<PatientData> sortByName = patientList.OrderBy(t => t.PatientDisplayName).ToList();
            sortByName.ForEach(patient =>
            {
                patient.SortDisplayNameIndex = sortIndex++;
            });

            sortIndex = 0;

            List<PatientData> sortByBedName = patientList.OrderBy(t => (t.BedName+ " " + t.PatientDisplayName)).ToList();
            sortByBedName.ForEach(patient =>
            {
                patient.SortBedNameIndex = sortIndex++;
            });
        }

        public static void MergePatients(this ObservableCollection<PatientData> currentList, List<PatientData> updatedList)
        {
            try
            {
                if (currentList.Count > updatedList.Count)
                {
                    for (int idx = currentList.Count; idx > updatedList.Count; idx--)
                    {
                        currentList.RemoveAt(idx - 1);
                    }
                }

                for (int i = 0; i < updatedList.Count; i++)
                {
                    PatientData data = currentList.ElementAtOrDefault(i);

                    if (i < updatedList.Count)
                    {
                        if (data != null)
                        {
                            data.CopyData(updatedList.ElementAt(i));
                        }
                        else
                        {
                            currentList.Add(updatedList.ElementAt(i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Warning, Properties.Resources.CRIClient_ModuleName,
                    "Failed to Merge PatientsData ObservableCollection. \ncurrentList.Count = " + currentList.Count.ToString() + "\nupdatedList.Count = " + updatedList.Count.ToString(), ex);
            }
        }

        public static void MergePatients(this List<PatientData> currentList, List<PatientData> updatedList)
        {
            try
            {
                if (currentList.Count > updatedList.Count)
                {
                    for (int idx = currentList.Count; idx > updatedList.Count; idx--)
                    {
                        currentList.RemoveAt(idx - 1);
                    }
                }

                for (int i = 0; i < updatedList.Count; i++)
                {
                    PatientData data = currentList.ElementAtOrDefault(i);

                    if (i < updatedList.Count)
                    {
                        if (data != null)
                        {
                            data.CopyData(updatedList.ElementAt(i));
                        }
                        else
                        {
                            currentList.Add(updatedList.ElementAt(i));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Warning, Properties.Resources.CRIClient_ModuleName,
                    "Failed to Merge PatientsData List. \ncurrentList.Count = " + currentList.Count.ToString() + "\nupdatedList.Count = " + updatedList.Count.ToString(), ex);
            }
        }
    }
}

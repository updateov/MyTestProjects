using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Globalization;
using PeriGen.Patterns.Research.SQLHelper;
using PeriGen.Patterns.Engine.Data;

namespace PeriGen.Patterns.Research.OfflineDetection
{
	static class DataUtils
	{
		/// <summary>
		/// Check if the database exists
		/// </summary>
		/// <returns></returns>
		public static bool CheckIfDatabaseExist()
		{
			using (PeriGen.Patterns.Research.SQLHelper.PatternsDataContext db = new PeriGen.Patterns.Research.SQLHelper.PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				return db.DatabaseExists();
			}
		}

		/// <summary>
		/// Check if the database exists
		/// </summary>
		public static void CreateDatabase()
		{
			using (PeriGen.Patterns.Research.SQLHelper.PatternsDataContext db = new PeriGen.Patterns.Research.SQLHelper.PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				db.CreateDatabase();
				db.ExecuteCommand("CREATE TRIGGER PatientInsert ON Patients AFTER INSERT\nAS\nBEGIN\nSET NOCOUNT ON;\nUPDATE Patients SET Created = CURRENT_TIMESTAMP WHERE PatientID IN (SELECT PatientID FROM inserted);\nEND\n");
				db.ExecuteCommand("CREATE TRIGGER PatientUpdate ON Patients AFTER INSERT,UPDATE\nAS\nBEGIN\nSET NOCOUNT ON;\nUPDATE Patients SET LastModified = CURRENT_TIMESTAMP WHERE PatientID IN (SELECT PatientID FROM inserted);\nEND\n");
				db.ExecuteCommand("CREATE TRIGGER UserActionUpdate ON UserActions AFTER INSERT,UPDATE\nAS\nBEGIN\nSET NOCOUNT ON;\nUPDATE UserActions SET PerformTime = CURRENT_TIMESTAMP WHERE ActionID IN (SELECT ActionID FROM inserted);\nEND");
			}
		}

		/// <summary>
		/// Remove the given patient from the DB
		/// </summary>
		/// <param name="patient_id"></param>
		public static void PurgePatient(string patient_id)
		{
			using (PeriGen.Patterns.Research.SQLHelper.PatternsDataContext db = new PeriGen.Patterns.Research.SQLHelper.PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
            {
                // First make sure the visit exists	
                var patient = (from p in db.Patients where p.PatientKey == patient_id select p).FirstOrDefault();
                if (patient != null)
                {
                    db.UserActions.DeleteAllOnSubmit(patient.UserActions);
                    db.Artifacts.DeleteAllOnSubmit(patient.Artifacts);
                    db.Tracings.DeleteAllOnSubmit(patient.Tracings);
                    db.Patients.DeleteOnSubmit(patient);
                    db.SubmitChanges();
                }
            }
		}

		/// <summary>
		/// Save the given data in the SQL DB
		/// </summary>
		/// <param name="patient_id"></param>
		/// <param name="blocks"></param>
		/// <param name="results"></param>
		public static void SaveData(string patient_id, IEnumerable<TracingBlock> blocks, IEnumerable<DetectionArtifact> results)
		{
			using (PeriGen.Patterns.Research.SQLHelper.PatternsDataContext db = new PeriGen.Patterns.Research.SQLHelper.PatternsDataContext(Properties.Settings.Default.DatabaseConnectionString))
			{
				SaveData(db, patient_id, blocks, results);
			}
		}

		/// <summary>
		/// Save the given data in the SQL DB
		/// </summary>
		/// <param name="db"></param>
		/// <param name="patient_id"></param>
		/// <param name="blocks"></param>
		/// <param name="results"></param>
		public static void SaveData(PeriGen.Patterns.Research.SQLHelper.PatternsDataContext db, string patient_id, IEnumerable<TracingBlock> blocks, IEnumerable<DetectionArtifact> results)
		{
			// First make sure the visit exists	
			var patient = (from p in db.Patients where p.PatientKey == patient_id select p).FirstOrDefault();
			if (patient == null)
			{
				patient = new PeriGen.Patterns.Research.SQLHelper.Patient();
				patient.PatientKey = patient_id;

				db.Patients.InsertOnSubmit(patient);
			}

			// Make sure the tracings are merged
			var tracings = TracingBlock.Merge(blocks, 30).OrderBy(b => b.Start);

			// Append the results
			foreach (var result in results)
			{
				patient.Artifacts.Add(result.ToDatabaseDetectionArtifact());
			}

			// Append the tracing
			foreach (var block in tracings)
			{
				PeriGen.Patterns.Research.SQLHelper.Tracing tracing = new PeriGen.Patterns.Research.SQLHelper.Tracing();
				tracing.StartTime = block.Start;

				// Do not store pure no data fhr1
				if (block.HRs.Any(b => b != 255 && b != 0))
				{
					tracing.SignalHR1 = new System.Data.Linq.Binary(block.HRs.ToArray());
				}

				// Do not store pure no data up
				if (block.UPs.Any(b => b != 255 && b != 0))
				{
					tracing.SignalUP = new System.Data.Linq.Binary(block.UPs.ToArray());
				}

				if ((tracing.SignalHR1 != null) || (tracing.SignalUP != null))
				{
					patient.Tracings.Add(tracing);
				}
			}

			db.SubmitChanges();
		}
	}
}

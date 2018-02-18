using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel.DALs
{
    public static class ArtifactCountersExportedModelFactDAL
    {

        #region SaveItems

        public static void SaveItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            #region SaveSQL query STR
            string SaveSQL = @"INSERT OR REPLACE  into Fact_ArtifactCountersExported 
                                (
	                                EpisodeId, 
	                                ExportId,
                                    IntervalId,
                                    SampleFromDate,
                                    SampleToDate,
                                    ExportedDate,
                                    IntervalDuration,
                                    Category,
                                    Comment,
                                    MeanContractionInterval,
                                    NumOfContractions,
                                    NumOfLongContractions,
                                    MeanMontevideoUnits,
                                    MeanBaseline,
                                    MeanBaselineVariability,
                                    NumOfAccelerations,
                                    NumOfDecelerations,
                                    NumOfEarlyDecelerations,
                                    NumOfVariableDecelerations,
                                    NumOfLateDecelerations,
                                    NumOfProlongedDecelerations,
                                    NumOfOtherDecelerations,
                                    DateInserted,
                                    LoginName,
                                    ContractionIntervalRange,
                                    ContractionDurationRange,
                                    ContractionIntensityRange,
                                    OriginalContractionDurationRange,
                                    OriginalContractionIntensityRange
                                )";
            string ValueSQL = @"  SELECT 
                                    '{0}'  AS EpisodeId,
                                    '{1}'  AS ExportId,
                                    '{2}'  AS IntervalId,
	                                '{3}'  AS SampleFromDate,
	                                '{4}'  AS SampleToDate,
	                                '{5}'  AS ExportedDate,
	                                '{6}'  AS IntervalDuration,
                                    '{7}'  AS Category,
                                    '{8}'  AS Comment,
	                                '{9}'  AS MeanContractionInterval,
	                                '{10}' AS NumOfContractions,
	                                '{11}' AS NumOfLongContractions,
	                                '{12}' AS MeanMontevideoUnits,
	                                '{13}' AS MeanBaseline,
	                                '{14}' AS MeanBaselineVariability,
	                                '{15}' AS NumOfAccelerations,
	                                '{16}' AS NumOfDecelerations,
	                                '{17}' AS NumOfEarlyDecelerations,
	                                '{18}' AS NumOfVariableDecelerations,
                                    '{19}' AS NumOfLateDecelerations,
                                    '{20}' AS NumOfProlongedDecelerations,
                                    '{21}' AS NumOfOtherDecelerations,
                                    '{22}' AS DateInserted,
                                    '{23}' AS LoginName,
                                    '{24}' AS ContractionIntervalRange,
                                    '{24}' AS ContractionDurationRange,
                                    '{24}' AS ContractionIntensityRange,
                                    '{24}' AS OriginalContractionDurationRange,
                                    '{24}' AS OriginalContractionIntensityRange
                                  ";
            #endregion SaveSQL query STR
            string resultsToExecute = null;
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
                {
                    var results = from item in episode.ArtifactCountersExportedList
                                  where (item.ConceptNumber == -102100) //IntervalDuration
                                  let MeanContractionInterval = (from item1 in episode.ArtifactCountersExportedList
                                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102101
                                                                 select item1.ConceptValue).FirstOrDefault()
                                  let NumOfContractions = (from item1 in episode.ArtifactCountersExportedList
                                                           where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102102
                                                           select item1.ConceptValue).FirstOrDefault()
                                  let NumOfLongContractions = (from item1 in episode.ArtifactCountersExportedList
                                                               where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102103
                                                               select item1.ConceptValue).FirstOrDefault()
                                  let MeanMontevideoUnits = (from item1 in episode.ArtifactCountersExportedList
                                                             where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102104
                                                             select item1.ConceptValue).FirstOrDefault()
                                  let Comment = (from item1 in episode.ArtifactCountersExportedList
                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102114
                                                 select item1.ConceptValue).FirstOrDefault()


                                  let MeanBaseline = (from item1 in episode.ArtifactCountersExportedList
                                                      where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102105
                                                      select item1.ConceptValue).FirstOrDefault()
                                  let MeanBaselineVariability = (from item1 in episode.ArtifactCountersExportedList
                                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102106
                                                                 select item1.ConceptValue).FirstOrDefault()
                                  let NumOfAccelerations = (from item1 in episode.ArtifactCountersExportedList
                                                            where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102107
                                                            select item1.ConceptValue).FirstOrDefault()
                                  let NumOfDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                            where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102108
                                                            select item1.ConceptValue).FirstOrDefault()
                                  let NumOfEarlyDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102109
                                                                 select item1.ConceptValue).FirstOrDefault()
                                  let NumOfVariableDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                                    where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102110
                                                                    select item1.ConceptValue).FirstOrDefault()
                                  let NumOfLateDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                                where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102111
                                                                select item1.ConceptValue).FirstOrDefault()
                                  let NumOfProlongedDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                                     where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102112
                                                                     select item1.ConceptValue).FirstOrDefault()
                                  let NumOfOtherDecelerations = (from item1 in episode.ArtifactCountersExportedList
                                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102113
                                                                 select item1.ConceptValue).FirstOrDefault()
                                  let Category = (from item1 in episode.ArtifactCountersExportedList
                                                  where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == -102013
                                                  select item1.ConceptValue).FirstOrDefault()

                                  let ContractionIntervalRange = (from item1 in episode.ArtifactCountersExportedList
                                                                 where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == 9901731
                                                                  select item1.ConceptValue).FirstOrDefault()

                                  let ContractionDurationRange = (from item1 in episode.ArtifactCountersExportedList
                                                                  where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == 9901731
                                                                  select item1.ConceptValue).FirstOrDefault()

                                  let ContractionIntensityRange = (from item1 in episode.ArtifactCountersExportedList
                                                                  where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == 9901731
                                                                  select item1.ConceptValue).FirstOrDefault()

                                  let OriginalContractionDurationRange = (from item1 in episode.ArtifactCountersExportedList
                                                                   where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == 9901731
                                                                   select item1.ConceptValue).FirstOrDefault()

                                  let OriginalContractionIntensityRange = (from item1 in episode.ArtifactCountersExportedList
                                                                          where item1.IntervalDuration == item.IntervalDuration && item1.ConceptNumber == 9901731
                                                                          select item1.ConceptValue).FirstOrDefault()


                                  select (string.Format(ValueSQL,
                                                           episode.EpisodeId,
                                                           item.ExportId,
                                                           item.IntervalId,
                                                           item.SampleFromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.SampleToDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.ExportedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.IntervalDuration,
                                                           Category,
                                                           Comment,
                                                           MeanContractionInterval,
                                                           NumOfContractions,
                                                           NumOfLongContractions,
                                                           MeanMontevideoUnits,
                                                           MeanBaseline,
                                                           MeanBaselineVariability,
                                                           NumOfAccelerations,
                                                           NumOfDecelerations,
                                                           NumOfEarlyDecelerations,
                                                           NumOfVariableDecelerations,
                                                           NumOfLateDecelerations,
                                                           NumOfProlongedDecelerations,
                                                           NumOfOtherDecelerations,
                                                           ((item.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : item.DateInserted).ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.LoginName,
                                                           item.ContractionIntervalRange,
                                                           item.ContractionDurationRange,
                                                           item.ContractionIntensityRange,
                                                           item.OriginalContractionDurationRange,
                                                           item.OriginalContractionIntensityRange
                                                        )
                                           );

                    while (results.Count() > 400)
                    {
                        resultsToExecute = SaveSQL + String.Join(" UNION ALL ", results.Take(400));
                        cmd.CommandText = resultsToExecute;
                        cmd.ExecuteNonQuery();
                        results = results.Skip(400);
                    }

                    if (results.Count() > 0)
                    {
                        resultsToExecute = SaveSQL + String.Join(" UNION ALL ", results);
                        cmd.CommandText = resultsToExecute;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Saving to SQLite{0}; Query:{1}", ex, resultsToExecute));
            }

        }
        #endregion SaveItems
       
    }
}

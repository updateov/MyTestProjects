alter table Episodes add 	LastMergeId INT NOT NULL  default(-1);

alter table Episodes add 	LastMergeTime DATETIME NOT NULL  default('0001-01-01 00:00:00');


create unique index IX_Episodes_EpisodeGuid on Episodes(EpisodeGuid);

insert into ConceptTypes 
select 9, 'ComboMultiValue';

delete from ConceptNumberToColumnMapping;

insert into ConceptNumberToColumnMapping (ConceptNumber, ObjectOfCare, ColumnName, ConceptTypeId, Comments, DateUpdated, DateInserted)
select -102100, 0, 'IntervalDuration', 1, '', '2015-01-01', '2015-01-01' UNION ALL	
select  9901731,0, 'MeanContractionInterval', 2, '', '2015-01-01', '2015-01-01' UNION ALL	
select -102101, 0, 'ContractionIntervalRange', 3, '', '2015-01-01', '2015-01-01' UNION ALL	
select -102102, 0, 'NumOfContractions', 1, '', '2015-01-01', '2015-01-01' UNION ALL	
select -102103, 0, 'NumOfLongContractions', 1, '', '2015-01-01', '2015-01-01' UNION ALL
select -101395, 0, 'MeanMontevideoUnits', 1, '', '2015-01-01', '2015-01-01' UNION ALL
select -101385, 1, 'MeanBaseline', 1, '', '2015-01-01', '2015-01-01' UNION ALL
select -102122, 1, '', 4, '', '2015-01-01', '2015-01-01' UNION ALL
select -102123, 1, '', 4, '', '2015-01-01', '2015-01-01' UNION ALL
select -101420, 1, 'MeanBaselineVariability', 5, '', '2015-01-01', '2015-01-01' UNION ALL
select -101417, 1, 'NumOfAccelerations', 5, '', '2015-01-01', '2015-01-01' UNION ALL
select -101418, 1, 'NumOfDecelerations', 7, '', '2015-01-01', '2015-01-01' UNION ALL
select -102124, 1, '', 4, '', '2015-01-01', '2015-01-01' UNION ALL
select -102013, 1, '', 4, '', '2015-01-01', '2015-01-01' UNION ALL	
select -102125, 1, '', 4, '', '2015-01-01', '2015-01-01' UNION ALL
select -102114, 0, '', 3, '', '2015-01-01', '2015-01-01' UNION ALL
select 9901382, 1, 'NumOfEarlyDecelerations', 8, '', '2015-01-01', '2015-01-01' UNION ALL		
select 9900798, 1, 'NumOfVariableDecelerations', 8, '', '2015-01-01', '2015-01-01' UNION ALL	
select 9901383, 1, 'NumOfLateDecelerations', 8, '', '2015-01-01', '2015-01-01' UNION ALL		
select 9901381, 1, 'NumOfProlongedDecelerations', 8, '', '2015-01-01', '2015-01-01' UNION ALL 
select -100451, 1, 'NumOfOtherDecelerations', 8, '', '2015-01-01', '2015-01-01'  UNION ALL
select -101392, 0, 'ContractionDurationRange', 3, '', '2015-01-01', '2015-01-01' UNION ALL	
select -101393, 0, 'ContractionIntensityRange', 3, '', '2015-01-01', '2015-01-01' UNION ALL
select  9901732,0, 'OriginalContractionDurationRange', 3, '', '2015-01-01', '2015-01-01' UNION ALL
select  9901733,0, 'OriginalContractionIntensityRange', 3, '', '2015-01-01', '2015-01-01';
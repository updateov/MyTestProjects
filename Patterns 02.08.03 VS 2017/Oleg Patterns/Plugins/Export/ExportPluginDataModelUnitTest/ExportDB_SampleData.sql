
Select * from Episodes;
Select * from ArtifactCounters;
Select * from ArtifactCountersExported;

INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (1, 'Key1', 15, 0, 4, 1, 'E77808DB-AF13-4EF8-806C-737CD50C579F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');
INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (2, 'Key2', 15, 0, 4, 1, 'E77808DB-AF13-4EF8-806C-737CD50C679F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');
INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (3, 'Key3', 15, 0, 4, 1, 'E77808DB-AF13-4EF8-806C-737CD50C779F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');
INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (4, 'Key4', 15, 0, 4, -1, 'E77808DB-AF13-4EF8-806C-737CD50C879F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');
INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (5, 'Key5', 15, 0, 4, 14, 'E77808DB-AF13-4EF8-806C-737CD50C979F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');
INSERT OR REPLACE INTO Episodes (EpisodeId, VisitKey, IntervalDuration, EpisodeStatusId, TotalNumOfCountersIntervals, LastMergeId, EpisodeGuid, DateUpdated, DateInserted) VALUES (6, 'Key6', 30, 0, 4, 15, 'E77808DB-AF13-4EF8-806C-737CD50C109F', '2015-01-01 00:00:00', '2015-01-01 00:00:00');

---------------------------------------------------------------ArtifactCounters ----------------------------------------------------------

insert into ArtifactCounters(EpisodeId, IntervalId, ObjectOfCare, ConceptNumber, SampleFromDate, SampleToDate, IntervalDuration, ConceptValue, IsNotApplicable, DateInserted)
select 
    1 as EpisodeId, 
    1 as IntervalId, 
    ObjectOfCare, 
    ConceptNumber, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    15 as IntervalDuration, 
    null as ConceptValue, 
	0 as IsNotApplicable,
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL 
select 
    1 as EpisodeId, 
    2 as IntervalId, 
    ObjectOfCare, 
    ConceptNumber, 
    '2015-01-01 12:45:00' SampleFromDate, 
    '2015-01-01 13:00:00'SampleToDate, 
    15 as IntervalDuration, 
    1 as ConceptValue, --in order to have diffrent values 
	0 as IsNotApplicable,
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL 
select 
    2 as EpisodeId, 
    1 as IntervalId, 
    ObjectOfCare, 
    ConceptNumber, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    15 as IntervalDuration, 
    2 as ConceptValue, --in order to have diffrent values 
	0 as IsNotApplicable,
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL 
select 
    3 as EpisodeId, 
    1 as IntervalId, 
    ObjectOfCare, 
    ConceptNumber, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    15 as IntervalDuration, 
    77 as ConceptValue, 
	0 as IsNotApplicable,
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping;


--------------------------------------------ArtifactCountersExported-------------------------------------------------------------
insert into ArtifactCountersExported(EpisodeId, ExportId, ObjectOfCare, ConceptNumber, IntervalId, SampleFromDate, SampleToDate, ExportedDate, IntervalDuration, ConceptValue, CalulatedValue, IsStikeOut, LoginName, DateInserted)
select 
    1 as EpisodeId, 
    1 as ExportId, 
    ObjectOfCare, 
    ConceptNumber, 
    1 as IntervalId, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    '2015-01-01 13:30:00'ExportedDate, 
    15 as IntervalDuration, 
    3 as ConceptValue, 
    30 as CalulatedValue,  
	0 as IsStikeOut,
    'aa' as LoginName, 
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL
select 
    1 as EpisodeId, 
    2 as ExportId, 
    ObjectOfCare, 
    ConceptNumber, 
    1 as IntervalId, 
    '2015-01-01 12:45:00' SampleFromDate, 
    '2015-01-01 13:00:00'SampleToDate, 
    '2015-01-01 13:30:00'ExportedDate, 
    15 as IntervalDuration, 
    4 as ConceptValue,  
    40 as CalulatedValue, 
	0 as IsStikeOut,
    'aa' as LoginName, 
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL
select 
    2 as EpisodeId, 
    1 as ExportId, 
    ObjectOfCare, 
    ConceptNumber, 
    2 as IntervalId, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    '2015-01-01 13:30:00'ExportedDate, 
    15 as IntervalDuration, 
    5 as ConceptValue,  
    50 as CalulatedValue, 
	0 as IsStikeOut,
    'aa' as LoginName, 
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping
UNION ALL
select 
    3 as EpisodeId, 
    1 as ExportId, 
    ObjectOfCare, 
    ConceptNumber, 
    3 as IntervalId, 
    '2015-01-01 12:30:00' SampleFromDate, 
    '2015-01-01 12:45:00'SampleToDate, 
    '2015-01-01 13:30:00'ExportedDate, 
    15 as IntervalDuration, 
    null as ConceptValue, 
    null as CalulatedValue, 
	0 as IsStikeOut,
    'aa' as LoginName, 
    '2015-01-01 13:30:00' DateInserted
from ConceptNumberToColumnMapping;





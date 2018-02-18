DROP TABLE IF EXISTS Episodes ;
DROP TABLE IF EXISTS ArtifactCounters;
DROP TABLE IF EXISTS ArtifactCountersExported;
DROP TABLE IF EXISTS ConceptNumberToColumnMapping;
DROP TABLE IF EXISTS ConceptTypes;

CREATE TABLE Episodes 
(
	EpisodeId					INT NOT NULL,
	VisitKey					NVARCHAR(100) NOT NULL,
	IntervalDuration			INT NOT NULL,
	EpisodeStatusId				SMALLINT NOT NULL,
	TotalNumOfCountersIntervals INT NOT NULL,
	LastMergeId					INT NOT NULL,
	LastMergeTime				DATETIME NOT NULL,
	EpisodeGuid					NVARCHAR(36) NOT  NULL default ('00000000-0000-0000-0000-000000000000'),
	DateUpdated					DATETIME NOT NULL,
	DateInserted				DATETIME NOT NULL,
	CONSTRAINT [PK_Episodes] PRIMARY KEY(EpisodeId)
);
create unique index IX_Episodes_EpisodeGuid on Episodes(EpisodeGuid);

CREATE TABLE ArtifactCounters
(
	EpisodeId					INT NOT NULL,
	IntervalId					INT NOT NULL,
	ObjectOfCare				INT NOT NULL,
	ConceptNumber				INT NOT NULL,
	SampleFromDate				DATETIME NOT NULL,
	SampleToDate				DATETIME NOT NULL,
	IntervalDuration			INT NOT NULL,
	ConceptValue				VARCHAR(1000) NULL,
	IsNotApplicable				INT NOT NULL,
	DateInserted				DATETIME NOT NULL,
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (EpisodeId, IntervalId, ObjectOfCare, ConceptNumber),
	CONSTRAINT FK_EpisodeIndicators_Episodes FOREIGN KEY(EpisodeId )REFERENCES Episodes (EpisodeId)
) WITHOUT ROWID;

CREATE TABLE ArtifactCountersExported
(
	EpisodeId					INT NOT NULL,
	ExportId					INT NOT NULL,
	ObjectOfCare				INT NOT NULL,
	ConceptNumber				INT NOT NULL,
	IntervalId					INT NOT NULL, --the original intervalID from ArtifactCounters
	SampleFromDate				DATETIME NOT NULL,
	SampleToDate				DATETIME NOT NULL,
	ExportedDate				DATETIME NOT NULL,
	IntervalDuration			INT NOT NULL,
	ConceptValue				VARCHAR(1000) NULL,
	CalulatedValue				VARCHAR(1000) NULL,
	IsStikeOut					INT NOT NULL,
	LoginName					NVARCHAR(50) NOT NULL,
	DateInserted				DATETIME NOT NULL,
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (EpisodeId, ExportId, ObjectOfCare, ConceptNumber),
	CONSTRAINT FK_EpisodeExportedIndicators_Episodes FOREIGN KEY(EpisodeId )REFERENCES Episodes (EpisodeId)
) WITHOUT ROWID;

CREATE TABLE ConceptTypes
(
	ConceptTypeId	INT NOT NULL,
	Name			NVARCHAR (255) NULL,
	CONSTRAINT [PK_ConceptTypes] PRIMARY KEY (ConceptTypeId)
) WITHOUT ROWID;

CREATE TABLE ConceptNumberToColumnMapping
(
	ConceptNumber	INT NOT NULL,
	ObjectOfCare	INT NOT NULL,
	ColumnName		NVARCHAR (255) NULL,
	ConceptTypeId	INT NOT NULL,
	Comments		VARCHAR (255) NULL,
	DateUpdated		DATETIME NOT NULL,
	DateInserted	DATETIME NOT NULL,
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (ConceptNumber, ObjectOfCare),
	CONSTRAINT FK_ConceptNumberToColumnMapping_ConceptTypes FOREIGN KEY(ConceptTypeId )REFERENCES ConceptTypes (ConceptTypeId)
) WITHOUT ROWID;



insert into ConceptTypes (ConceptTypeId, Name)
select 1, 'Int' UNION ALL
select 2, 'Double' UNION ALL
select 3, 'String' UNION ALL
select 4, 'Combo' UNION ALL
select 5, 'CalculatedCombo' UNION ALL
select 6, 'CheckboxGroup' UNION ALL
select 7, 'CalculatedCheckboxGroup' UNION ALL
select 8, 'CalculatedCheckboxGroupItem' UNION ALL
select 9, 'ComboMultiValue';


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
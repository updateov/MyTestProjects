DROP TABLE IF EXISTS Episodes ;
DROP TABLE IF EXISTS ArtifactCounters;
DROP TABLE IF EXISTS ArtifactCountersExported;

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

CREATE TABLE EpisodeStatuses (
	EpisodeStatusId	INT NOT NULL,
	Name			NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_EpisodeStatuses] PRIMARY KEY (EpisodeStatusId)
);

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


CREATE TABLE Fact_ArtifactCountersExported
(
	EpisodeId					INT NOT NULL,
	ExportId					INT NOT NULL,
	IntervalId					INT NOT NULL,
	SampleFromDate				DATETIME NOT NULL,
	SampleToDate				DATETIME NOT NULL,
	ExportedDate				DATETIME NOT NULL,
	IntervalDuration			INT NOT NULL,
	Category					NVARCHAR(100) NULL,
	Comment						NVARCHAR(1000) NULL,
	MeanContractionInterval		FLOAT NULL,
	NumOfContractions			INT NULL,
	NumOfLongContractions		INT NULL,
	MeanMontevideoUnits			INT NULL,
	MeanBaseline				INT NULL,
	MeanBaselineVariability		float NULL,
	NumOfAccelerations			INT NULL,
	NumOfDecelerations			INT NULL,
	NumOfEarlyDecelerations		INT NULL,
	NumOfVariableDecelerations	INT NULL,
	NumOfLateDecelerations		INT NULL,
	NumOfProlongedDecelerations INT NULL,
	NumOfOtherDecelerations		INT NULL,
	DateInserted				DATETIME NOT NULL,
	LoginName					NVARCHAR(50) NOT NULL,
	ContractionIntervalRange    NVARCHAR(100) NULL,
	ContractionDurationRange	NVARCHAR(100) NULL,
	ContractionIntensityRange	NVARCHAR(100) NULL,
	OriginalContractionDurationRange NVARCHAR(100) NULL,
	OriginalContractionIntensityRange NVARCHAR(100) NULL,
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (EpisodeId, ExportId),
	CONSTRAINT FK_EpisodeExportedIndicators_Episodes FOREIGN KEY(EpisodeId )REFERENCES Episodes (EpisodeId)
) WITHOUT ROWID;


INSERT INTO EpisodeStatuses (EpisodeStatusId, Name) 
select 0, 'Unknown' union all 
select 1, 'Admitted' union all 
select 2, 'Discharged' union all 
select 3, 'ToHistory';

DROP TABLE IF EXISTS Episodes ;
DROP TABLE IF EXISTS [Contractilities];
DROP TABLE IF EXISTS Persistencies;

CREATE TABLE Episodes (
	EpisodeId		INT NOT NULL,
	EpisodeKey		NVARCHAR(100) NULL,
	GA				NVARCHAR(100) NULL,
	Fetuses			INT NULL,
	StatusId		SMALLINT NULL, 
	Tracing			INT NULL,
	Artifact		INT NULL,
	Contractility	INT NULL,
	Action			INT NULL,
	Incremental		INT NULL,
	Serveruid		NVARCHAR(255) NULL,
	LastMergeId		INT NOT NULL,
	LastMergeTime   DATETIME NOT NULL,
	EpisodeGuid		NVARCHAR(36) NOT  NULL default ('00000000-0000-0000-0000-000000000000'),
	DateUpdated		DATETIME NOT NULL,
	DateInserted	DATETIME NOT NULL,
	CONSTRAINT [PK_Episodes] PRIMARY KEY(EpisodeId)
);

CREATE TABLE Contractilities (
	ContractilityId					INT NOT NULL,	
	EpisodeId						INT NOT NULL,
	StartDateComputed				DATETIME NOT NULL, --for reporting performance will hold (YYYY-MM-dd)
	StartTime						DATETIME NOT NULL,
	EndTime							DATETIME NOT NULL,
	Classification					SMALLINT NOT NULL,
	Variability						FLOAT NOT NULL,
	IsVariabilityForStatus			BIT NOT NULL,
	Accels							INT NOT NULL,
	IsAccelsForStatus				BIT NOT NULL,
	Contractions					INT NOT NULL,
	IsContractionsForStatus		BIT NOT NULL,
	LongContractions				INT NOT NULL,
	IsLongContractionsForStatus	BIT NOT NULL,
	LargeDeceles					INT NOT NULL,
	IsLargeDecelesForStatus			BIT NOT NULL,
	LateDecels						INT NOT NULL,
	IsLateDecelsForStatus			BIT NOT NULL,
	ProlongedDecels					INT NOT NULL,
	IsProlongedDecelsForStatus		BIT NOT NULL,
	DateUpdated						DATETIME NOT NULL,
	DateInserted					DATETIME NOT NULL,
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (ContractilityId, EpisodeId)
);

CREATE INDEX IX_Contractilities_EpisodeId ON Contractilities(EpisodeId);

CREATE TABLE Persistencies (
	PersistenceId					INT NOT NULL,
	EpisodeId						INT NOT NULL,
	StartDateComputed				DATETIME NOT NULL, --for reporting performance will hold (YYYY-MM-dd)
	StartTime						DATETIME NOT NULL,
	EndTime							DATETIME NOT NULL,
	State							SMALLINT NOT NULL,
	AcknowledgeDate					DATETIME NOT NULL,
	AcknowledgeUserType				NVARCHAR(100) NOT NULL,
	Variability						FLOAT NOT NULL,
	IsVariabilityForStatus			BIT NOT NULL,
	Accels							INT NOT NULL,
	IsAccelsForStatus				BIT NOT NULL,
	Contractions					INT NOT NULL,
	IsContractionsForStatus		BIT NOT NULL,
	LongContractions				INT NOT NULL,
	IsLongContractionsForStatus	BIT NOT NULL,
	LargeDeceles					INT NOT NULL,
	IsLargeDecelesForStatus			BIT NOT NULL,
	LateDecels						INT NOT NULL,
	IsLateDecelsForStatus			BIT NOT NULL,
	ProlongedDecels					INT NOT NULL,
	IsProlongedDecelsForStatus		BIT NOT NULL,
	DateUpdated						DATETIME NOT NULL,
	DateInserted					DATETIME NOT NULL,
	CONSTRAINT [PK_Persistencies] PRIMARY KEY (PersistenceId, EpisodeId)
);
CREATE INDEX IX_Persistencies_EpisodeId ON Persistencies(EpisodeId);
CREATE INDEX IX_Persistencies_StartDateComputed_State_AcknowledgeDate_AcknowledgeUserType ON "Persistencies"( StartDateComputed, State, AcknowledgeDate, AcknowledgeUserType);
CREATE INDEX IX_Persistencies_StartTime ON "Persistencies"( StartTime);


CREATE TABLE EpisodeStatuses (
	EpisodeStatusId	INT NOT NULL,
	Name			NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_EpisodeStatuses] PRIMARY KEY (EpisodeStatusId)
);
CREATE TABLE ContractilitiesClassifications (
	ClassificationId	INT NOT NULL,
	Name				NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_ContractilitiesClassifications] PRIMARY KEY (ClassificationId)
);
CREATE TABLE PersistenceStates (
	PersistenceStateId	INT NOT NULL,
	Name				NVARCHAR(100) NOT NULL,
	CONSTRAINT [PK_PersistenceStates] PRIMARY KEY (PersistenceStateId)
);


INSERT INTO EpisodeStatuses (EpisodeStatusId, Name) 
select 0, 'Unknown' union all 
select 1, 'Admitted' union all 
select 2, 'Discharged' union all 
select 3, 'ToHistory';

INSERT INTO ContractilitiesClassifications (ClassificationId, Name) 
select -1, 'Unknown' union all 
select 0, 'Normal' union all 
select 1, 'Alert' union all 
select 2, 'Danger';

INSERT INTO PersistenceStates (PersistenceStateId, Name) 
select 0, 'Unknown' union all 
select 1, 'Negative' union all 
select 2, 'Positive';



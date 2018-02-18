DROP TABLE IF EXISTS Episodes ;
DROP TABLE IF EXISTS [Contractilities];
DROP TABLE IF EXISTS Persistencies;
DROP TABLE IF EXISTS DBUpdate;
DROP TABLE IF EXISTS AppVersionHistory;

CREATE TABLE Episodes (
	EpisodeId		INT NOT NULL,
	EpisodeKey		NVARCHAR(100) NOT NULL,
	GA				NVARCHAR(100) NOT NULL,
	Fetuses			INT NOT NULL,
	StatusId		SMALLINT NOT NULL, 
	Tracing			INT NOT NULL,
	Artifact		INT NOT NULL,
	Contractility	INT NOT NULL,
	Action			INT NOT NULL,
	Incremental		INT NOT  NULL,
	Serveruid		NVARCHAR(255) NOT NULL,
	LastMergeId		INT NOT NULL,
	LastMergeTime	DATETIME NOT NULL,
	EpisodeGuid		NVARCHAR(36) NOT  NULL default ('00000000-0000-0000-0000-000000000000'),
	DateUpdated		DATETIME NOT NULL,
	DateInserted	DATETIME NOT NULL,
	CONSTRAINT [PK_Episodes] PRIMARY KEY(EpisodeId)
);

CREATE TABLE Contractilities (
	ContractilityId					INT NOT NULL,	
	EpisodeId						INT NOT NULL,
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
	CONSTRAINT [PK_Contractilities] PRIMARY KEY (ContractilityId, EpisodeId),
	CONSTRAINT FK_Persistence_Episodes FOREIGN KEY(EpisodeId )REFERENCES Episodes (EpisodeId)
);

CREATE INDEX IX_Contractilities_EpisodeId ON Contractilities(EpisodeId);

CREATE TABLE Persistencies (
	PersistenceId					INT NOT NULL,
	EpisodeId						INT NOT NULL,
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
	CONSTRAINT [PK_Persistencies] PRIMARY KEY (PersistenceId, EpisodeId),
	CONSTRAINT FK_Persistence_Episodes FOREIGN KEY(EpisodeId )REFERENCES Episodes (EpisodeId)
);
CREATE INDEX IX_Persistencies_EpisodeId ON Persistencies(EpisodeId);

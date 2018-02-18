
create unique index IX_Episodes_EpisodeGuid on Episodes(EpisodeGuid);

alter table Fact_ArtifactCountersExported add ContractionIntervalRange    NVARCHAR(100) NULL;
alter table Fact_ArtifactCountersExported add ContractionDurationRange	NVARCHAR(100) NULL;
alter table Fact_ArtifactCountersExported add ContractionIntensityRange	NVARCHAR(100) NULL;
alter table Fact_ArtifactCountersExported add OriginalContractionDurationRange NVARCHAR(100) NULL;
alter table Fact_ArtifactCountersExported add OriginalContractionIntensityRange NVARCHAR(100) NULL;
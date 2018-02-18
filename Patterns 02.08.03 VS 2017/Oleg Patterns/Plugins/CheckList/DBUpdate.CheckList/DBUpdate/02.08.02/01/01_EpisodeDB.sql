alter table Episodes add 	LastMergeId INT NOT NULL  default(-1);

alter table Episodes add 	LastMergeTime DATETIME NOT NULL  default('0001-01-01 00:00:00');


create unique index IX_Episodes_EpisodeGuid on Episodes(EpisodeGuid);

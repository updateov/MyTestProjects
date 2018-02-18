SELECT "EpisodeId"
	,"StartTime"
	,"EndTime"
	,"psi"."Name" [State]
	,"AcknowledgeDate"
	,"AcknowledgeUserType"
	,"Contractions"
	,"LongContractions"
	,"Accels"
	,"LargeDeceles"
	,"LateDecels"
	,"ProlongedDecels"
	,"Variability"
	,"IsContractionsForStatus"
	,"IsLongContractionsForStatus"
	,"IsAccelsForStatus"
	,"IsLargeDecelesForStatus"
	,"IsLateDecelsForStatus"
	,"IsProlongedDecelsForStatus"
	,"IsVariabilityForStatus"
	,"PersistenceId"
FROM "Persistencies" p
INNER JOIN PersistenceStates psi ON psi.PersistenceStateId = p.STATE
WHERE p.StartTime BETWEEN '!startDateRange!' AND '!endDateRange!';
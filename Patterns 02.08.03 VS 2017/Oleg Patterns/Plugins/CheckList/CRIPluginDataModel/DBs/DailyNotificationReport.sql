SELECT 	
		strftime('%m-%d-%Y', TimeStamp) [DATE],
		AvgTime							[Average elapsed time (hh:mm:ss)],
		AckCount						[Reviewed],
		UnAckCount						[Non-Reviewed]
FROM (
	SELECT t1.Timestamp
		,t1.AvgTime
		,ackCount
		,t2.UnAckCount
	FROM (
		SELECT TIME(AVG((strftime('%s', AcknowledgeDate) - strftime('%s', StartTime))), 'unixepoch') [AvgTime]
			,StartDateComputed [Timestamp]
			,count(*) [AckCount]
			FROM Persistencies p
			WHERE p.StartDateComputed BETWEEN '!startDateRange!' AND '!endDateRange!' 
				AND STATE = 2 
				AND AcknowledgeUserType <> 'SYSTEM'
				AND AcknowledgeDate <> '0001-01-01 00:00:00'
		GROUP BY StartDateComputed
		) t1
	LEFT JOIN (
		/*select all the nonreviewed rows*/
		SELECT StartDateComputed [Timestamp]
			,count(*) UnAckCount
		FROM Persistencies p
		WHERE p.StartDateComputed BETWEEN '!startDateRange!' AND '!endDateRange!' 
				AND STATE = 2	
				AND (AcknowledgeUserType = 'SYSTEM' OR AcknowledgeDate = '0001-01-01 00:00:00')
		GROUP BY StartDateComputed
		) t2 ON t1.Timestamp = t2.Timestamp
	
	UNION ALL
	/*select all the unacknowledged data, in days without any*/
	/*acknowledge at all (alternative implementation for*/
	/*cross outer join).*/
	SELECT StartDateComputed [Timestamp]
		,'-'
		,0
		,count(*)
	FROM Persistencies p1
	WHERE 
		p1.StartDateComputed BETWEEN '!startDateRange!' AND '!endDateRange!' 
		AND(AcknowledgeDate = '0001-01-01 00:00:00' OR AcknowledgeUserType = 'SYSTEM')
		AND NOT EXISTS (
			SELECT *
			FROM Persistencies p2
			WHERE
				p2.StartTime BETWEEN '!startDateRange!' AND '!endDateRange!' AND
				p2.StartDateComputed = p1.StartDateComputed
				AND AcknowledgeDate <> '0001-01-01 00:00:00'
			)
	GROUP BY StartDateComputed
	)
ORDER BY TimeStamp DESC
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
	exclude-result-prefixes="msxsl">
	<xsl:output method="html" indent="yes" version="4.0"/>
	<xsl:template match="Episodes">		
		<html>
			<head>
				<meta http-equiv="expires" CONTENT="-1" />
				<meta http-equiv="pragma" CONTENT="no-cache" />
				<meta http-equiv="cache-control" CONTENT="no-cache" />
			</head>
			<body>
				<center>
					<span style="font-weight: bold;font-size: large;font-family: Verdana, Geneva, Tahoma, sans-serif;color: #800000;">Episodes</span>
				</center>
				<br></br>
					<table width="100%" border="1">
					<tr style="background-color: #000080;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FFFFFF;font-weight: bold;	font-size: smaller;">
						<th>ID</th>
						<th>MRN</th>
						<th>Name</th>
						<th>Bed</th>
						<th>Unit</th>
						<th>Fetuses</th>
						<th>EDD</th>
						<th>Updated</th>
						<th>Last Tracing</th>
						<th>Status</th>
						<th>Recovering</th>
						<th>Merging</th>
					</tr>
						<xsl:for-each select="Episode">
							<xsl:call-template name="episode"/>
						</xsl:for-each>						
				</table>				
			</body>
		</html>
	</xsl:template>
	<xsl:template name="episode">
		<tr>
			<td>
				<xsl:value-of select="PatientUniqueId"/>
			</td>
			<td>
				<xsl:value-of select="Patient/Id"/>
			</td>
			<td>
				<xsl:value-of select="Patient/Name"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="Patient/Bed"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="Patient/Unit"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="Patient/Fetuses"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="Patient/EDD"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="Patient/LastUpdated"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="LastTracingDateTime"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="EpisodeStatus"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="IsInRecovery"/>
			</td>
			<td style="text-align: center;">
				<xsl:value-of select="IsMergeInProgress"/>
			</td>
		</tr>
	</xsl:template>
</xsl:stylesheet>

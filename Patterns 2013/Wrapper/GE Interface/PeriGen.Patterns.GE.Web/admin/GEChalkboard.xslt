<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	exclude-result-prefixes="msxsl">
	<xsl:output method="html" indent="yes" version="4.0"/>
	<xsl:template match="Episodes">
		<div>
			<center>
				<span style="font-weight: bold;font-size: 16px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #800000;">PeriGen PeriCALM® Patterns™ (GE) Chalkboard</span>
				<br />
				<span style="font-weight: bold;font-size: 14px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #000080;">
					Last refreshed:&#160;<xsl:value-of select="substring(substring-before(@LastRefreshed, 'T'),3)"/>&#160;<xsl:value-of select="substring-after(@LastRefreshed, 'T')"/>
				</span>
				<xsl:if test="@IsOffline='True'">
					<br />
					<br />
					<br />
					<span style="font-weight: bold;font-size: 22px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;">OBLink service cannot be reached.</span>
				</xsl:if>
			</center>
			<br/>
			<xsl:variable name="vIsOffLine" select="@IsOffline"/>
			<xsl:if test="$vIsOffLine='False'">
				<table width="100%">
					<tr style="background-color: #000080;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FFFFFF;font-weight: bold;	font-size: smaller;">
						<th>ID</th>
						<th>MRN</th>
						<th>Name</th>
						<th>Bed</th>
						<th>Fetus</th>
						<th>EDD</th>
						<th>Status</th>
						<th>Recovery</th>
						<th>Merging</th>
						<th>Conflict</th>
						<th>Created</th>
						<th>Monitored</th>
						<th>Realtime offset</th>
						<th>Database</th>
						<th></th>
					</tr>
					<xsl:for-each select="Episode">
						<xsl:call-template name="episode"/>
					</xsl:for-each>
				</table>
			</xsl:if>
		</div>
	</xsl:template>
	<xsl:template name="episode">
		<tr>
			<xsl:choose>
				<xsl:when test="position() mod 2 != 1">
					<xsl:attribute  name="style">background-color:#dddddd</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute  name="style">background-color:#e8edff</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<td>
				<xsl:value-of select="@PatientUniqueId"/>
			</td>
			<td>
				<xsl:variable name="testurl" select="'testpage.aspx?id='"/>
				<xsl:variable name="testmrn" select="Patient/@MRN"/>
				<a>
					<xsl:attribute name="href">
						<xsl:value-of select='concat($testurl, $testmrn)'/>
					</xsl:attribute>
					<xsl:value-of select="Patient/@MRN"/>
				</a>
			</td>
			<td>
				<xsl:value-of select="Patient/@Name"/>
			</td>
			<td>
				(<xsl:value-of select="Patient/@BedId"/>) <xsl:value-of select="Patient/@UnitName"/> - <xsl:value-of select="Patient/@BedName"/>
			</td>
			<td>
				<xsl:value-of select="Patient/@Fetuses"/>
			</td>
			<td>
				<xsl:value-of select="substring-before(substring(Patient/@EDD, 3), 'T')"/>
			</td>
			<td>
				<xsl:value-of select="@EpisodeStatus"/>
			</td>
			<td>
				<xsl:if test="@RecoveryInProgress='True'">
					<xsl:attribute  name="style">color:red</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@RecoveryInProgress"/>
			</td>
			<td>
				<xsl:if test="@MergeInProgress='True'">
					<xsl:attribute  name="style">color:red</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@MergeInProgress"/>
			</td>
			<td>
				<xsl:value-of select="@ReconciliationConflict"/>
				<xsl:if test="@ReconciliationConflict!=''">
					<xsl:attribute  name="style">color:red</xsl:attribute>
				</xsl:if>
			</td>
			<td>
				<xsl:value-of select="substring(substring-before(@Created, 'T'),3)"/>&#160;<xsl:value-of select="substring-after(@Created, 'T')"/>
			</td>
			<td>
				<xsl:value-of select="substring(substring-before(@LastMonitored, 'T'),3)"/>&#160;<xsl:value-of select="substring-after(@LastMonitored, 'T')"/>
			</td>
			<td>
				<xsl:if test="contains(@EpisodeStatus, 'Normal') and not(contains(@RealTimeLatency, 'Live'))">
					<xsl:attribute  name="style">color:red</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@RealTimeLatency"/>
			</td>
			<td>
				<xsl:value-of select="@DatabaseFile"/>
			</td>
			<td>
				<xsl:if test="@EpisodeStatus!='Closed'">
					<xsl:variable name="url" select="'gechalkboard.aspx?id='"/>
					<xsl:variable name="mrn" select="Patient/@MRN"/>
					<xsl:variable name="id" select="@PatientUniqueId"/>
					<xsl:variable name="quote">"</xsl:variable>
					<xsl:variable name="message" select='concat("return", " ", "confirm(",$quote,"Are you sure you want to close the episode with the MRN: ", $mrn, " ID: ", $id, "?", $quote,")")'/>
					<a>
						<xsl:attribute name="href">
							<xsl:value-of select='concat($url, $id)'/>
						</xsl:attribute>
						<xsl:attribute name="onclick">
							<xsl:value-of select="$message"/>
						</xsl:attribute>
						Close
					</a>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
</xsl:stylesheet>

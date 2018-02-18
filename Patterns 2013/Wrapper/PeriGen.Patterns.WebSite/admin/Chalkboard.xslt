<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:msxsl="urn:schemas-microsoft-com:xslt"
	exclude-result-prefixes="msxsl">
	<xsl:output method="html" indent="yes" version="4.0"/>
	<xsl:template match="Episodes">		
		<div>
			<center>
				<span style="font-weight: bold;font-size: 22px;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FEFFFF;">PeriGen PeriCALM® Patterns™ Chalkboard</span>
				<br />
				<span style="font-size: 16px;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #DCE6F1;">
					Last refreshed:&#160;<xsl:value-of select="substring(substring-before(@LastRefreshed, 'T'),3)"/>&#160;<xsl:value-of select="substring-after(@LastRefreshed, 'T')"/>
				</span>
				<xsl:if test="@IsOffline='True'">
					<br />
					<br />
					<br />
					<span style="font-weight: bold;font-size: 22px;font-family: 'Lucida Sans Unicode', 'Lucida Grande', Sans-Serif;color: #FF0000;">Service cannot be reached.</span>
				</xsl:if>
			</center>
			<br/>
			<xsl:variable name="vIsOffLine" select="@IsOffline"/>
			<xsl:variable name="varCurveEnabled" select="@CurveEnabled"/>
			<xsl:variable name="varPatternsEnabled" select="@PatternsEnabled"/>
			<xsl:if test="$vIsOffLine='False'">
				<table width="100%" style="clear:both;">
					<tr style="background-color: #4F81BD;font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;color: #FFFFFF;font-weight: bold;font-size: smaller;">
						<th>ID</th>
						<th>Visit Key</th>
						<th>MRN</th>
						<th>Account N&#176;</th>
						<th>Name</th>
						<th>Bed</th>
						<th>Recovery</th>
						<th>Created</th>
						<th>Realtime offset</th>
						<th>Database</th>
						<th>Status</th>
						<xsl:if test="contains($varPatternsEnabled, 'true')">
									<th>Patterns</th>
						</xsl:if>
						<xsl:if test="contains($varCurveEnabled, 'true')">
									<th>Curve</th>
						</xsl:if>
						<th>Action</th>
					</tr>
					<xsl:for-each select="Episode">
						<xsl:call-template name="episode">
							<xsl:with-param name="curveEnabled" select="$varCurveEnabled"/>
							<xsl:with-param name="patternsEnabled" select="$varPatternsEnabled"/>
						</xsl:call-template>
					</xsl:for-each>
				</table>
			</xsl:if>
		</div>
	</xsl:template>
	<xsl:template name="episode">
		<xsl:param name="curveEnabled" />
		<xsl:param name="patternsEnabled" />
		<tr>
			<xsl:choose>
				<xsl:when test="position() mod 2 != 1">
					<xsl:attribute  name="style">background-color:#DCE6F1</xsl:attribute>
					<xsl:attribute name="onmouseover">style.backgroundColor='#DCDCDC'</xsl:attribute>
					<xsl:attribute name="onmouseout">style.backgroundColor='#DCE6F1'</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute  name="style">background-color:#B8CCE4</xsl:attribute>
					<xsl:attribute name="onMouseOver">style.backgroundColor='#DCDCDC'</xsl:attribute>
					<xsl:attribute name="onMouseOut">style.backgroundColor='#B8CCE4'</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<td>
				<xsl:value-of select="@PatientUniqueId"/>
			</td>
			<td>
				<xsl:value-of select="Patient/@Key"/>
			</td>
			<td>
				<xsl:value-of select="Patient/@MRN"/>
			</td>
			<td>
				<xsl:value-of select="Patient/@AccountNo"/>
			</td>
			<td>
				<xsl:variable name="tempName" select="Patient/@FirstName"/>
				<xsl:variable name="tempLastName" select="Patient/@LastName"/>
				<xsl:value-of select='concat($tempLastName," ",$tempName)'/>
			</td>
			<td>
				<xsl:if test="contains(@EpisodeStatus, 'Admitted')">					
					<xsl:value-of select="Patient/@UnitName"/> - <xsl:value-of select="Patient/@BedName"/>
				</xsl:if>				
			</td>
			<td>
				<xsl:if test="@RecoveryInProgress='True'">
					<xsl:attribute  name="style">color:red;</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@RecoveryInProgress"/>
			</td>
			<td>
				<xsl:value-of select="substring(substring-before(@Created, 'T'),3)"/>&#160;<xsl:value-of select="substring-after(@Created, 'T')"/>
			</td>
			<td>
				<xsl:if test="contains(@RealTimeLatency, '(Late)')">
					<xsl:attribute name="style">color:red</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@RealTimeLatency"/>
			</td>
			<td>
				<xsl:value-of select="@DatabaseFile"/>
			</td>
			<td>
				<xsl:value-of select="@EpisodeStatus"/>
			</td>
			<xsl:if test="contains($patternsEnabled, 'true')">
				<td>
					<xsl:if test="@EpisodeStatus!='Closed' and @EpisodeStatus!='New' and @PatientUniqueId!=0">
						<xsl:variable name="url" select="'chalkboard.aspx?action=patterns'"/>
						<xsl:variable name="id" select="Patient/@Key"/>
						<xsl:variable name="quote">"</xsl:variable>
						<a>
							<xsl:attribute name="href">
								<xsl:value-of select='concat($url, "&amp;id=", $id)'/>
							</xsl:attribute>
							Patterns
						</a>
					</xsl:if>
				</td>
			</xsl:if>
			<xsl:if test="contains($curveEnabled, 'true')">
			<td>
				<xsl:if test="@EpisodeStatus!='Closed' and @EpisodeStatus!='New' and @PatientUniqueId!=0">
					<xsl:variable name="url" select="'chalkboard.aspx?action=curve'"/>
					<xsl:variable name="id" select="Patient/@Key"/>
					<xsl:variable name="quote">"</xsl:variable>
					<a>
						<xsl:attribute name="href">
							<xsl:value-of select='concat($url, "&amp;id=", $id)'/>
						</xsl:attribute>
						Curve
					</a>
				</xsl:if>
			</td>
			</xsl:if>
			<td>
				<xsl:if test="@EpisodeStatus!='Closed' and @EpisodeStatus!='New' and @PatientUniqueId!=0">
					<xsl:variable name="url" select="'chalkboard.aspx?id='"/>
					<xsl:variable name="mrn" select="Patient/@MRN"/>
					<xsl:variable name="id" select="@PatientUniqueId"/>
					<xsl:variable name="quote">"</xsl:variable>
					<xsl:variable name="message" select='concat("return", " ", "confirm(",$quote,"Are you sure you want to close the episode with the MRN: ", $mrn, " ID: ", $id, "?", $quote,")")'/>
					<a>
						<xsl:attribute name="href">
							<xsl:value-of select='concat($url, $id, "&amp;action=close")'/>
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

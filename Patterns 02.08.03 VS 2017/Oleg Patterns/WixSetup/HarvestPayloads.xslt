<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl wix"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">

  <xsl:output method="xml" indent="yes"/>

  <xsl:param name="BindVarName">BIND_VAR</xsl:param>
  <xsl:param name="BaseDir">BASE_DIR</xsl:param>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Convert ComponentGroup to PayloadGroup -->
  <xsl:template match="//wix:ComponentGroup">
    <wix:PayloadGroup>
      <xsl:attribute name="Id">
        <xsl:value-of select="./@Id"/>
      </xsl:attribute>

      <!-- Copy all File elements as Payload elements -->
      <xsl:for-each select="//wix:File">
      <wix:Payload>
        <xsl:attribute name="Id">
          <xsl:value-of select="./@Id"/>
        </xsl:attribute>
        <xsl:attribute name="SourceFile">
          <xsl:value-of select="concat('!(bindpath.', $BindVarName, ')', $BaseDir, '\', substring-after( ./@Source, 'SourceDir\'))"/>
        </xsl:attribute>
        <xsl:attribute name="Name">
          <xsl:value-of select="substring-after( ./@Source, 'SourceDir\')"/>
        </xsl:attribute>
      </wix:Payload>
      </xsl:for-each>
    </wix:PayloadGroup>
  </xsl:template>

  <!-- Rid of all components, directories etc. -->  
  <xsl:template match="//wix:DirectoryRef"/>
</xsl:stylesheet>
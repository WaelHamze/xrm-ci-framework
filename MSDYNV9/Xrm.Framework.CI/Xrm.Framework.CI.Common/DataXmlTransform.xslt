<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>

  <xsl:template match="records">
    <xsl:copy>
      <xsl:apply-templates select="record">
        <xsl:sort data-type="text" order="ascending" select="@id"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="m2mrelationships">
    <xsl:copy>
      <xsl:apply-templates select="m2mrelationship">
        <xsl:sort data-type="text" order="ascending" select="@sourceid"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="targetids">
    <xsl:copy>
      <xsl:apply-templates select="targetid">
        <xsl:sort data-type="text" order="ascending" select="node()"/>
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>

  <!--<xsl:template match="*">
    <xsl:copy>
      <xsl:apply-templates/>
    </xsl:copy>
  </xsl:template>-->
  
</xsl:stylesheet>

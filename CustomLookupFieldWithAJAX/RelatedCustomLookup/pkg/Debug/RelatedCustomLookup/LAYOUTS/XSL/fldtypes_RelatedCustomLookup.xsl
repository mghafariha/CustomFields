<!--<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>
</xsl:stylesheet>-->
<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema"
                xmlns:d="http://schemas.microsoft.com/sharepoint/dsp"
                version="1.0"
                exclude-result-prefixes="xsl msxsl ddwrt"
                xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime"
                xmlns:asp="http://schemas.microsoft.com/ASPNET/20"
                xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:SharePoint="Microsoft.SharePoint.WebControls"
                xmlns:ddwrt2="urn:frontpage:internal">

  <xsl:template match="FieldRef[@FieldType='RelatedCustomLookupQuery']" mode="Lookup_body" ddwrt:dvt_mode="body" priority="10">
    <xsl:param name="thisNode" select="."/>
    <xsl:param name="fieldValue" select="$thisNode/@*[name()=current()/@Name]"/>

    <xsl:variable name="fieldDefinition" select="$XmlDefinition/ViewFields/FieldRef[@Name=current()/@Name]"/>
    <xsl:variable name="fieldId" select="$fieldDefinition/@ID"/>

    <xsl:variable name="lookupDataValue">
      <xsl:call-template name="string-replace-with">
        <xsl:with-param name="candidate" select="$fieldId" />
        <xsl:with-param name="replace" select="'-'" />
        <xsl:with-param name="with" select="''" />
      </xsl:call-template>
    </xsl:variable>

    <xsl:value-of select="$thisNode/@*[name()=concat('r_', substring($lookupDataValue,1,30))]" disable-output-escaping ="yes"/>
  </xsl:template>

  <xsl:template name="string-replace-with">
    <xsl:param name="candidate" />
    <xsl:param name="replace" />
    <xsl:param name="with" />
    <xsl:choose>
      <xsl:when test="contains($candidate, $replace)">
        <xsl:value-of select="substring-before($candidate,$replace)" />
        <xsl:value-of select="$with" />
        <xsl:call-template name="string-replace-with">
          <xsl:with-param name="candidate" select="substring-after($candidate,$replace)" />
          <xsl:with-param name="replace" select="$replace" />
          <xsl:with-param name="with" select="$with" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$candidate" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>

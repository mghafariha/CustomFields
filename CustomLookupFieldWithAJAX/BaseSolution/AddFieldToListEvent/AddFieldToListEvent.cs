using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Xml;

namespace BaseSolution.AddFieldToListEvent
{
    /// <summary>
    /// List Events
    /// </summary>
   public class AddFieldToListEvent : SPListEventReceiver
{
    // Methods
    public override void FieldAdded(SPListEventProperties properties)
    {
        string fieldXml = properties.FieldXml;
        XmlDocument document = new XmlDocument();
        document.LoadXml(properties.FieldXml);
        XmlElement documentElement = document.DocumentElement;
        if (documentElement.Attributes["Status"] != null)
        {
            switch (documentElement.Attributes["Status"].Value)
            {
                case "Disable":
                    properties.Field.ReadOnlyField = true;
                    break;

                case "Hide":
                    properties.Field.Hidden = true;
                    break;
            }
        }
        string schemaXmlWithResourceTokens = properties.Field.SchemaXmlWithResourceTokens;
        string xPath = properties.Field.XPath;
        base.FieldAdded(properties);
    }

    public override void FieldUpdated(SPListEventProperties properties)
    {
        string fieldXml = properties.FieldXml;
        XmlDocument document = new XmlDocument();
        document.LoadXml(properties.FieldXml);
        XmlElement documentElement = document.DocumentElement;
        if (documentElement.Attributes["Status"] != null)
        {
            switch (documentElement.Attributes["Status"].Value)
            {
                case "Disable":
                    properties.Field.ReadOnlyField = true;
                    break;

                case "Hide":
                    properties.Field.Hidden = true;
                    break;
            }
        }
        base.FieldUpdated(properties);
    }
}

 

 

}
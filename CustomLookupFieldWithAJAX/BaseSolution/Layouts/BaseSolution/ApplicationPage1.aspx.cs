using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using System.Web;
using System.Web.Services;
using System.Collections.Generic;
using BaseSolution.Classes;

namespace BaseSolution.Layouts.BaseSolution
{
   public  partial class ApplicationPage1 : LayoutsPageBase
{
    // Methods
    [WebMethod]
    public static List<SPFieldGeneral> GetFieldsList(string listId)
    {
        SPWeb web;
        try
        {
            web = SPContext.Current.Web;
        }
        catch (Exception)
        {
            web = new SPSite("http://net-sp:100").OpenWeb();
        }
        SPList list = web.Lists[new Guid(listId)];
        List<SPFieldGeneral> list2 = new List<SPFieldGeneral>();
        string str = HttpContext.Current.Request.Url.ToString();
        foreach (SPField field in list.Fields)
        {
            if ((field.FromBaseType && (field.InternalName != "Title")) && (field.InternalName != "ID"))
            {
                continue;
            }
            SPFieldGeneral item = new SPFieldGeneral();
            string str2 = field.Type.ToString();
            item.Guid = field.Id;
            item.InternalName = field.InternalName;
            item.Title = field.Title;
            item.DefaultValue = field.DefaultValue;
            item.IsRequire = field.Required;
            item.Type = field.TypeAsString;
            switch (field.TypeAsString)
            {
                case "Text":
                    item.MaxLength = ((SPFieldText) field).MaxLength;
                    break;

                case "Number":
                    item.MaxValue = ((SPFieldNumber) field).MaximumValue;
                    item.MinValue = ((SPFieldNumber) field).MinimumValue;
                    item.ShowAsPercentage = ((SPFieldNumber) field).ShowAsPercentage;
                    break;

                case "Lookup":
                    item.LookupList = ((SPFieldLookup) field).LookupList.Replace("{", "").Replace("}", "");
                    item.LookupTitleField = "Title";
                    item.LookupValueField = "ID";
                    item.AllowMultipleValue = ((SPFieldLookup) field).AllowMultipleValues;
                    break;

                case "LookupMulti":
                    item.LookupList = ((SPFieldLookup) field).LookupList.Replace("{", "").Replace("}", "");
                    item.LookupTitleField = "Title";
                    item.LookupValueField = "ID";
                    item.AllowMultipleValue = ((SPFieldLookup) field).AllowMultipleValues;
                    break;

                case "RelatedCustomLookupQuery":
                    item.LookupList = field.GetCustomProperty("ListNameLookup").ToString().Replace("{", "").Replace("}", "");
                    item.LookupTitleField = field.GetCustomProperty("FieldTitleLookup").ToString();
                    item.LookupValueField = field.GetCustomProperty("FieldValueLookup").ToString();
                    item.RelatedFields = field.GetCustomProperty("RelatedFields").ToString().Split(new char[] { '|' });
                    item.Query = field.GetCustomProperty("QueryLookup").ToString();
                    break;

                case "MasterDetail":
                    item.LookupList = field.GetCustomProperty("ListNameLookup").ToString();
                    item.RelatedFields = field.GetCustomProperty("RelatedFields").ToString().Split(new char[] { '|' });
                    item.MasterLookupName = field.GetCustomProperty("MasterFieldNameLookup").ToString();
                    break;

                case "CustomComputedField":
                    item.LookupList = field.GetCustomProperty("ListNameQuery").ToString();
                    item.LookupTitleField = field.GetCustomProperty("FieldNameQuery").ToString();
                    item.Query = field.GetCustomProperty("TextQuery").ToString();
                    item.AggregationFunction = field.GetCustomProperty("AggregatorFunction").ToString();
                    break;

                case "Choice":
                {
                    SPFieldChoice choice = (SPFieldChoice) field;
                    item.options = new List<string>();
                    foreach (string str3 in choice.Choices)
                    {
                        item.options.Add(str3);
                    }
                    item.DefaultValue = ((SPFieldChoice) field).DefaultValue;
                    item.AllowMultipleValue = ((SPFieldChoice) field).ListItemMenu;
                    break;
                }
                case "MultiChoice":
                {
                    SPFieldMultiChoice choice2 = (SPFieldMultiChoice) field;
                    item.options = new List<string>();
                    foreach (string str3 in choice2.Choices)
                    {
                        item.options.Add(str3);
                    }
                    item.AllowMultipleValue = ((SPFieldMultiChoice) field).ListItemMenu;
                    break;
                }
            }
            list2.Add(item);
        }
        return list2;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }
}

 

 

}

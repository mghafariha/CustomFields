using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Collections.Generic;

namespace BaseSolution.AddItemToListEvent
{
    /// <summary>
    /// List Item Events
    /// </summary>
   public class AddItemToListEvent : SPItemEventReceiver
{
    // Methods
    private List<string> AllIndexesOf(string oldStr, string value, out string str, SPItemEventDataCollection item)
    {
        List<string> list = new List<string>();
        str = "";
        string newValue = "";
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("the string to find may not be empty", "value");
        }
        List<int> list2 = new List<int>();
        int num = 0;
        int startIndex = 0;
        while (true)
        {
            startIndex = oldStr.IndexOf(value, startIndex);
            if (startIndex == -1)
            {
                str = oldStr;
                return list;
            }
            int index = oldStr.IndexOf("}}", (int) (startIndex + 1));
            string str3 = oldStr.Substring(startIndex + 2, (index - startIndex) - 2);
            oldStr = oldStr.Replace(oldStr.Substring(startIndex, (index - startIndex) + 2), newValue);
            list.Add(str3);
            num++;
            startIndex += value.Length;
        }
    }

    private string CalculateValue(SPWeb web, SPItemEventDataCollection item, string listName, string fields, string filters, string func, string items)
    {
        SPListItemCollection items2;
        float num2;
        SPList list = web.GetList("/Lists/" + listName);
        SPQuery query = new SPQuery();
        string str = "";
        string s = "";
        List<string> list2 = this.AllIndexesOf(filters, "{{", out str, item);
        string[] strArray = fields.Split(new char[] { ',' });
        if (filters != "")
        {
            query.Query = str;
            query.ViewAttributes = "Scope='RecursiveAll'";
        }
        string str5 = func;
        switch (str5)
        {
            case "خالی":
            case "":
                str5 = items;
                if ((str5 == null) || (str5 != "Current"))
                {
                    return s;
                }
                return item[strArray[0]].ToString();

            case "Max":
                switch (items)
                {
                    case "Current":
                    {
                        float num = 0f;
                        foreach (string str3 in strArray)
                        {
                            if (float.Parse(item[str3].ToString()) > num)
                            {
                                num = float.Parse(item[str3].ToString());
                            }
                        }
                        return num.ToString();
                    }
                    case "All":
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        return items2[0][strArray[0]].ToString();

                    case "Current&All":
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        num2 = float.Parse(item[strArray[0]].ToString());
                        if (num2 <= float.Parse(s))
                        {
                            return s;
                        }
                        return num2.ToString();
                }
                return s;

            case "Min":
                switch (items)
                {
                    case "Current":
                    {
                        float num3 = 0f;
                        foreach (string str3 in strArray)
                        {
                            if (float.Parse(item[str3].ToString()) > num3)
                            {
                                num3 = float.Parse(item[str3].ToString());
                            }
                        }
                        return num3.ToString();
                    }
                    case "All":
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        return items2[0][strArray[0]].ToString();

                    case "Current&All":
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        s = items2[0][strArray[0]].ToString();
                        num2 = float.Parse(item[strArray[0]].ToString());
                        if (num2 >= float.Parse(s))
                        {
                            return s;
                        }
                        return num2.ToString();
                }
                return s;

            case "Count":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                s = items2.Count.ToString();
                if (items == "Current&All")
                {
                    s = (float.Parse(s) + 1f).ToString();
                }
                return s;

            case "First":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                return items2[0][strArray[0]].ToString();

            case "Last":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                return items2[items2.Count - 1][strArray[0]].ToString();

            case "Sum":
                float num4;
                switch (items)
                {
                    case "Current":
                        num4 = 0f;
                        foreach (string str3 in strArray)
                        {
                            num4 += float.Parse(item[str3].ToString());
                        }
                        return num4.ToString();

                    case "All":
                        num4 = 0f;
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        foreach (SPListItem item2 in items2)
                        {
                            num4 += float.Parse(item[strArray[0]].ToString());
                        }
                        return num4.ToString();

                    case "Current&All":
                    {
                        num4 = 0f;
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        foreach (SPListItem item2 in items2)
                        {
                            num4 += float.Parse(item[strArray[0]].ToString());
                        }
                        float num7 = num4 + float.Parse(item[strArray[0]].ToString());
                        return num7.ToString();
                    }
                }
                return s;
        }
        return s;
    }

    private bool CompareCheck(string compareAction, string sourceValue, string destValue)
    {
        float num = 0f;
        float num2 = 0f;
        string str = "float";
        try
        {
            num = float.Parse(sourceValue);
            num2 = float.Parse(destValue);
        }
        catch (Exception)
        {
            DateTime time = DateTime.Parse(sourceValue);
            DateTime time2 = DateTime.Parse(destValue);
            str = "DateTime";
        }
        switch (compareAction)
        {
            case "StrCmp":
                if (!string.Equals(sourceValue, destValue))
                {
                    break;
                }
                return true;

            case "Greater":
                if (str != "DateTime")
                {
                    if (float.Parse(sourceValue) > float.Parse(destValue))
                    {
                        return true;
                    }
                    break;
                }
                if (DateTime.Parse(sourceValue) <= DateTime.Parse(destValue))
                {
                    break;
                }
                return true;

            case "GreaterEqual":
                if (str != "DateTime")
                {
                    if (float.Parse(sourceValue) >= float.Parse(destValue))
                    {
                        return true;
                    }
                    break;
                }
                if (DateTime.Parse(sourceValue) < DateTime.Parse(destValue))
                {
                    break;
                }
                return true;

            case "Less":
                if (str != "DateTime")
                {
                    if (float.Parse(sourceValue) < float.Parse(destValue))
                    {
                        return true;
                    }
                    break;
                }
                if (DateTime.Parse(sourceValue) >= DateTime.Parse(destValue))
                {
                    break;
                }
                return true;

            case "LessEqual":
                if (str != "DateTime")
                {
                    if (float.Parse(sourceValue) <= float.Parse(destValue))
                    {
                        return true;
                    }
                    break;
                }
                if (DateTime.Parse(sourceValue) > DateTime.Parse(destValue))
                {
                    break;
                }
                return true;

            case "Equal":
                if ((str != "DateTime") || !(DateTime.Parse(sourceValue) == DateTime.Parse(destValue)))
                {
                    if (float.Parse(sourceValue) == float.Parse(destValue))
                    {
                        return true;
                    }
                    break;
                }
                return true;
        }
        return false;
    }

    private string CreateErrorMsg(SPWeb web, string prCompareAction, string value, string sourceValue, string sourceField, string sourceFilters, string prSourceAction, string sourceList, string destValue, string destField, string destFilters, string prDestAction, string destList)
    {
        SPList list = web.GetList("/Lists/" + sourceList);
        string title = list.Title;
        string str2 = list.Fields[sourceField].Title;
        string str3 = prSourceAction + sourceField + " از لیست " + title;
        string str4 = "";
        if (sourceFilters.Length > 0)
        {
            str3 = str3 + " با فیلتر (" + sourceFilters + ")";
        }
        if (destField != "")
        {
            string str7 = str4;
            str4 = str7 + prDestAction + " (" + destField + ") از لیست " + destList;
        }
        else
        {
            str4 = str4 + "مقدار ثابت(" + value + ")";
        }
        if ((destFilters.Length > 0) && (destField != ""))
        {
            str4 = str4 + "با فیلتر (" + destFilters + ")";
        }
        return (str3 + "  در مقایسه با " + str4 + prCompareAction + "نیست");
    }

    public override void ItemAdding(SPItemEventProperties properties)
    {
        SPList list = properties.List;
        SPWeb web = properties.Web;
    }
}

 

 

}
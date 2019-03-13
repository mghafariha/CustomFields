using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseSolution.Classes;
using Microsoft.SharePoint;
using System.Web.Script.Serialization;
using System.Globalization;

namespace BaseSolution
{
    public static class Utility
{
    // Methods
    public static List<string> AllIndexesOf(string oldStr, string value, out string str, List<SPFieldValue> item)
    {
        List<string> list = new List<string>();
        str = "";
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
            string filter = oldStr.Substring(startIndex + 2, (index - startIndex) - 2);
            SPFieldValue value2 = item.FirstOrDefault<SPFieldValue>(a => a.InternalName == filter);
            oldStr = oldStr.Replace(oldStr.Substring(startIndex, (index - startIndex) + 2), value2.value);
            list.Add(filter);
            num++;
            startIndex += value.Length;
        }
    }

    public static string Approve(SPWeb web, string comment, string listId, int itemId, int currentUserID, List<SPFieldValue> fields, List<Attachment> files)
    {
       
        string stImprove = "";
        Guid siteID = web.Site.ID;
        int canApprove = -1;
        string sourceValue = "";
        string destValue = "";
        string compareAction = "";
        string[] sourceFieldArray = null;
        int creatorId = 0;
        int approver1Id = 0;
        int approver2Id = 0;
        int approver3Id = 0;
        int approver4Id = 0;
        int approver5Id = 0;
        int contractId = 0;
        int areaId = 0;
        string status = "";
        int nextUserId = 0;
        List<ErrorMessage> list = new List<ErrorMessage>();
        List<StepFields> list2 = CanApprove(web, listId, itemId, out canApprove);
        if (canApprove != -1)
        {
            SPListItemCollection items = FindValidation(web, new Guid(listId));
            foreach (SPListItem item in items)
            {
                
                List<string> fieldNames = item["SourceField"].ToString().Split(new char[] { ',' }).ToList<string>();
                if (fields.Where<SPFieldValue>(delegate (SPFieldValue a) {
                    
                    return fieldNames.Any<string>(b => (b == a.InternalName));
                }).ToList<SPFieldValue>().Count > 0)
                {
                    CheckValidation(web, item, fields, out sourceFieldArray, out sourceValue, out destValue, out compareAction);
                    if (((destValue != "") && (sourceValue != "")) && !CompareCheck(compareAction, sourceValue, destValue))
                    {
                        ErrorMessage message = new ErrorMessage {
                            FieldNames = fieldNames,
                            Message = item["Message"].ToString(),
                            RowNumber = -1
                        };
                        list.Add(message);
                    }
                }
            }
        }
        if (list.Count > 0)
        {
            return new JavaScriptSerializer().Serialize(list);
        }
         SPSecurity.RunWithElevatedPrivileges(delegate {
                            using (SPSite site = new SPSite(siteID))
                          {
                              using (SPWeb web1 = site.OpenWeb())
                               {
       // SPWeb web1=web;
                                    //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US", false); 
  
                        Exception exception;
                        SPList spList = web1.Lists[new Guid(listId)];
                        string title = spList.Title;
                        string url = spList.RootFolder.Url;
                        string str3 = url.Substring(url.LastIndexOf("/") + 1);
                        SPListItem itemById = spList.GetItemById(itemId);
                        string permissionFieldLookup = GetPermissionFieldLookup(web, spList.ID.ToString());
                        int lookupId = 0;
                        if (permissionFieldLookup != "")
                        {
                            lookupId = new SPFieldLookupValue(itemById[permissionFieldLookup].ToString()).LookupId;
                        }
                        try
                        {
                            areaId = new SPFieldLookupValue(itemById["Area"].ToString()).LookupId;
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                        }
                        GetContractUsers(web, listId, lookupId, areaId, out contractId, out creatorId, out approver1Id, out approver2Id, out approver3Id, out approver4Id, out approver5Id);
                        try
                        {
                            if ((canApprove == 1) && (currentUserID == approver1Id))
                            {
                                if (approver2Id != 0)
                                {
                                    status = "در انتظار تایید";
                                    nextUserId = approver2Id;
                                }
                                else
                                {
                                    status = "پایان فرآیند";
                                    nextUserId = 0;
                                }
                            }
                            else if ((canApprove == 2) && (currentUserID == approver2Id))
                            {
                                if (approver3Id != 0)
                                {
                                    status = "در انتظار تایید";
                                    nextUserId = approver3Id;
                                }
                                else
                                {
                                    status = "پایان فرآیند";
                                    nextUserId = 0;
                                }
                            }
                            else if ((canApprove == 3) && (currentUserID == approver3Id))
                            {
                                if (approver4Id != 0)
                                {
                                    status = "در انتظار تایید";
                                    nextUserId = approver5Id;
                                }
                                else
                                {
                                    status = "پایان فرآیند";
                                    nextUserId = 0;
                                }
                            }
                            else if ((canApprove == 4) && (currentUserID == approver4Id))
                            {
                                if (approver5Id != 0)
                                {
                                    status = "در انتظار تایید";
                                    nextUserId = approver4Id;
                                }
                                else
                                {
                                    status = "پایان فرآیند";
                                    nextUserId = 0;
                                }
                            }
                            else if ((canApprove == 5) && (currentUserID == approver5Id))
                            {
                                status = "پایان فرآیند";
                                nextUserId = 0;
                            }
                            else
                            {
                                stImprove = "شما دسترسی لازم را ندارید.";
                            }
                            if (stImprove == "")
                            {
                                web1.AllowUnsafeUpdates = true;
                                SPFieldValue item = new SPFieldValue {
                                    InternalName = "Status",
                                    Type = "Text",
                                    value = status
                                };
                                fields.Add(item);
                                SPFieldValue value3 = new SPFieldValue {
                                    InternalName = "CurrentUser",
                                    Type = "User",
                                    value = (nextUserId != 0) ? nextUserId.ToString() : null
                                };
                                fields.Add(value3);
                                string str5 = UpdateFiles(web, contractId, fields, files, new List<Attachment>());
                                if (str5 != "ok")
                                {
                                    stImprove = str5;
                                }
                                else
                                {
                                    
                                    setItemFields(fields, itemById);
                                    itemById.Update();
                                    CreateHistory(web1, itemById.ID, DateTime.Now.ToString(), "تایید اطلاعات", comment, itemById.Url.Split(new char[] { '/' })[1], currentUserID);
                                    if (nextUserId != 0)
                                    {
                                        string str6 = string.Concat(new object[] { "/_Layouts/15/testNew/dist/edit/EditForm.aspx?ListName=", str3, "&ID=", itemById.ID });
                                    }
                                    web1.AllowUnsafeUpdates = false;
                                }
                            }
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                            stImprove = exception.Message;
                        }
                   }
               }
            });
       
       
        return stImprove;
    }

    public static string CalculateValue(SPWeb web, List<SPFieldValue> item, string listName, string fields, string filters, string func, string items)
    {
        SPFieldValue value2;
        SPListItemCollection items2;
        Func<SPFieldValue, bool> predicate = null;
        Func<SPFieldValue, bool> func14 = null;
        Func<SPFieldValue, bool> func15 = null;
        string[] strArray;
        int num5;
        SPList list = web.Lists[new Guid(listName)];
        SPQuery query = new SPQuery();
        string str = "";
        string s = "";
        List<string> list2 = AllIndexesOf(filters, "{{", out str, item);
        string[] fieldArray = fields.Split(new char[] { ',' });
        if (filters != "")
        {
            query.Query = str;
            query.ViewAttributes = "Scope='RecursiveAll'";
        }
        string str4 = func;
        switch (str4)
        {
            case "خالی":
            case "":
                str4 = items;
                if ((str4 == null) || (str4 != "Current"))
                {
                    return s;
                }
                if (predicate == null)
                {
                    predicate = a => a.InternalName == fieldArray[0].ToString();
                }
                if (item.FirstOrDefault<SPFieldValue>(predicate) == null)
                {
                    return s;
                }
                if (func14 == null)
                {
                    func14 = a => a.InternalName == fieldArray[0].ToString();
                }
                return item.FirstOrDefault<SPFieldValue>(func14).value;

            case "Max":
                float num;
                switch (items)
                {
                    case "Current":
                        num = 0f;
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func2 = null;
                            Func<SPFieldValue, bool> func3 = null;
                            string str2 = strArray[num5];
                            if (func2 == null)
                            {
                                func2 = a => a.InternalName == str2;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func2) != null)
                            {
                                if (func3 == null)
                                {
                                    func3 = a => a.InternalName == str2;
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func3);
                                if (float.Parse(value2.value.ToString()) > num)
                                {
                                    num = float.Parse(value2.value.ToString());
                                }
                            }
                        }
                        return num.ToString();

                    case "All":
                        items2 = (filters != "") ? list.GetItems(new string[] { str }) : list.GetItems(new string[0]);
                        return items2[0][fieldArray[0]].ToString();

                    case "Current&All":
                        items2 = (filters != "") ? list.GetItems(new string[] { str }) : list.GetItems(new string[0]);
                        num = float.Parse(items2[0][fieldArray[0]].ToString());
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func4 = null;
                            Func<SPFieldValue, bool> func5 = null;
                            string str3 = strArray[num5];
                            if (func4 == null)
                            {
                                func4 = a => a.InternalName == str3;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func4) != null)
                            {
                                if (func5 == null)
                                {
                                    func5 = a => a.InternalName == str3;
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func5);
                                if (float.Parse(value2.value.ToString()) > num)
                                {
                                    num = float.Parse(value2.value.ToString());
                                }
                            }
                        }
                        return num.ToString();
                }
                return s;

            case "Min":
                float num2;
                switch (items)
                {
                    case "Current":
                        num2 = 0f;
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func6 = null;
                            string str5 = strArray[num5];
                            if (func6 == null)
                            {
                                func6 = a => a.InternalName == str5;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func6) != null)
                            {
                                if (func15 == null)
                                {
                                    func15 = a => a.InternalName == fieldArray[0].ToString();
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func15);
                                if (float.Parse(value2.value) > num2)
                                {
                                    num2 = float.Parse(value2.value);
                                }
                            }
                        }
                        return num2.ToString();

                    case "All":
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        return items2[0][fieldArray[0]].ToString();

                    case "Current&All":
                        num2 = 0f;
                        items2 = (filters != "") ? list.GetItems(new string[] { str }) : list.GetItems(new string[0]);
                        num2 = float.Parse(items2[0][fieldArray[0]].ToString());
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func7 = null;
                            Func<SPFieldValue, bool> func8 = null;
                            string str6 = strArray[num5];
                            if (func7 == null)
                            {
                                func7 = a => a.InternalName == str6;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func7) != null)
                            {
                                if (func8 == null)
                                {
                                    func8 = a => a.InternalName == str6;
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func8);
                                if (float.Parse(value2.value.ToString()) < num2)
                                {
                                    num2 = float.Parse(value2.value.ToString());
                                }
                            }
                        }
                        return num2.ToString();
                }
                return s;

            case "Count":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                value2 = item.FirstOrDefault<SPFieldValue>(a => a.InternalName == "ID");
                s = items2.Count.ToString();
                if (((value2.value != "0") && (items2.Count > 0)) && (items2.GetItemById(int.Parse(value2.value)) != null))
                {
                    if (items2.GetItemById(int.Parse(value2.value)) != null)
                    {
                        s = (items2.Count - 1).ToString();
                    }
                    if (items == "Current&All")
                    {
                        s = (float.Parse(s) + 1f).ToString();
                    }
                }
                return s;

            case "First":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                return items2[0][fieldArray[0]].ToString();

            case "Last":
                items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                return items2[items2.Count - 1][fieldArray[0]].ToString();

            case "Sum":
                float num3;
                switch (items)
                {
                    case "Current":
                        num3 = 0f;
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func9 = null;
                            Func<SPFieldValue, bool> func10 = null;
                            string str7 = strArray[num5];
                            if (func9 == null)
                            {
                                func9 = a => a.InternalName == str7;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func9) != null)
                            {
                                if (func10 == null)
                                {
                                    func10 = a => a.InternalName == str7;
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func10);
                                num3 += float.Parse(value2.value);
                            }
                        }
                        return num3.ToString();

                    case "All":
                        num3 = 0f;
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        foreach (SPListItem item2 in items2)
                        {
                            num3 += float.Parse(item2[fieldArray[0]].ToString());
                        }
                        return num3.ToString();

                    case "Current&All":
                        num3 = 0f;
                        items2 = (filters != "") ? list.GetItems(query) : list.GetItems(new string[0]);
                        foreach (SPListItem item2 in items2)
                        {
                            num3 += float.Parse(item2[fieldArray[0]].ToString());
                        }
                        strArray = fieldArray;
                        for (num5 = 0; num5 < strArray.Length; num5++)
                        {
                            Func<SPFieldValue, bool> func11 = null;
                            Func<SPFieldValue, bool> func12 = null;
                            string str8 = strArray[num5];
                            if (func11 == null)
                            {
                                func11 = a => a.InternalName == str8;
                            }
                            if (item.FirstOrDefault<SPFieldValue>(func11) != null)
                            {
                                if (func12 == null)
                                {
                                    func12 = a => a.InternalName == str8;
                                }
                                value2 = item.FirstOrDefault<SPFieldValue>(func12);
                                num3 += float.Parse(value2.value);
                            }
                        }
                        return num3.ToString();
                }
                return s;
        }
        return s;
    }

    public static List<StepFields> CanApprove(SPWeb web, string listId, int itemId, out int canApprove)
    {
        List<StepFields> stepFields = new List<StepFields>();
        int approveStep = -1;
        int currentUserId = web.CurrentUser.ID;
        List<string> list = new List<string>();
        string siteURL = web.Url;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb web1 = site.OpenWeb())
                {
                    int lookupId = 0;
                    int areaId = 0;
                    SPList contractList = web1.GetList("/Lists/Contracts");
                    SPList list2 = web1.GetList("/Lists/FormPermissions");
                    SPList list3 = web1.Lists[new Guid(listId)];
                    string title = list3.Title;
                    string url = list3.RootFolder.Url;
                    string str4 = url.Substring(url.LastIndexOf("/") + 1);
                    SPListItem item = (itemId != 0) ? list3.GetItemById(itemId) : null;
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                          <Eq>\r\n                                             <FieldRef Name='ListName' />\r\n                                             <Value Type='Text'>{0}</Value>\r\n                                          </Eq>\r\n                                       </Where>", str4)
                    };
                    SPListItem item2 = (list2.GetItems(query).Count > 0) ? list2.GetItems(query)[0] : null;
                    if (item2 != null)
                    {
                        if (itemId != 0)
                        {
                            int num3 = 0;
                            string str5 = "";
                            string str6 = (item2["PermissionField"] != null) ? item2["PermissionField"].ToString() : "";
                            string strUrl = (item2["PermissionLookupList"] != null) ? item2["PermissionLookupList"].ToString() : "";
                            string str8 = (item2["PermissionLookupListField"] != null) ? item2["PermissionLookupListField"].ToString() : "";
                            if (strUrl != "")
                            {
                                SPList list4 = web1.GetList(strUrl);
                                int id = new SPFieldLookupValue(item[str8].ToString()).LookupId;
                                lookupId = new SPFieldLookupValue(list4.GetItemById(id)[str6].ToString()).LookupId;
                            }
                            else if (str6 != "")
                            {
                                lookupId = new SPFieldLookupValue(item[str6].ToString()).LookupId;
                            }
                            if (item["CurrentUser"] != null)
                            {
                                num3 = new SPFieldLookupValue(item["CurrentUser"].ToString()).LookupId;
                            }
                            if (item["Status"] != null)
                            {
                                str5 = item["Status"].ToString();
                            }
                            try
                            {
                                areaId = new SPFieldLookupValue(item["Area"].ToString()).LookupId;
                            }
                            catch (Exception)
                            {
                            }
                            int num5 = GetRelatedUser(web, new SPFieldLookupValue(item2["Creator"].ToString()).LookupId, lookupId, areaId, new SPFieldUserValue(web, item["Author"].ToString()).LookupId);
                            int num6 = (item2["Approver1"] != null) ? GetRelatedUser(web, new SPFieldLookupValue(item2["Approver1"].ToString()).LookupId, lookupId, areaId, new SPFieldUserValue(web, item["Author"].ToString()).LookupId) : 0;
                            int num7 = (item2["Approver2"] != null) ? GetRelatedUser(web, new SPFieldLookupValue(item2["Approver2"].ToString()).LookupId, lookupId, areaId, currentUserId) : 0;
                            int num8 = (item2["Approver3"] != null) ? GetRelatedUser(web, new SPFieldLookupValue(item2["Approver3"].ToString()).LookupId, lookupId, areaId, currentUserId) : 0;
                            int num9 = (item2["Approver4"] != null) ? GetRelatedUser(web, new SPFieldLookupValue(item2["Approver4"].ToString()).LookupId, lookupId, areaId, currentUserId) : 0;
                            int num10 = (item2["Approver5"] != null) ? GetRelatedUser(web, new SPFieldLookupValue(item2["Approver5"].ToString()).LookupId, lookupId, areaId, currentUserId) : 0;
                            if (num3 != 0)
                            {
                                if (((str5 == "در انتظار تایید") && (num6 == currentUserId)) && (currentUserId == num3))
                                {
                                    approveStep = 1;
                                }
                                else if ((((num7 != 0) && (str5 == "در انتظار تایید")) && (num7 == currentUserId)) && (currentUserId == num3))
                                {
                                    approveStep = 2;
                                }
                                else if ((((num8 != 0) && (str5 == "در انتظار تایید")) && (num8 == currentUserId)) && (currentUserId == num3))
                                {
                                    approveStep = 3;
                                }
                                else if ((((num9 != 0) && (str5 == "در انتظار تایید")) && (num9 == currentUserId)) && (currentUserId == num3))
                                {
                                    approveStep = 4;
                                }
                                else if ((((num10 != 0) && (str5 == "در انتظار تایید")) && (num10 == currentUserId)) && (currentUserId == num3))
                                {
                                    approveStep = 5;
                                }
                            }
                        }
                        for (int j = (approveStep == -1) ? 1 : approveStep; j <= 5; j++)
                        {
                            string str9 = "Approver" + j.ToString() + "Fields";
                            StepFields fields = new StepFields {
                                Step = j,
                                Fields = (item2[str9] != null) ? item2[str9].ToString().Split(new char[] { ',' }).ToList<string>() : null
                            };
                            stepFields.Add(fields);
                        }
                    }
                }
            }
        });
        canApprove = approveStep;
        return stepFields;
    }

    public static void CheckValidation(SPWeb web, SPListItem itm, List<SPFieldValue> fields, out string[] sourceFieldArray, out string sourceValue, out string destValue, out string compareAction)
    {
        string str = (itm["value"] != null) ? itm["value"].ToString() : "";
        string g = itm["SourceList"].ToString();
        string str3 = itm["SourceField"].ToString();
        sourceFieldArray = str3.Split(new char[] { ',' });
        string filters = (itm["SourceFilter"] != null) ? itm["SourceFilter"].ToString() : "";
        string func = (itm["SourceAction"] != null) ? itm["SourceAction"].ToString() : "";
        string items = (itm["SourceItems"] != null) ? itm["SourceItems"].ToString() : "";
        SPList list = web.Lists[new Guid(g)];
        sourceValue = (((filters == "") && ((func == "") || (func == "خالی"))) && (((items == "") || (items == "خالی")) && (str != ""))) ? str : CalculateValue(web, fields, g, str3, filters, func, items);
        string listName = (itm["DestList"] != null) ? itm["DestList"].ToString() : "";
        string str9 = (itm["DestField"] != null) ? itm["DestField"].ToString() : "";
        string str10 = (itm["DestFilter"] != null) ? itm["DestFilter"].ToString() : "";
        string str11 = (itm["DestAction"] != null) ? itm["DestAction"].ToString() : "";
        string str12 = (itm["DestItems"] != null) ? itm["DestItems"].ToString() : "";
        destValue = (((listName == "") && (str9 == "")) && ((str10 == "") && (str != ""))) ? str : CalculateValue(web, fields, listName, str9, str10, str11, str12);
        compareAction = itm["CompareAction"].ToString();
    }

    public static bool CompareCheck(string compareAction, string sourceValue, string destValue)
    {
        float num = 0f;
        float num2 = 0f;
        string str = "float";
        if (compareAction != "StrCmp")
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
                if (string.Equals(sourceValue, destValue))
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

    public static string CompleteTask(SPWeb web, int itemId, string listName, int userId)
    {
        string strError = "";
        int iD = web.CurrentUser.ID;
        Guid siteID = web.Site.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteID))
            {
                using (SPWeb Web = site.OpenWeb())
                {
                    Web.AllowUnsafeUpdates = true;
                    SPList list = Web.GetList("/Lists/" + "/Lists/WorkflowTasks");
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                                      <And>\r\n                                                         <Eq>\r\n                                                            <FieldRef Name='ItemId' />\r\n                                                            <Value Type='Number'>{0}</Value>\r\n                                                         </Eq>\r\n                                                         <And>\r\n                                                            <Eq>\r\n                                                               <FieldRef Name='ListName' />\r\n                                                               <Value Type='Text'>{1}</Value>\r\n                                                            </Eq>\r\n                                                            <And>\r\n                                                               <Neq>\r\n                                                                  <FieldRef Name='PercentComplete' />\r\n                                                                  <Value Type='Number'>100</Value>\r\n                                                               </Neq>\r\n                                                               <Contains>\r\n                                                                  <FieldRef Name='AssignedTo' LookupId='TRUE' />\r\n                                                                  <Value Type='LookupMulti'>{2}</Value>\r\n                                                               </Contains>\r\n                                                            </And>\r\n                                                         </And>\r\n                                                      </And>\r\n                                                   </Where>", itemId, listName, userId)
                    };
                    SPListItem item = list.GetItems(query)[0];
                    item["PercentComplete"] = 100;
                    try
                    {
                        item.Update();
                    }
                    catch (Exception exception)
                    {
                        strError = exception.Message;
                    }
                }
            }
        });
        return strError;
    }

    public static string CreateErrorMsg(SPWeb web, string prCompareAction, string value, string sourceValue, string sourceField, string sourceFilters, string prSourceAction, string sourceList, string destValue, string destField, string destFilters, string prDestAction, string destList)
    {
        SPList list = web.Lists[new Guid(sourceList)];
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

    public static string CreateHistory(SPWeb weba, int itemID, string date, string eventString, string description, string listname, int userId)
    {
        string strError = "";
        int iD = weba.CurrentUser.ID;
        Guid siteID = weba.Site.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteID))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    string strUrl = "/Lists/WorkflowHistories";
                    SPListItem item = web.GetList(strUrl).AddItem();
                    item["Title"] = eventString + date;
                    item["ListName"] = listname;
                    item["ItemID"] = itemID;
                    item["User"] = new SPFieldUserValue(web, userId, "");
                    item["Date"] = DateTime.Now;
                    item["Event"] = eventString;
                    item["Comment"] = description;
                    try
                    {
                        item.Update();
                    }
                    catch (Exception exception)
                    {
                        strError = exception.Message;
                    }
                }
            }
        });
        return strError;
    }

    public static string CreateTask(SPWeb web, string taskName, int assignToID, string description, string linkItem, string list, int itemid)
    {
        string strError = "";
        int iD = web.CurrentUser.ID;
        Guid siteID = web.Site.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteID))
            {
                using (SPWeb Web = site.OpenWeb())
                {
                    Web.AllowUnsafeUpdates = true;
                    string strUrl = "/Lists/WorkflowTasks";
                    SPListItem item = Web.GetList(strUrl).AddItem();
                    item["Title"] = taskName;
                    item["StartDate"] = DateTime.Now;
                    item["ListName"] = list;
                    item["ItemId"] = itemid;
                    SPFieldUserValueCollection values = new SPFieldUserValueCollection();
                    values.Add(new SPFieldUserValue(Web, assignToID, ""));
                    item["AssignedTo"] = values;
                    SPFieldUrlValue value2 = new SPFieldUrlValue {
                        Description = description,
                        Url = linkItem
                    };
                    item["ItemLink"] = value2;
                    try
                    {
                        item.Update();
                        int num = 0;
                        foreach (SPFieldUserValue value3 in values)
                        {
                            if (num == 0)
                            {
                                SetListItemPermission(item, value3.LookupId, 0x40000002, true);
                            }
                            else
                            {
                                SetListItemPermission(item, value3.LookupId, 0x40000002, false);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        strError = exception.Message;
                    }
                }
            }
        });
        return strError;
    }

    public static string DeleteItemFromList(SPList list, int itemId)
    {
        string message = "ok";
        try
        {
            Guid siteID = list.ParentWeb.Site.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate {
                using (SPSite site = new SPSite(siteID))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        web.AllowUnsafeUpdates = true;
                        SPList list1 = web.Lists[list.ID];
                        list1.Items.DeleteItemById(itemId);
                        web.AllowUnsafeUpdates = false;
                    }
                }
            });
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }
        return message;
    }

    public static SPListItemCollection FindValidation(SPWeb web, Guid listId)
    {
        SPList list = web.GetList("Lists/CheckValidationsList");
        SPQuery query = new SPQuery {
            Query = string.Format("<Where>\r\n                                            <Eq>\r\n                                             <FieldRef Name='SourceList' />\r\n                                             <Value Type='Text'>{0}</Value>\r\n                                            </Eq> \r\n                                         </Where>", listId)
        };
        return list.GetItems(query);
    }

    public static void GetContractUsers(SPWeb web, string listId, int permissionFieldValue, int areaId, out int contractId, out int creatorId, out int approver1Id, out int approver2Id, out int approver3Id, out int approver4Id, out int approver5Id)
    {
        List<StepFields> list = new List<StepFields>();
        int contrId = 0;
        int currentUserId = web.CurrentUser.ID;
        List<string> list2 = new List<string>();
        int creatId = 0;
        int appr1Id = 0;
        int appr2Id = 0;
        int appr3Id = 0;
        int appr4Id = 0;
        int appr5Id = 0;
        string siteURL = web.Url;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb Web = site.OpenWeb())
                {
                    SPList contractlist = Web.GetList("/Lists/Contracts");
                    SPList perList = Web.GetList("/Lists/FormPermissions");
                    SPList list3 = Web.Lists[new Guid(listId)];
                    string title = list3.Title;
                    string url = list3.RootFolder.Url;
                    string str3 = url.Substring(url.LastIndexOf("/") + 1);
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                                          <Eq>\r\n                                                             <FieldRef Name='ListName' />\r\n                                                             <Value Type='Text'>{0}</Value>\r\n                                                          </Eq>\r\n                                                       </Where>", str3)
                    };
                    SPListItem item = (perList.GetItems(query).Count > 0) ? perList.GetItems(query)[0] : null;
                    if (item != null)
                    {
                        string str4 = (item["PermissionField"] != null) ? item["PermissionField"].ToString() : "";
                        string strUrl = (item["PermissionLookupList"] != null) ? item["PermissionLookupList"].ToString() : "";
                        string str6 = (item["PermissionLookupListField"] != null) ? item["PermissionLookupListField"].ToString() : "";
                        if (strUrl != "")
                        {
                            SPListItem itemById = Web.GetList(strUrl).GetItemById(permissionFieldValue);
                            contrId = new SPFieldLookupValue(itemById[str4].ToString()).LookupId;
                        }
                        else if (str4 != "")
                        {
                            contrId = permissionFieldValue;
                        }
                        creatId = GetRelatedUser(web, new SPFieldLookupValue(item["Creator"].ToString()).LookupId, contrId, areaId, currentUserId);
                        appr1Id = (item["Approver1"] != null) ? GetRelatedUser(Web, new SPFieldLookupValue(item["Approver1"].ToString()).LookupId, contrId, areaId, currentUserId) : 0;
                        appr2Id = (item["Approver2"] != null) ? GetRelatedUser(Web, new SPFieldLookupValue(item["Approver2"].ToString()).LookupId, contrId, areaId, currentUserId) : 0;
                        appr3Id = (item["Approver3"] != null) ? GetRelatedUser(Web, new SPFieldLookupValue(item["Approver3"].ToString()).LookupId, contrId, areaId, currentUserId) : 0;
                        appr4Id = (item["Approver4"] != null) ? GetRelatedUser(Web, new SPFieldLookupValue(item["Approver4"].ToString()).LookupId, contrId, areaId, currentUserId) : 0;
                        appr5Id = (item["Approver5"] != null) ? GetRelatedUser(Web, new SPFieldLookupValue(item["Approver5"].ToString()).LookupId, contrId, areaId, currentUserId) : 0;
                    }
                }
            }
        });
        contractId = contrId;
        creatorId = creatId;
        approver1Id = appr1Id;
        approver2Id = appr2Id;
        approver3Id = appr3Id;
        approver4Id = appr4Id;
        approver5Id = appr5Id;
    }

    public static List<HistoryDetail> GetHistory(SPWeb web, int dailyItemID, string listName)
    {
        List<HistoryDetail> HistoryDetailList = new List<HistoryDetail>();
        string siteURL = web.Site.Url;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb web1 = site.OpenWeb())
                {
                    web1.AllowUnsafeUpdates = true;
                    SPList list = web.GetList("/Lists/WorkflowHistories");
                    SPListItem itemById = web.GetList("/Lists/" + listName).GetItemById(dailyItemID);
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                          <And>\r\n                                             <Eq>\r\n                                                <FieldRef Name='ItemID' />\r\n                                                <Value Type='Number'>{0}</Value>\r\n                                             </Eq>\r\n                                             <Eq>\r\n                                                <FieldRef Name='ListName' />\r\n                                                <Value Type='Text'>{1}</Value>\r\n                                             </Eq>\r\n                                          </And>\r\n                                          </Where>\r\n                                          <OrderBy>\r\n                                              <FieldRef Name='ID' Ascending='TRUE' />\r\n                                          </OrderBy>", dailyItemID, listName)
                    };
                    SPListItemCollection items = list.GetItems(query);
                    foreach (SPListItem item2 in items)
                    {
                        HistoryDetail item = new HistoryDetail {
                            HistoryID = item2.ID
                        };
                        try
                        {
                            item.UserName = new SPFieldUserValue(web, item2["User"].ToString()).User.Name;
                            item.ListName = item2["ListName"].ToString();
                            item.state = item2["Event"].ToString();
                            item.HistoryDate = DateTime.Parse(item2["Created"].ToString());
                            item.Description = Convert.ToString(item2["Comment"]).Replace("\"", "'");
                            HistoryDetailList.Add(item);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        });
        return HistoryDetailList;
    }

    public static string GetPermissionFieldLookup(SPWeb web, string listId)
    {
        int iD = web.CurrentUser.ID;
        List<string> list = new List<string>();
        string perLookupField = "";
        string siteURL = web.Url;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb Web = site.OpenWeb())
                {
                    SPList contractList = Web.GetList("/Lists/Contracts");
                    SPList list2 = Web.GetList("/Lists/FormPermissions");
                    SPList list3 = Web.Lists[new Guid(listId)];
                    string title = list3.Title;
                    string url = list3.RootFolder.Url;
                    string str3 = url.Substring(url.LastIndexOf("/") + 1);
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                                          <Eq>\r\n                                                             <FieldRef Name='ListName' />\r\n                                                             <Value Type='Text'>{0}</Value>\r\n                                                          </Eq>\r\n                                                       </Where>", str3)
                    };
                    SPListItem item = (list2.GetItems(query).Count > 0) ? list2.GetItems(query)[0] : null;
                    if (item != null)
                    {
                        string str4 = (item["PermissionField"] != null) ? item["PermissionField"].ToString() : "";
                        string str5 = (item["PermissionLookupList"] != null) ? item["PermissionLookupList"].ToString() : "";
                        string str6 = (item["PermissionLookupListField"] != null) ? item["PermissionLookupListField"].ToString() : "";
                        if (str5 != "")
                        {
                            perLookupField = str6;
                        }
                        else if (str4 != "")
                        {
                            perLookupField = str4;
                        }
                    }
                }
            }
        });
        return perLookupField;
    }

    public static int GetRelatedUser(SPWeb web, int userLookupId, int contractId, int areaId, int currentUserId)
    {
        SPList list = web.GetList("/Lists/Contracts");
        SPList list2 = web.GetList("/Lists/Areas");
        SPList list3 = web.GetList("/Lists/ContractUsers");
        SPListItem item = (contractId > 0) ? list.GetItemById(contractId) : null;
        SPListItem itemById = list3.GetItemById(userLookupId);
        if (userLookupId == 1)
        {
            return new SPFieldLookupValue(item["ContractorUser"].ToString()).LookupId;
        }
        if (userLookupId == 2)
        {
            return new SPFieldLookupValue(item["ConsultantUser"].ToString()).LookupId;
        }
        if (userLookupId == 4)
        {
            return new SPFieldLookupValue(item["ManagerUser"].ToString()).LookupId;
        }
        if (userLookupId == 5)
        {
            return new SPFieldLookupValue(list2.GetItemById(new SPFieldLookupValue(item["Area"].ToString()).LookupId)["AreaManagerUser"].ToString()).LookupId;
        }
        if (userLookupId == 9)
        {
            SPQuery query = new SPQuery {
                Query = string.Format("<Where>\r\n                                                          <Eq>\r\n                                                                <FieldRef Name='Company' LookupId='TRUE' />\r\n                                                                <Value Type='Lookup'>{0}</Value>\r\n                                                            </Eq>\r\n                                                        </Where>", contractId)
            };
            SPListItem item3 = list2.GetItems(query)[0];
            return new SPFieldLookupValue(item3["AreaManagerUser"].ToString()).LookupId;
        }
        if (userLookupId == 12)
        {
            return currentUserId;
        }
        if (userLookupId == 13)
        {
            return new SPFieldLookupValue(list2.GetItemById(new SPFieldLookupValue(item["Area"].ToString()).LookupId)["ExperienceManager"].ToString()).LookupId;
        }
        if (userLookupId == 0x16)
        {
            return new SPFieldLookupValue(list2.GetItemById(areaId)["CManagerUser"].ToString()).LookupId;
        }
        if (userLookupId == 0x17)
        {
            return new SPFieldLookupValue(list2.GetItemById(areaId)["AreaManagerUser"].ToString()).LookupId;
        }
        return new SPFieldLookupValue(itemById["UserName"].ToString()).LookupId;
    }

    public static string Reject(SPWeb web, string comment, string listId, int itemId, int currentUserID)
    {
        string stReject = "";
        Guid siteID = web.Site.ID;
        int canApprove = -1;
        int creatorId = 0;
        int approver1Id = 0;
        int approver2Id = 0;
        int approver3Id = 0;
        int approver4Id = 0;
        int approver5Id = 0;
        int contractId = 0;
        int areaId = 0;
        string status = "";
        int nextUserId = 0;
        List<StepFields> list = CanApprove(web, listId, itemId, out canApprove);
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteID))
            {
                using (SPWeb web1 = site.OpenWeb())
                {
                    Exception exception;
                    SPList masterList = web1.Lists[new Guid(listId)];
                    string url = masterList.RootFolder.Url;
                    string str2 = url.Substring(url.LastIndexOf("/") + 1);
                    SPListItem item = masterList.GetItemById(itemId);
                    int lookupId = new SPFieldLookupValue(item["Author"].ToString()).LookupId;
                    string permissionFieldLookup = GetPermissionFieldLookup(web, masterList.ID.ToString());
                    int permissionFieldValue = 0;
                    if (permissionFieldLookup != "")
                    {
                        permissionFieldValue = new SPFieldLookupValue(item[permissionFieldLookup].ToString()).LookupId;
                    }
                    try
                    {
                        areaId = new SPFieldLookupValue(item["Area"].ToString()).LookupId;
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                    }
                    GetContractUsers(web, listId, permissionFieldValue, areaId, out contractId, out creatorId, out approver1Id, out approver2Id, out approver3Id, out approver4Id, out approver5Id);
                    try
                    {
                        if ((canApprove == 1) && (currentUserID == approver1Id))
                        {
                            status = "در انتظار ویرایش";
                            nextUserId = creatorId;
                            SetListItemPermission(item, nextUserId, 0x40000003, false);
                        }
                        else if (canApprove == 2)
                        {
                            status = "در انتظار تایید";
                            nextUserId = approver1Id;
                        }
                        else if (canApprove == 3)
                        {
                            status = "در انتظار تایید";
                            nextUserId = approver2Id;
                        }
                        else if (canApprove == 4)
                        {
                            status = "در انتظار تایید";
                            nextUserId = approver3Id;
                        }
                        else if (canApprove == 5)
                        {
                            status = "در انتظار تایید";
                            nextUserId = approver4Id;
                        }
                        else
                        {
                            stReject = "شما دسترسی لازم را ندارید.";
                        }
                        if (stReject == "")
                        {
                            string str4;
                            web1.AllowUnsafeUpdates = true;
                            item["Status"] = status;
                            item["CurrentUser"] = (nextUserId != 0) ? new SPFieldUserValue(web, nextUserId, "") : null;
                            item.Update();
                            CreateHistory(web1, item.ID, DateTime.Now.ToString(), "رد اطلاعات", comment, item.Url.Split(new char[] { '/' })[1], currentUserID);
                            if (canApprove != 1)
                            {
                                str4 = string.Concat(new object[] { "/_Layouts/15/testNew/dist/display/DisplayForm.aspx?ListName=", str2, "&ID=", item.ID });
                            }
                            else
                            {
                                str4 = string.Concat(new object[] { "/_Layouts/15/testNew/dist/edit/EditForm.aspx?ListName=", str2, "&ID=", item.ID });
                            }
                            web1.AllowUnsafeUpdates = false;
                        }
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                        stReject = exception.Message;
                    }
                }
            }
        });
        return stReject;
    }

    public static void ResetItemPermission(SPListItem item)
    {
        string siteURL = item.ParentList.ParentWeb.Url;
        Guid listId = item.ParentList.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPListItem itemById = web.Lists[listId].GetItemById(item.ID);
                    web.AllowUnsafeUpdates = true;
                    itemById.ResetRoleInheritance();
                    web.AllowUnsafeUpdates = false;
                }
            }
        });
    }

    private static SPListItem setItemFields(List<SPFieldValue> fields, SPListItem item)
    {
        foreach (SPFieldValue value2 in fields)
        {
            string[] strArray;
            SPFieldLookupValueCollection values;
            int num;
            switch (value2.Type)
            {
                case "Text":
                {
                    item[value2.InternalName] = value2.value;
                    continue;
                }
                case "Note":
                {
                    item[value2.InternalName] = value2.value;
                    continue;
                }
                case "Number":
                {
                    item[value2.InternalName] = decimal.Parse(value2.value);
                    continue;
                }
                case "DateTime":
                {
                   // item[value2.InternalName] = Convert.ToDateTime(value2.value);
                    CultureInfo culture = new CultureInfo("en-US");

                    item[value2.InternalName] = Convert.ToDateTime(value2.value, culture);
                    continue;
                }
                case "Lookup":
                {
                    item[value2.InternalName] = new SPFieldLookupValue(int.Parse(value2.value), "");
                    continue;
                }
                case "LookupMulti":
                    strArray = value2.value.Split(new char[] { ',' });
                    values = new SPFieldLookupValueCollection();
                    num = 0;
                    goto Label_0216;

                case "RelatedCustomLookupQuery":
                {
                    item[value2.InternalName] = new SPFieldLookupValue(int.Parse(value2.value), "");
                    continue;
                }
                case "File":
                {
                    item[value2.InternalName] = new SPFieldLookupValue(int.Parse(value2.value), "");
                    continue;
                }
                case "CustomComputedField":
                {
                    continue;
                }
                case "Choice":
                {
                    item[value2.InternalName] = value2.value;
                    continue;
                }
                case "MultiChoice":
                {
                    string[] strArray2 = value2.value.Split(new char[] { ',' });
                    SPFieldMultiChoiceValue value3 = new SPFieldMultiChoiceValue();
                    num = 0;
                    while (num < strArray2.Length)
                    {
                        value3.Add(strArray2[num]);
                        num++;
                    }
                    item[value2.InternalName] = value3;
                    continue;
                }
                case "Boolean":
                {
                    item[value2.InternalName] = value2.value;
                    continue;
                }
                case "User":
                {
                    if (value2.value == null)
                    {
                        goto Label_034A;
                    }
                    item[value2.InternalName] = new SPFieldUserValue(SPContext.Current.Web, int.Parse(value2.value), "");
                    continue;
                }
                default:
                {
                    continue;
                }
            }
        Label_01F7:
            values.Add(new SPFieldLookupValue(int.Parse(strArray[num]), ""));
            num++;
        Label_0216:
            if (num < strArray.Length)
            {
                goto Label_01F7;
            }
            item[value2.InternalName] = values;
            continue;
        Label_034A:
            item[value2.InternalName] = null;
        }
        return item;
    }

    public static string SetListItemPermission(SPListItem Item, int userId, int PermissionID, bool ClearPreviousPermissions)
    {
        string strError = "";
        string siteURL = Item.ParentList.ParentWeb.Url;
        Guid listId = Item.ParentList.ID;
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    SPPrincipal byID;
                    Exception exception;
                    web.AllowUnsafeUpdates = true;
                    SPListItem itemById = web.Lists[listId].GetItemById(Item.ID);
                    if (!itemById.HasUniqueRoleAssignments)
                    {
                        itemById.BreakRoleInheritance(!ClearPreviousPermissions);
                    }
                    try
                    {
                        byID = web.SiteUsers.GetByID(userId);
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        byID = web.SiteGroups.GetByID(userId);
                    }
                    SPRoleAssignment roleAssignment = new SPRoleAssignment(byID);
                    SPRoleDefinition roleDefinition = web.RoleDefinitions.GetById(PermissionID);
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                    itemById.RoleAssignments.Remove(byID);
                    itemById.RoleAssignments.Add(roleAssignment);
                    try
                    {
                        itemById.SystemUpdate(false);
                    }
                    catch (Exception exception2)
                    {
                        exception = exception2;
                        strError = exception.Message;
                    }
                }
            }
        });
        return strError;
    }

    public static string UpdateFiles(SPWeb web, int contractId, List<SPFieldValue> fields, List<Attachment> addFiles, List<Attachment> deleteFiles)
    {
        string message = "ok";
        try
        {
            Guid siteId = web.Site.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate {
                using (SPSite site = new SPSite(siteId))
                {
                    using (SPWeb Web = site.OpenWeb())
                    {
                        SPList list;
                        foreach (Attachment attachment in deleteFiles)
                        {
                            list = Web.Lists[new Guid(attachment.LookupList)];
                            Web.AllowUnsafeUpdates = true;
                            SPFolder folder = Web.GetFolder(list.RootFolder + contractId.ToString());
                            if (!folder.Exists)
                            {
                                list.RootFolder.Files[attachment.FileName].Delete();
                            }
                            else
                            {
                                folder.Files[attachment.FileName].Delete();
                            }
                            list.RootFolder.Update();
                            Web.AllowUnsafeUpdates = false;
                        }
                        foreach (Attachment attachment in addFiles)
                        {
                            list = web.Lists[new Guid(attachment.LookupList)];
                            byte[] buffer = Convert.FromBase64String(attachment.Content);
                            if (!string.IsNullOrEmpty(attachment.FileName))
                            {
                                web.AllowUnsafeUpdates = true;
                                SPFile file = null;
                                if (contractId == 0)
                                {
                                    file = list.RootFolder.Files.Add(attachment.FileName, buffer, false);
                                }
                                else
                                {
                                    SPFolder folder2 = web.GetFolder(list.RootFolder + "/" + contractId.ToString());
                                    if (!folder2.Exists)
                                    {
                                        SPListItem item = list.Folders.Add(list.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, contractId.ToString());
                                        item.Update();
                                        file = item.Folder.Files.Add(attachment.FileName, buffer, false);
                                    }
                                    else
                                    {
                                        file = folder2.Files.Add(attachment.FileName, buffer, false);
                                    }
                                }
                                file.Item["Title"] = attachment.Title;
                                file.Item.Update();
                                web.AllowUnsafeUpdates = false;
                                SPFieldValue value2 = new SPFieldValue {
                                    InternalName = attachment.InternalName,
                                    Type = "File",
                                    LookupList = attachment.LookupList,
                                    value = file.Item.ID.ToString()
                                };
                                fields.Add(value2);
                            }
                        }
                    }
                }
            });
        }
        catch (Exception exception)
        {
            message = exception.Message;
        }
        return message;
    }
}

 

 

}

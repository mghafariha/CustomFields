using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.Services;
using System.Collections.Generic;
using BaseSolution.Classes;
using System.Web.Script.Serialization;
using System.Linq;
using System.Xml;

namespace BaseSolution.Layouts.BaseSolution
{
   public partial class Services : LayoutsPageBase
{
    // Methods
    [WebMethod]
    public static string Approve(int itemId, string comment, string listId, List<SPFieldValue> fields, List<Attachment> addFiles)
    {
        string str = "";
        SPWeb web = SPContext.Current.Web;
        int iD = web.CurrentUser.ID;

        SPListItem item = web.Lists[new Guid(listId)].GetItemById(itemId);
      //  item["DateConsultant"] = Convert.ToDateTime(fields.FirstOrDefault(a => a.InternalName == "DateConsultant").value);
      //  fields.FirstOrDefault(a => a.InternalName=="DateTime").value.da

     //   item.Update();
       str = Utility.Approve(web, comment, listId, itemId, iD, fields, addFiles);
        if (str == "")
        {
            return "OK";
        }
        return str;
    }

    [WebMethod]
    public static string GetData(string listId, string fieldName, int value, string select)
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
        SPQuery query = new SPQuery {
            Query = string.Format("<Where>\r\n                                              <Eq>\r\n                                                 <FieldRef Name='{0}'  LookupId='TRUE'/>\r\n                                                 <Value Type='Lookup'>{1}</Value>\r\n                                              </Eq>\r\n                                           </Where>", fieldName, value),
            ViewAttributes = "Scope='RecursiveAll'"
        };
        SPListItemCollection items = list.GetItems(query);
        string[] strArray = select.Split(new char[] { ',' });
        string str = "[";
        List<object> list2 = new List<object>();
        foreach (SPListItem item in items)
        {
            str = str + "{";
            foreach (string str2 in strArray)
            {
                string str4 = str;
                str = str4 + "\"" + str2 + "\" : \"" + Convert.ToString(item[str2]) + "\",";
            }
            str = str.TrimEnd(new char[] { ',' });
            str = str + "},";
        }
        return (str.TrimEnd(new char[] { ',' }) + "]");
    }

    [WebMethod]
    public static object GetFieldsList(string listId, string itemId, string formType, string contentTypeId)
    {
        SPWeb web;
        Predicate<SPFieldGeneral> match = null;
        Func<StepFields, bool> predicate = null;
        Func<SPFieldGeneral, bool> func2 = null;
        Predicate<SPFieldGeneral> predicate2 = null;
        List<HistoryDetail> list = new List<HistoryDetail>();
        try
        {
            web = SPContext.Current.Web;
            SPUser currentUser = web.CurrentUser;
        }
        catch (Exception)
        {
            web = new SPSite("http://net-sp:100").OpenWeb();
        }
        SPList list2 = web.Lists[new Guid(listId)];
        string str = list2.RootFolder.ServerRelativeUrl.ToString();
        string listName = str.Substring(str.LastIndexOf("/") + 1);
        List<SPFieldGeneral> source = getListFileds(list2, contentTypeId);
        List<SPFieldGeneral> list4 = new List<SPFieldGeneral>();
        int canApprove = -1;
        if (itemId == "")
        {
            return source;
        }
        List<StepFields> stepFields = Utility.CanApprove(web, listId, int.Parse(itemId), out canApprove);
        if ((formType == "Edit") || (formType == "New"))
        {
            List<string> fieldNames = stepFields.Where(a => a.Fields != null).SelectMany(b => b.Fields).ToList();
            source.RemoveAll(a => fieldNames.IndexOf(a.InternalName) != -1);
           
        }
        else if ((formType == "Display") && (canApprove != -1))
        {

            if (canApprove != -1)
            {

                source.RemoveAll(a => stepFields.Where(c => c.Fields != null && c.Step > canApprove).ToList().SelectMany(f => f.Fields).Contains(a.InternalName));

                // stepFields.First(a=>a.Step==stepFields.Max(b=>b.Step)).Fields.Contains("dddd");
                if (stepFields.FirstOrDefault(b => b.Fields != null && b.Step == canApprove) != null)
                {
                    list4 = source.Where(a => stepFields.First(b => b.Fields != null && b.Step == canApprove).Fields.IndexOf(a.InternalName) != -1).ToList();
                    source.RemoveAll(a => stepFields.First(b => b.Fields != null && b.Step == canApprove).Fields.IndexOf(a.InternalName) != -1);
                }
            }
            
        }
        return new { 
            canApprove = canApprove,
            fields = source,
            approveFields = list4,
            histories = ((itemId != "0") && (itemId != "")) ? GetHistories(int.Parse(itemId), listName) : null
        };
    }

    public static List<HistoryDetail> GetHistories(int itemId, string listName)
    {
        return Utility.GetHistory(SPContext.Current.Web, itemId, listName);
    }

    private static string GetItemPermissionFromContract(SPWeb web, SPList list, int permissionFieldValue, int areaId, int currentUserId, out List<int> viewersIds, out List<int> editorsIds)
    {
        string serverRelativeUrl = list.RootFolder.ServerRelativeUrl;
        string listName = serverRelativeUrl.Substring(serverRelativeUrl.LastIndexOf("/") + 1);
        Guid siteID = web.Site.ID;
        List<int> vvids = new List<int>();
        List<int> edids = new List<int>();
        SPSecurity.RunWithElevatedPrivileges(delegate {
            using (SPSite site = new SPSite(siteID))
            {
                using (SPWeb web1 = site.OpenWeb())
                {
                    int contractId = 0;
                    SPList perList = web1.GetList("/Lists/FormPermissions");
                    SPList list2 = web1.GetList("/Lists/Contracts");
                    SPQuery query = new SPQuery {
                        Query = string.Format("<Where>\r\n                                          <Eq>\r\n                                             <FieldRef Name='ListName' />\r\n                                             <Value Type='Text'>{0}</Value>\r\n                                          </Eq>\r\n                                       </Where>", listName)
                    };
                    SPListItem item = (perList.GetItems(query).Count > 0) ? perList.GetItems(query)[0] : null;
                    if (item != null)
                    {
                        int lookupId;
                        string str = (item["PermissionField"] != null) ? item["PermissionField"].ToString() : "";
                        string strUrl = (item["PermissionLookupList"] != null) ? item["PermissionLookupList"].ToString() : "";
                        string str3 = (item["PermissionLookupListField"] != null) ? item["PermissionLookupListField"].ToString() : "";
                        if (strUrl != "")
                        {
                            contractId = new SPFieldLookupValue(web1.GetList(strUrl).GetItemById(permissionFieldValue)[str].ToString()).LookupId;
                        }
                        else if (str != "")
                        {
                            contractId = permissionFieldValue;
                        }
                        int num2 = Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Creator"].ToString()).LookupId, contractId, areaId, currentUserId);
                        int num3 = (item["Approver1"] != null) ? Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Approver1"].ToString()).LookupId, contractId, areaId, currentUserId) : 0;
                        int num4 = (item["Approver2"] != null) ? Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Approver2"].ToString()).LookupId, contractId, areaId, currentUserId) : 0;
                        int num5 = (item["Approver3"] != null) ? Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Approver3"].ToString()).LookupId, contractId, areaId, currentUserId) : 0;
                        int num6 = (item["Approver4"] != null) ? Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Approver4"].ToString()).LookupId, contractId, areaId, currentUserId) : 0;
                        int num7 = (item["Approver5"] != null) ? Utility.GetRelatedUser(web1, new SPFieldLookupValue(item["Approver5"].ToString()).LookupId, contractId, areaId, currentUserId) : 0;
                        SPListItem item3 = (contractId > 0) ? list2.GetItemById(contractId) : null;
                        if (item3 != null)
                        {
                            SPFieldUserValueCollection values = (item3["Viewers"] != null) ? new SPFieldUserValueCollection(web, item3["Viewers"].ToString()) : null;
                            if (values != null)
                            {
                                foreach (SPFieldUserValue value2 in values)
                                {
                                    lookupId = value2.LookupId;
                                    vvids.Add(lookupId);
                                }
                            }
                        }
                        SPFieldLookupValueCollection values2 = (item["Viewers"] != null) ? new SPFieldLookupValueCollection(item["Viewers"].ToString()) : null;
                        if (values2 != null)
                        {
                            foreach (SPFieldLookupValue value3 in values2)
                            {
                                lookupId = Utility.GetRelatedUser(web1, value3.LookupId, contractId, areaId, currentUserId);
                                vvids.Add(lookupId);
                            }
                        }
                        SPFieldLookupValueCollection values3 = (item["Editors"] != null) ? new SPFieldLookupValueCollection(item["Editors"].ToString()) : null;
                        if (values3 != null)
                        {
                            foreach (SPFieldLookupValue value3 in values3)
                            {
                                lookupId = Utility.GetRelatedUser(web1, value3.LookupId, contractId, areaId, currentUserId);
                                edids.Add(lookupId);
                            }
                        }
                        vvids.Add(num2);
                        if (num3 != 0)
                        {
                            vvids.Add(num3);
                        }
                        if (num4 != 0)
                        {
                            vvids.Add(num4);
                        }
                        if (num5 != 0)
                        {
                            vvids.Add(num5);
                        }
                        if (num6 != 0)
                        {
                            vvids.Add(num6);
                        }
                        if (num7 != 0)
                        {
                            vvids.Add(num7);
                        }
                        int iD = web1.Groups["تیم راهبری"].ID;
                        vvids.Add(iD);
                        int num10 = web1.Groups["تیم راهبری-ویرایش"].ID;
                        edids.Add(num10);
                    }
                }
            }
        });
        viewersIds = vvids;
        editorsIds = edids;
        return "";
    }

    private static List<SPFieldGeneral> getListFileds(SPList list, string contentTypeId)
    {
        SPContentType type = list.ContentTypes[contentTypeId];
        List<SPFieldGeneral> list2 = new List<SPFieldGeneral>();
        SPFieldCollection fields = (contentTypeId != "") ? list.ContentTypes[new SPContentTypeId(contentTypeId)].Fields : list.ContentTypes[0].Fields;
        foreach (SPField field in fields)
        {
            if ((field.Hidden || (field.FromBaseType && (field.InternalName != "Title"))) || ((field.InternalName == "Status") || (field.InternalName == "CurrentUser")))
            {
                continue;
            }
            SPFieldGeneral general = new SPFieldGeneral();
            string str = field.Type.ToString();
            general.Guid = field.Id;
            general.InternalName = field.InternalName;
            XmlDocument document = new XmlDocument();
            document.LoadXml(field.SchemaXml);
            XmlElement documentElement = document.DocumentElement;
            if (documentElement.HasAttribute("DisplayName"))
            {
                general.Title = documentElement.GetAttribute("DisplayName");
            }
            else
            {
                general.Title = field.Title;
            }
            general.Title = field.GetProperty("DisplayName");
            general.DefaultValue = field.DefaultValue;
            general.IsRequire = field.Required;
            general.Type = field.TypeAsString;
            general.Description = field.Description;
            switch (field.TypeAsString)
            {
                case "Text":
                    general.MaxLength = ((SPFieldText) field).MaxLength;
                    break;

                case "Number":
                    general.MaxValue = ((SPFieldNumber) field).MaximumValue;
                    general.MinValue = ((SPFieldNumber) field).MinimumValue;
                    general.ShowAsPercentage = ((SPFieldNumber) field).ShowAsPercentage;
                    break;

                case "Lookup":
                    general.LookupList = ((SPFieldLookup) field).LookupList.Replace("{", "").Replace("}", "");
                    general.LookupTitleField = "Title";
                    general.LookupValueField = "ID";
                    general.AllowMultipleValue = ((SPFieldLookup) field).AllowMultipleValues;
                    break;

                case "LookupMulti":
                    general.LookupList = ((SPFieldLookup) field).LookupList.Replace("{", "").Replace("}", "");
                    general.LookupTitleField = "Title";
                    general.LookupValueField = "ID";
                    general.AllowMultipleValue = ((SPFieldLookup) field).AllowMultipleValues;
                    break;

                case "RelatedCustomLookupQuery":
                    general.AllowMultipleValue = ((SPFieldLookup)field).AllowMultipleValues;
                    general.LookupList = field.GetCustomProperty("ListNameLookup").ToString().Replace("{", "").Replace("}", "");
                    general.LookupTitleField = field.GetCustomProperty("FieldTitleLookup").ToString();
                    general.LookupValueField = field.GetCustomProperty("FieldValueLookup").ToString();
                    general.RelatedFields = field.GetCustomProperty("RelatedFields").ToString().Split(new char[] { '|' });
                    general.Query = field.GetCustomProperty("QueryLookup").ToString();
                    if ((field.GetCustomProperty("IsFile") != null) ? bool.Parse(field.GetCustomProperty("IsFile").ToString()) : false)
                    {
                        general.Type = "File";
                        general.TypeFile = field.GetCustomProperty("TypeFile").ToString();
                        general.VolumeFile = field.GetCustomProperty("VolumeFile").ToString();
                    }
                    break;

                case "MasterDetail":
                    general.LookupList = field.GetCustomProperty("ListNameLookup").ToString();
                    general.RelatedFields = field.GetCustomProperty("RelatedFields").ToString().Split(new char[] { '|' });
                    general.MasterLookupName = field.GetCustomProperty("MasterFieldNameLookup").ToString();
                    break;

                case "CustomComputedField":
                    general.LookupList = field.GetCustomProperty("ListNameQuery").ToString();
                    general.LookupTitleField = field.GetCustomProperty("FieldNameQuery").ToString();
                    general.Query = field.GetCustomProperty("TextQuery").ToString();
                    general.AggregationFunction = field.GetCustomProperty("AggregatorFunction").ToString();
                    break;

                case "Choice":
                {
                    SPFieldChoice choice = (SPFieldChoice) field;
                    general.options = new List<string>();
                    foreach (string str2 in choice.Choices)
                    {
                        general.options.Add(str2);
                    }
                    general.DefaultValue = ((SPFieldChoice) field).DefaultValue;
                    general.AllowMultipleValue = ((SPFieldChoice) field).ListItemMenu;
                    break;
                }
                case "MultiChoice":
                {
                    SPFieldMultiChoice choice2 = (SPFieldMultiChoice) field;
                    general.options = new List<string>();
                    foreach (string str2 in choice2.Choices)
                    {
                        general.options.Add(str2);
                    }
                    general.AllowMultipleValue = ((SPFieldMultiChoice) field).ListItemMenu;
                    break;
                }
            }
            list2.Add(general);
        }
        SPField fieldByInternalName = list.Fields.GetFieldByInternalName("ID");
        SPFieldGeneral item = new SPFieldGeneral {
            Guid = fieldByInternalName.Id,
            InternalName = fieldByInternalName.InternalName,
            Title = fieldByInternalName.Title,
            DefaultValue = fieldByInternalName.DefaultValue,
            IsRequire = fieldByInternalName.Required,
            Type = fieldByInternalName.TypeAsString,
            Description = fieldByInternalName.Description
        };
        list2.Add(item);
        return list2;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }

    [WebMethod(EnableSession=true)]
    public static string Reject(int itemId, string comment, string listId)
    {
        string str = "";
        SPWeb web = SPContext.Current.Web;
        int iD = web.CurrentUser.ID;
        str = Utility.Reject(web, comment, listId, itemId, iD);
        if (str == "")
        {
            return "OK";
        }
        return str;
    }

    [WebMethod]
    public static string SaveFieldItems(string guid, List<SPFieldValue> fields, List<SPListItemDelete> deletedItems, List<Attachment> addFiles, List<Attachment> deleteFiles)
    {
        List<ErrorMessage> list = new List<ErrorMessage>();
        string s = "ok";
        string sourceValue = "";
        string destValue = "";
        string compareAction = "";
        string[] sourceFieldArray = null;
        List<int> viewersIds = new List<int>();
        List<int> editorsIds = new List<int>();
        List<SPItemSave> saveItems = new List<SPItemSave>();
        int creatorId = 0;
        int num2 = 0;
        int num3 = 0;
        int num4 = 0;
        int num5 = 0;
        int num6 = 0;
        int contractId = 0;
        SPWeb web = SPContext.Current.Web;
        SPList list4 = web.Lists[new Guid(guid)];
        string url = list4.RootFolder.Url;
        string listname = url.Substring(url.LastIndexOf('/') + 1);
        foreach (SPListItemDelete delete in deletedItems)
        {
            SPList list5 = web.Lists[new Guid(delete.ListId)];
            s = Utility.DeleteItemFromList(list5, delete.ItemId);
            if (s != "ok")
            {
                return s;
            }
        }
        SPFieldValue value2 = fields.FirstOrDefault<SPFieldValue>(a => a.InternalName == "ID");
        SPListItem item = (value2.value != "0") ? list4.GetItemById(int.Parse(value2.value)) : null;
        string permissionField = Utility.GetPermissionFieldLookup(web, guid);
        SPFieldValue value3 = fields.FirstOrDefault<SPFieldValue>(a => a.InternalName == permissionField);
        SPFieldValue value4 = fields.FirstOrDefault<SPFieldValue>(a => a.InternalName == "Area");
        Utility.GetContractUsers(web, guid, (value3 != null) ? int.Parse(value3.value) : 0, (value4 != null) ? int.Parse(value4.value) : 0, out contractId, out creatorId, out num2, out num3, out num4, out num5, out num6);
       
        
        if ((value2.value == "0") || ((value2.value != "0") && ( item["CurrentUser"]!=null && new SPFieldLookupValue(item["CurrentUser"].ToString()).LookupId == web.CurrentUser.ID)))
        {
            SPFieldValue value5 = new SPFieldValue
            {
                InternalName = "Status",
                Type = "Text",
                value = (num2 > 0) ? "در انتظار تایید" : "پایان فرآیند"
            };
            fields.Add(value5);
            SPFieldValue value6 = new SPFieldValue
            {
                InternalName = "CurrentUser",
                Type = "User",
                value = (num2 > 0) ? num2.ToString() : "0"
            };
            fields.Add(value6);
        }
       
        GetItemPermissionFromContract(web, list4, (value3 != null) ? int.Parse(value3.value) : 0, (value4 != null) ? int.Parse(value4.value) : 0, web.CurrentUser.ID, out viewersIds, out editorsIds);
        if (s == "ok")
        {
            List<SPFieldValue> list6 = (from a in fields
                where a.Type == "MasterDetail"
                select a).ToList<SPFieldValue>();
            SPListItemCollection items = Utility.FindValidation(web, list4.ID);
           



            foreach (SPListItem item2 in items)
            {

                List<string> fieldNames = item2["SourceField"].ToString().Split(',').ToList();
                List<SPFieldValue> checkField = fields.Where(a => fieldNames.Any(b => b == a.InternalName)).ToList();
                if (checkField.Count > 0)
                {
                    Utility.CheckValidation(web, item2, fields, out sourceFieldArray, out sourceValue, out destValue, out compareAction);
                    if (((destValue != "") && (sourceValue != "")) && !Utility.CompareCheck(compareAction, sourceValue, destValue))
                    {
                        ErrorMessage message = new ErrorMessage {
                            FieldNames = fieldNames,
                            Message = item2["Message"].ToString(),
                            RowNumber = -1
                        };
                        list.Add(message);
                    }
                }
            }
            foreach (SPFieldValue value7 in list6)
            {
                SPList list8 = web.Lists[new Guid(value7.LookupList)];
                SPListItemCollection items2 = Utility.FindValidation(web, list8.ID);
                int num7 = 0;
                foreach (List<SPFieldValue> list9 in value7.rows)
                {
                    foreach (SPListItem item2 in items2)
                    {
                        Utility.CheckValidation(web, item2, list9, out sourceFieldArray, out sourceValue, out destValue, out compareAction);
                        if (!Utility.CompareCheck(compareAction, sourceValue, destValue))
                        {
                            List<string> list10 = sourceFieldArray.ToList<string>();
                            ErrorMessage message2 = new ErrorMessage {
                                FieldNames = list10,
                                Message = item2["Message"].ToString(),
                                RowNumber = num7
                            };
                            list.Add(message2);
                        }
                    }
                    num7++;
                }
            }
            if (list.Count > 0)
            {
                return new JavaScriptSerializer().Serialize(list);
            }
            int result = 0;
            s = Utility.UpdateFiles(web, contractId, fields, addFiles, deleteFiles);
            if (s != "ok")
            {
                return s;
            }
            SPListItem item4 = (int.Parse(value2.value) > 0) ? list4.GetItemById(int.Parse(value2.value)) : list4.AddItem((contractId > 0) ? (list4.RootFolder.Url + "/" + contractId.ToString()) : list4.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.File);
            SPListItem item3 = setItemFields(fields, item4);
            SPItemSave save = new SPItemSave {
                ListId = guid,
                Item = item3,
                Folder = web.GetFolder((contractId > 0) ? (list4.RootFolder.Url + "/" + contractId.ToString()) : list4.RootFolder.ServerRelativeUrl)
            };
            s = SaveItem(web, save.ListId, save.Item, save.Folder, contractId, web.CurrentUser.ID, viewersIds, editorsIds, creatorId);
            int.TryParse(s, out result);
            if (result > 0)
            {
                s = "ok";
                using (List<SPFieldValue>.Enumerator enumerator3 = list6.GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        SPFieldValue detailField = enumerator3.Current;
                        SPFieldValue lk = new SPFieldValue {
                            InternalName = detailField.value,
                            Type = "Lookup",
                            value = result.ToString()
                        };
                        Guid siteID = web.Site.ID;
                        SPSecurity.RunWithElevatedPrivileges(delegate {
                            using (SPSite site = new SPSite(siteID))
                            {
                                using (SPWeb web1 = site.OpenWeb())
                                {
                                    SPList detailList = web1.Lists[new Guid(detailField.LookupList)];
                                    SPListItemCollection detailItems = Utility.FindValidation(web, detailList.ID);
                                    SPFolder folder = web1.GetFolder((contractId > 0) ? (detailList.RootFolder.Url + "/" + contractId.ToString()) : detailList.RootFolder.ServerRelativeUrl);
                                    foreach (List<SPFieldValue> list2 in detailField.rows)
                                    {
                                        list2.Add(lk);
                                        SPFieldValue idValue = list2.FirstOrDefault<SPFieldValue>(a => a.InternalName == "ID");
                                        SPListItem detailItem = setItemFields(list2, (int.Parse(value2.value) > 0) ? detailList.GetItemById(int.Parse(idValue.value)) : detailList.AddItem(folder.Url, SPFileSystemObjectType.File));
                                        SPItemSave detailsave = new SPItemSave {
                                            ListId = detailField.LookupList,
                                            Item = detailItem,
                                            Folder = folder
                                        };
                                        saveItems.Add(detailsave);
                                    }
                                }
                            }
                        });
                    }
                }
            }
            string str7 = "";
            int num10 = 0;
            foreach (SPItemSave save2 in saveItems)
            {
                str7 = SaveItem(web, save2.ListId, save2.Item, save2.Folder, contractId, web.CurrentUser.ID, viewersIds, editorsIds, 0);
            }
            int.TryParse(str7, out num10);
            if ((num10 > 0) || ((s == "ok") && (saveItems.Count == 0)))
            {
                s = Utility.CreateHistory(web, result, DateTime.Now.ToString(), "ثبت", "", listname, web.CurrentUser.ID);
                if (s == "")
                {
                    s = "ok";
                }
            }
        }
        return s;
    }

    private static string SaveItem(SPWeb web, string guid, SPListItem item, SPFolder spFolder, int contractId, int currentUserId, List<int> viewers, List<int> editors, int adder)
    {
        SPSecurity.CodeToRunElevated secureCode = null;
        string str = "";
        Guid siteID = web.Site.ID;
        try
        {
            if (secureCode == null)
            {
                secureCode = delegate {
                    using (SPSite site = new SPSite(siteID))
                    {
                        using (SPWeb Web = site.OpenWeb())
                        {
                            SPList list = Web.Lists[new Guid(guid)];
                            if (!spFolder.Exists)
                            {
                                SPListItem folderItem = list.Items.Add(list.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder);
                                folderItem["Title"] = contractId.ToString();
                                Web.AllowUnsafeUpdates = true;
                                folderItem.Update();
                                int num = 0;
                                foreach (int num2 in viewers)
                                {
                                    if (num == 0)
                                    {
                                        Utility.SetListItemPermission(folderItem, num2, 0x40000002, true);
                                    }
                                    else
                                    {
                                        Utility.SetListItemPermission(folderItem, num2, 0x40000002, false);
                                    }
                                    num++;
                                }
                                foreach (int num3 in editors)
                                {
                                    if (num == 0)
                                    {
                                        Utility.SetListItemPermission(folderItem, num3, 0x40000003, true);
                                    }
                                    else
                                    {
                                        Utility.SetListItemPermission(folderItem, num3, 0x40000003, false);
                                    }
                                    num++;
                                }
                                if (adder != 0)
                                {
                                    Utility.SetListItemPermission(folderItem, adder, 0x4000006b, false);
                                }
                                spFolder = web.GetFolder(list.RootFolder.Url + "/" + contractId);
                            }
                        }
                    }
                };
            }
            SPSecurity.RunWithElevatedPrivileges(secureCode);
            SPWeb web2 = item.Web;
            web2.AllowUnsafeUpdates = true;
            item.Update();
            if (contractId != 0)
            {
                Utility.ResetItemPermission(item);
            }
            else
            {
                int num = 0;
                foreach (int num2 in viewers)
                {
                    if (num == 0)
                    {
                        Utility.SetListItemPermission(item, num2, 0x40000002, true);
                    }
                    else
                    {
                        Utility.SetListItemPermission(item, num2, 0x40000002, false);
                    }
                    num++;
                }
                foreach (int num3 in editors)
                {
                    if (num == 0)
                    {
                        Utility.SetListItemPermission(item, num3, 0x40000003, true);
                    }
                    else
                    {
                        Utility.SetListItemPermission(item, num3, 0x40000003, false);
                    }
                    num++;
                }
            }
            str = item.ID.ToString();
            web2.AllowUnsafeUpdates = false;
        }
        catch (Exception exception)
        {
            return exception.Message;
        }
        return str;
    }

    private static void SetFieldValue(SPListItem item, SPFieldValue fieldValue)
    {
        int num;
        switch (fieldValue.Type)
        {
            case "Text":
                item[fieldValue.InternalName] = fieldValue.value;
                break;

            case "Note":
                item[fieldValue.InternalName] = fieldValue.value;
                break;

            case "Number":
                item[fieldValue.InternalName] = decimal.Parse(fieldValue.value);
                break;

            case "DateTime":
                item[fieldValue.InternalName] = Convert.ToDateTime(fieldValue.value);
                break;

            case "Lookup":
                item[fieldValue.InternalName] = new SPFieldLookupValue(int.Parse(fieldValue.value), "");
                break;

            case "LookupMulti":
            {
                string[] strArray = fieldValue.value.Split(new char[] { ',' });
                SPFieldLookupValueCollection values = new SPFieldLookupValueCollection();
                num = 0;
                while (num < strArray.Length)
                {
                    values.Add(new SPFieldLookupValue(int.Parse(strArray[num]), ""));
                    num++;
                }
                item[fieldValue.InternalName] = values;
                break;
            }
            case "RelatedCustomLookupQuery":
                item[fieldValue.InternalName] = new SPFieldLookupValue(int.Parse(fieldValue.value), "");
                break;
            case "CustomComputedField":
                item[fieldValue.InternalName] = new SPFieldLookupValue(int.Parse(fieldValue.value), "");
                break;

            case "Choice":
                item[fieldValue.InternalName] = fieldValue.value;
                break;

            case "MultiChoice":
            {
                string[] strArray2 = fieldValue.value.Split(new char[] { ',' });
                SPFieldMultiChoiceValue value2 = new SPFieldMultiChoiceValue();
                for (num = 0; num < strArray2.Length; num++)
                {
                    value2.Add(strArray2[num]);
                }
                item[fieldValue.InternalName] = value2;
                break;
            }
            case "Boolean":
                item[fieldValue.InternalName] = fieldValue.value;
                break;
        }
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
                    if (value2.value == "")
                        item[value2.InternalName] = null;
                    else
                        item[value2.InternalName] = Convert.ToDateTime(value2.value);
                    continue;
                }
                case "Lookup":
                {
                    item[value2.InternalName] = ((value2.value != "") && (value2.value != "0")) ? new SPFieldLookupValue(int.Parse(value2.value), "") : null;
                    continue;
                }
                case "LookupMulti":
                    strArray = value2.value.Split(new char[] { ',' });
                    values = new SPFieldLookupValueCollection();
                    num = 0;
                    goto Label_0262;

                case "RelatedCustomLookupQuery":
                {
                   
                    strArray = value2.value.Split(new char[] { ',' });
                    if (strArray.Count() > 0)
                    {
                        values = new SPFieldLookupValueCollection();
                        num = 0;
                        goto Label_0262;
                    }
                    else
                    item[value2.InternalName] = ((value2.value != "") && (value2.value != "0")) ? new SPFieldLookupValue(int.Parse(value2.value), "") : null;
                    continue;
                }
                case "CustomComputedField":
                {
                    item[value2.InternalName] = value2.value;
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
                    item[value2.InternalName] = ((value2.value != "") && (value2.value != "0")) ? new SPFieldUserValue(SPContext.Current.Web, int.Parse(value2.value), "") : null;
                    continue;
                }
                case "File":
                {
                    item[value2.InternalName] = ((value2.value != "") && (value2.value != "0")) ? new SPFieldLookupValue(int.Parse(value2.value), "") : null;
                    continue;
                }
                default:
                {
                    continue;
                }
            }
        Label_0243:
            values.Add(new SPFieldLookupValue(int.Parse(strArray[num]), ""));
            num++;
        Label_0262:
            if (num < strArray.Length)
            {
                goto Label_0243;
            }
            item[value2.InternalName] = values;
        }
        return item;
    }
}

 

 

}

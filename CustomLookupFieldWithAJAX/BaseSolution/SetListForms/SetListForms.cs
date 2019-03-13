using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace BaseSolution.SetListForms
{
    /// <summary>
    /// List Events
    /// </summary>
    public class SetListForms : SPListEventReceiver
    {
        // Methods
        public override void ListAdded(SPListEventProperties properties)
        {
            base.ListAdded(properties);
            SPList list = properties.List;
            SPList list2 = properties.Web.GetList("/Lists/Contracts");
            SPContentType type = list.ContentTypes[0];
            type.NewFormUrl = "/_Layouts/15/FrameWork/Pages/NewForm/index.html";
            type.EditFormUrl = "/_Layouts/15/FrameWork/Pages/EditForm/index.html";
            type.DisplayFormUrl = "/_Layouts/15/FrameWork/Pages/DisplayForm/index.html";
            type.Update();
            list.Update();
        }
    }
}


 

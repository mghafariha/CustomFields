using System;
using System.Web.Services;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using System.Collections.Generic;
using BaseSolution.Classes;

namespace BaseSolution.Layouts.BaseSolution
{
   public partial class Service2 : LayoutsPageBase
{
    // Methods
    [WebMethod]
    public static string GetFieldsList1(string listId)
    {
      //  return new List<SPFieldGeneral>();
        return "";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }
}

 

 

}

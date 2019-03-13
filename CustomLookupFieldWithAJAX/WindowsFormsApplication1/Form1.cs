using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.SharePoint;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SPSite site = new SPSite("http://net-sp");
            SPWeb web = site.OpenWeb();
           // SPList list = web.GetList("/Lists/Canalets2");
           //// SPList list2 = properties.Web.GetList("/Lists/Contracts");
           // SPContentType type = list.ContentTypes[0];
           // type.NewFormUrl = "/_Layouts/15/FrameWork/Pages/NewForm/index.html";
           // type.EditFormUrl = "/_Layouts/15/FrameWork/Pages/EditForm/index.html";
           // type.DisplayFormUrl = "/_Layouts/15/FrameWork/Pages/DisplayForm/index.html";
           // type.Update();
           // list.Update();


           // SPList InvoiceCM2 = web.GetList("/Lists/InvoiceCM2");

            
           // // SPList list2 = properties.Web.GetList("/Lists/Contracts");
           // SPContentType type2 = InvoiceCM2.ContentTypes[0];
           // type2.NewFormUrl = "/_Layouts/15/FrameWork/Pages/NewForm/index.html";
           // type2.EditFormUrl = "/_Layouts/15/FrameWork/Pages/EditForm/index.html";
           // type2.DisplayFormUrl = "/_Layouts/15/FrameWork/Pages/DisplayForm/index.html";
           // type2.Update();
           // InvoiceCM2.Update();

            SPList list = web.GetList("/Lists/DefectFailures");
            SPContentType type = list.ContentTypes[0];
            type.NewFormUrl = "/_Layouts/15/FrameWork/Pages/NewForm/index.html";
             type.EditFormUrl = "/_Layouts/15/FrameWork/Pages/EditForm/index.html";
            type.DisplayFormUrl = "/_Layouts/15/FrameWork/Pages/DisplayForm/index.html";
             type.Update();
             list.Update();
        }
    }
}

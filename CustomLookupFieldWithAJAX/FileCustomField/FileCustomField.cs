using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Security;
using System.Security.Permissions;
using System.Windows.Controls;
using System.Globalization;


namespace FileCustomField
{
  public  class FileCustomField:SPFieldText
    {
        public FileCustomField(SPFieldCollection fields, string fieldName) : base(fields, fieldName) {
            
        }
        public FileCustomField(SPFieldCollection fields,string typeName, string fieldName) : base(fields,typeName, fieldName) {
            
        }
        public override BaseFieldControl FieldRenderingControl
        {
            [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
            get
            {
                BaseFieldControl fieldControl = new FileCustomFieldControl();
                fieldControl.FieldName = this.InternalName;

                return fieldControl;
            }
        }
        public override string GetValidatedString(object value)
        {
            if ((this.Required == true)
               &&
               ((value == null)
                ||
               ((String)value == "")))
            {
                throw new SPFieldValidationException(this.Title
                    + " must have a value.");
            }
            else
            {
                ISBN10ValidationRule rule = new ISBN10ValidationRule();
                ValidationResult result = rule.Validate(value, CultureInfo.InvariantCulture);

                if (!result.IsValid)
                {
                    throw new SPFieldValidationException((String)result.ErrorContent);
                }
                else
                {
                    return base.GetValidatedString(value);
                }
            }
        }

    }
}

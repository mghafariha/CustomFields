using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseSolution.Classes
{
   public class BaseValidation
{
    // Fields
    private bool checkValue;
    private string compareFunc;
    private string message;
    private string sourceValue;

    // Methods
    public void CheckValidation(string sourceValue, string destValue, string func)
    {
        this.checkValue = false;
        if ((func == "Greater") && (float.Parse(sourceValue) <= float.Parse(destValue)))
        {
            this.message = "مقدار ";
        }
    }

    // Properties
    public bool CheckValue
    {
        get
        {
            return this.checkValue;
        }
        set
        {
            this.checkValue = value;
        }
    }

    public string CompareAction { get; set; }

    public string DestAction { get; set; }

    public string DestField { get; set; }

    public string DestList { get; set; }

    public string Message
    {
        get
        {
            return this.message;
        }
        set
        {
            this.message = value;
        }
    }

    public string SourceAction { get; set; }

    public string SourceField { get; set; }

    public string SourceList { get; set; }

    public string value { get; set; }
}

 


}

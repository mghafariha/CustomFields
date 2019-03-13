using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Globalization;
using System;

namespace FileCustomField
{
    public class ISBN10ValidationRule : ValidationRule
    {
        private const Int32 ISBNMODULO = 11;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            String iSBN = (String)value;
            String errorMessage = "";

            Regex rxISBN = new Regex(@"^(?'GroupID'\d{1,5})-(?'PubPrefix'\d{1,7})-(?'TitleID'\d{1,6})-(?'CheckDigit'[0-9X]{1})$");

            if (!rxISBN.IsMatch(iSBN))
            {
                errorMessage = "An ISBN must have this structure:\n1-5 digit Group ID, hyphen, \n1-7 digit Publisher Prefix, hyphen, \n1-6 digit Title ID, hyphen, \n1 Check Digit (which can be \"X\" to indicate \"10\").\n";
            }

            if (errorMessage == "") // Matched the RegEx, so check for group length errors.
            {
                Match mISBN = rxISBN.Match(iSBN);
                GroupCollection groupsInString = mISBN.Groups;

                String groupID = groupsInString["GroupID"].Value;
                String pubPrefix = groupsInString["PubPrefix"].Value;

                if ((groupID.Length + pubPrefix.Length) >= 9)
                {
                    errorMessage = "The Group ID and Publisher Prefix can total no more than 8 digits.\n";
                }

                String titleID = groupsInString["TitleID"].Value;

                if (((groupID.Length + pubPrefix.Length) + titleID.Length) != 9)
                {
                    errorMessage = errorMessage + "The Group ID, Publisher Prefix, and \nTitle ID must total exactly 9 digits.\n";
                }

                if (errorMessage == "") //No group length errors, so verify the check digit algorithm.
                {
                    Int32 checkDigitValue;
                    String checkDigit = groupsInString["CheckDigit"].Value;

                    // To ensure check digit is one digit, "10" is represented by "X".
                    if (checkDigit == "X")
                    {
                        checkDigitValue = 10;
                    }
                    else
                    {
                        checkDigitValue = Convert.ToInt32(checkDigit);
                    }

                    String iSBN1st3Groups = groupID + pubPrefix + titleID; //Concatenate without the hyphens.

                    // Sum the weighted digits.
                    //Int32 weightedSum = (10 * Convert.ToInt32(iSBN1st3Groups.Substring(0, 1))) +
                    //                     (9 * Convert.ToInt32(iSBN1st3Groups.Substring(1, 1))) +
                    //                     (8 * Convert.ToInt32(iSBN1st3Groups.Substring(2, 1))) +
                    //                     (7 * Convert.ToInt32(iSBN1st3Groups.Substring(3, 1))) +
                    //                     (6 * Convert.ToInt32(iSBN1st3Groups.Substring(4, 1))) +
                    //                     (5 * Convert.ToInt32(iSBN1st3Groups.Substring(5, 1))) +
                    //                     (4 * Convert.ToInt32(iSBN1st3Groups.Substring(6, 1))) +
                    //                     (3 * Convert.ToInt32(iSBN1st3Groups.Substring(7, 1))) +
                    //                     (2 * Convert.ToInt32(iSBN1st3Groups.Substring(8, 1))) +
                    //                      checkDigitValue;

                    //Int32 remainder = weightedSum % ISBNMODULO;  // ISBN is invalid if weighted sum modulo 11 is not 0.

                    //if (remainder != 0)
                    //{
                    //    errorMessage = "Number fails Check Digit verification.";
                    //}

                    if (errorMessage == "") // Passed check digit verification. 
                    {
                        return new ValidationResult(true, "This is a valid ISBN.");
                    }// end check digit verification passed

                    else // the check digit verification failed
                    {
                        return new ValidationResult(false, errorMessage);
                    }

                }// end no group length errors

                else // There was some error in a group length
                {
                    return new ValidationResult(false, errorMessage);
                }

            }// end RegEx match succeeded

            else // There was a RegEx match failure
            {
                return new ValidationResult(false, errorMessage);
            }

        }// end Validate method 

    }// end ISBN10ValidationRule class
}

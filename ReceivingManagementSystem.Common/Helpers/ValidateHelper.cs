using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ReceivingManagementSystem.Common.Helpers
{
    public class ValidateHelper
    {
        public static bool CheckPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return true;
            }

            string pattern1 = "^\\d{1,9}-\\d{1,9}-\\d{1,9}$";
            string pattern2 = "^\\d{1,11}$";
            Regex regex = new Regex(pattern2);

            if (!regex.IsMatch(phoneNumber))
            {
                regex = new Regex(pattern1);

                if (!regex.IsMatch(phoneNumber))
                {
                    return false;
                }
            }

            phoneNumber = phoneNumber.Replace("-", "");
            if (phoneNumber.Length < 10 || phoneNumber.Length > 11)
            {
                return false;
            }

            return true;

        }
    }
}





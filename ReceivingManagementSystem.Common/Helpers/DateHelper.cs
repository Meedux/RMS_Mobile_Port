using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ReceivingManagementSystem.Common.Helpers
{
    public static class DateHelper
    {
        public static string Date_Format_YYYYMMDD = "yyyy/MM/dd";
        public static string Date_Format_YYYYMMDDHHMMSS = "yyyy/MM/dd HH:mm:ss";

        public static string Date_Format_HHMMSS = "HH:mm:ss";

        public static string ToStringYYYYMMDD(this DateTime value)
        {
            return value.ToString(Date_Format_YYYYMMDD);
        }

        public static string ToSartDateYYYYMMDD(this DateTime value)
        {
            return $"{value.ToString(Date_Format_YYYYMMDD)} 00:00:00";
        }

        public static string ToEndDateYYYYMMDD(this DateTime value)
        {
            return $"{value.ToString(Date_Format_YYYYMMDD)} 23:59:59";
        }

        public static bool CheckInputDate(string inputDate)
        {
            if (string.IsNullOrEmpty(inputDate))
            {
                return false;
            }

            try
            {
                DateTime.ParseExact(inputDate, Date_Format_YYYYMMDD, CultureInfo.CurrentCulture);
            }
            catch (Exception /* ex */)
            {
                return false;
            }

            return true;
        }

        public static DateTime? GetDateByInputDate(string inputDate)
        {
            DateTime? dateTime = null;
            if (string.IsNullOrEmpty(inputDate))
            {
                return dateTime;
            }

            try
            {
                dateTime = DateTime.ParseExact(inputDate, DateHelper.Date_Format_YYYYMMDD, CultureInfo.CurrentCulture);
            }
            catch (Exception /* ex */)
            {
                
            }

            return dateTime;
        }
    }
}

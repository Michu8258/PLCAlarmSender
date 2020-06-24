using System;

namespace RealmDBHandler.CommonClasses
{
    public static class TimeStampConverter
    {
        public static long ConvertToInteger(DateTime timestamp)
        {
            return (Int64)(timestamp.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0))).TotalSeconds;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}

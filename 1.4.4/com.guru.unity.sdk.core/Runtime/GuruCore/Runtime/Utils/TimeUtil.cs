using System;
using System.Globalization;

namespace Guru
{
    public static class TimeUtil
    {
        public static readonly int MIN_TO_SECOND = 60;
        public static readonly int HOUR_TO_MIN = 60;
        public static readonly int HOUR_TO_SECOND = 3600; //60 * 60;
        public static readonly int DAY_TO_SECOND = 86400; //24 * 60 * 60;

        public static readonly DateTime START_TIME = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        public static readonly DateTime UTC_START_TIME = new DateTime(1970, 1, 1);

        //UnbiasedTime.tick unit : 100 nanosecond
        private static int TICK_TO_MILLISECOND = 10000;

        //from AD1_1_1 to 1970_1_1 timeStamp 
        private static long STAMP_1970_1_1 = 62135596800000;

        /// <summary>
        /// 返回时间戳
        /// </summary>
        public static long GetCurrentTimeStamp() => GetTimeStamp(DateTime.Now.ToUniversalTime());

        public static int GetCurrentTimeStampSecond() => (int)(GetCurrentTimeStamp() / 1000 );

        /// <summary>
        /// 获取固定日期的时间戳
        /// </summary>
        public static long GetTimeStamp(DateTime dateTime)
        {
            return dateTime.Ticks / TICK_TO_MILLISECOND - STAMP_1970_1_1;
        }
        
        public static string GetTimeStampString(DateTime dateTime)
        {
            return GetTimeStamp(dateTime).ToString();
        }
        

        /// <summary>
        /// 时间戳转日期
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime ConvertTimeSpanToDateTime(long unixTimeStamp)
        {
            return UTC_START_TIME.AddMilliseconds(unixTimeStamp);
        }

        /// <summary>
        /// 获取标准日期格式字符串202210826T170026
        /// </summary>
        public static string GetFormatCurrentTime()
        {
            return DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
        }
    }
}
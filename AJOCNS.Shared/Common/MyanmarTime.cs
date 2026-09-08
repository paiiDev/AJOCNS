namespace AJOCNS.Shared.Common
{
    public static class MyanmarTime
    {
        private static readonly TimeZoneInfo Zone = GetZone();

        public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

        public static DateTime ToMyanmar(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc), Zone);
        }

        public static DateTime ToUtc(DateTime myanmarDateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(myanmarDateTime, DateTimeKind.Unspecified), Zone);
        }

        private static TimeZoneInfo GetZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Yangon");
            }
        }
    }
}

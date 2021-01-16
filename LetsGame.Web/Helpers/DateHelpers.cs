using System;

namespace LetsGame.Web.Helpers
{
    public static class DateHelpers
    {
        public static DateTime ConvertFromUserTimezoneToUtc(DateTime dt)
        {
            
            return dt - GetUserTimeZone().GetUtcOffset(dt);
        }

        public static DateTime ConvertFromUtcToUserTimezone(DateTime dt)
        {
            return dt + GetUserTimeZone().GetUtcOffset(dt);
        }

        public static DateTime RemoveSeconds(DateTime dt)
        {
            return dt.AddSeconds(-dt.Second);
        }

        private static TimeZoneInfo GetUserTimeZone()
        {
            // TODO: Implement user timezone preference
            return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        public static string FormatUtcToUserFriendlyDate(DateTime dt)
        {
            var localDt = ConvertFromUtcToUserTimezone(dt);
            var localNow = ConvertFromUtcToUserTimezone(DateTime.UtcNow);

            if (localDt.Date == localNow.Date) return $"Today at {GetFriendlyTime(localDt)}";
            if (localDt.Date == localNow.AddDays(1)) return $"Tomorrow at {GetFriendlyTime(localDt)}";
            if ((localDt.Date - localNow).Days < 7) return $"{localDt.DayOfWeek} at {GetFriendlyTime(localDt)}";
            return $"{localDt.DayOfWeek}, {localDt:MMMM dd} at {GetFriendlyTime(localDt)}";
        }

        private static string GetFriendlyTime(DateTime dt)
        {
            var fmt = dt.Minute == 0
                ? "h tt"
                : "h:mm tt";
            
            return dt.ToString(fmt);
        }
    }
}
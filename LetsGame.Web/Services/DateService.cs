using System;
using LetsGame.Web.Data;
using Microsoft.Extensions.Configuration;

namespace LetsGame.Web.Services
{
    public class DateService
    {
        private readonly string _localTimezone;
        
        public DateService(IConfiguration config)
        {
            _localTimezone = config["LocalTimezone"];
        }

        public DateTime ConvertFromUserTimezoneToUtc(DateTime dt, AppUser userOverride = null)
        {
            return dt - GetUserTimeZone(userOverride).GetUtcOffset(dt);
        }

        public DateTime ConvertFromUtcToUserTimezone(DateTime dt, AppUser userOverride = null)
        {
            return dt + GetUserTimeZone(userOverride).GetUtcOffset(dt);
        }

        public DateTime RemoveSeconds(DateTime dt)
        {
            return dt.AddSeconds(-dt.Second);
        }

        private TimeZoneInfo GetUserTimeZone(AppUser userOverride = null)
        {
            // TODO: Implement user timezone preference
            return TimeZoneInfo.FindSystemTimeZoneById(_localTimezone);
        }

        public string FormatUtcToUserFriendlyDate(DateTime dt, AppUser userOverride = null)
        {
            var localDt = ConvertFromUtcToUserTimezone(dt, userOverride);
            var localNow = ConvertFromUtcToUserTimezone(DateTime.UtcNow, userOverride);

            if (localDt.Date == localNow.Date) return $"Today at {GetFriendlyTime(localDt)}";
            if (localDt.Date == localNow.AddDays(1).Date) return $"Tomorrow at {GetFriendlyTime(localDt)}";
            if (localDt.Date == localNow.AddDays(-1).Date) return $"Yesterday at {GetFriendlyTime(localDt)}";
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
using System.Globalization;
using LetsGame.Web.Data;
using Microsoft.Extensions.Configuration;
using NodaTime;
using NodaTime.TimeZones;

namespace LetsGame.Web.Services
{
    public class DateService
    {
        private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
        
        private readonly string? _localTimezone;

        public DateService(IConfiguration config, IDateTimeZoneProvider dateTimeZoneProvider)
        {
            _dateTimeZoneProvider = dateTimeZoneProvider;
            
            _localTimezone = config["LocalTimezone"];
        }

        public LocalDateTime ConvertToUserLocalTime(Instant instant, AppUser? userOverride = null)
        {
            return instant
                .InZone(GetUserDateTimeZone(userOverride))
                .LocalDateTime;
        }

        public Instant ConvertFromUserLocalTime(LocalDateTime localTime, AppUser? userOverride = null)
        {
            return localTime
                .InZone(GetUserDateTimeZone(userOverride), Resolvers.LenientResolver)
                .ToInstant();
        }

        private DateTimeZone GetUserDateTimeZone(AppUser? userOverride = null)
        {
            // TODO: Implement user timezone preference
            if (_localTimezone == null) return DateTimeZone.Utc;
            return _dateTimeZoneProvider.GetZoneOrNull(_localTimezone) ?? DateTimeZone.Utc;
        }

        public string FormatToUserFriendlyDate(Instant instant, AppUser? userOverride = null)
        {
            var localValue = ConvertToUserLocalTime(instant, userOverride);
            var localNow = ConvertToUserLocalTime(SystemClock.Instance.GetCurrentInstant(), userOverride);

            if (localValue.Date == localNow.Date) return $"Today at {GetFriendlyTime(localValue)}";
            if (localValue.Date == (localNow + Period.FromDays(1)).Date) return $"Tomorrow at {GetFriendlyTime(localValue)}";
            if (localValue.Date == (localNow - Period.FromDays(1)).Date) return $"Yesterday at {GetFriendlyTime(localValue)}";
            return $"{localValue.DayOfWeek}, {localValue:MMMM dd} at {GetFriendlyTime(localValue)}";
        }

        private static string GetFriendlyTime(LocalDateTime dt)
        {
            var fmt = dt.Minute == 0
                ? "h tt"
                : "h:mm tt";
            
            return dt.ToString(fmt, CultureInfo.InvariantCulture);
        }
    }
}
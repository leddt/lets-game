using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LetsGame.Web.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveAccents(this string value)
        {
            var bytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(value);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        
        public static string Sluggify(this string value)
        {
            return Regex.Replace(value.RemoveAccents().ToLower(), @"[^a-z0-9]+", "-");
        }

        public static string ToInitials(this string value, int maxLength)
        {
            var initials = value
                .Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToUpper(x[0]))
                .Take(maxLength)
                .ToArray();

            return new string(initials);
        }
    }
}
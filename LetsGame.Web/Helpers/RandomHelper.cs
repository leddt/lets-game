using System;
using System.Text;

namespace LetsGame.Web.Helpers
{
    public static class RandomHelper
    {
        public static string GetRandomIdentifier(int length)
        {
            var random = new Random();
            
            const string suffixChars = "abcdefghijklmnopqrstuvwxyz1234567890";

            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(suffixChars[random.Next(suffixChars.Length)]);
            }

            return sb.ToString();
        }
    }
}
using System;
using System.Text;

namespace LetsGame.Web.GraphQL
{
    public static class ID
    {
        public static string Typed<T>(object id) => Encode($"{typeof(T).Name}:{id}");

        public static string ToString<T>(string id)
        {
            var parts = Decode(id).Split(':');
            if (parts.Length != 2) throw new InvalidIdFormatException();

            var expectedType = typeof(T).Name;
            if (expectedType != parts[0]) throw new UnexpectedIdTypeException(expectedType, parts[0]);

            return parts[1];
        }
        
        public static long ToLong<T>(string id)
        {
            var rawId = ToString<T>(id);

            if (!long.TryParse(rawId, out var longId))
                throw new InvalidIdFormatException();

            return longId;
        }
        
        public static long? ToLongOptional<T>(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            
            var rawId = ToString<T>(id);

            if (!long.TryParse(rawId, out var longId))
                throw new InvalidIdFormatException();

            return longId;
        }
        
        private static string Encode(string input) => Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        private static string Decode(string input) => Encoding.UTF8.GetString(Convert.FromBase64String(input));
        
        public class InvalidIdFormatException : Exception {}
        public class UnexpectedIdTypeException : Exception
        {
            public UnexpectedIdTypeException(string expected, string actual) : base($"Unexpected ID Type. Expected: {expected} Actual: {actual}")
            {
            }
        }
    }
}
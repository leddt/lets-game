#nullable enable

using System;
using HotChocolate;

namespace LetsGame.Web.Extensions
{
    public static class OptionalExtensions
    {
        public static Optional<TTo> Map<TFrom, TTo>(this Optional<TFrom> from, Func<TFrom?, TTo?> mapper)
        {
            if (from.IsEmpty) return Optional<TTo>.Empty();
            return new Optional<TTo>(mapper(from.Value));
        }
    }
}
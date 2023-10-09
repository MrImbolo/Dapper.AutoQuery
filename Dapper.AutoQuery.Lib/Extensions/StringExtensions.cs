using System.Linq.Expressions;
using System.Security;

namespace Dapper.AutoQuery.Lib.Extensions
{
    public static class StringExtensionsHelper<T>
    {
        internal static Dictionary<string, Func<T, string>> ToStringExpressionCache { get; } = new();
        internal static Dictionary<string, Func<string, T>> FromStringExpressionCache { get; } = new();
    }

    public static class StringExtensions
    {
        public static string Stringify<T>(this IEnumerable<T> items, string separator, Expression<Func<T, string>>? toString = null)
        {
            ArgumentNullException.ThrowIfNull(items, nameof(items));
            ArgumentException.ThrowIfNullOrEmpty(separator, nameof(separator));

            if (toString is null)
            {
                return string.Join(separator, items.Select(StaticToString));
            }

            string expKey = toString.ToString();

            if (!StringExtensionsHelper<T>.ToStringExpressionCache.TryGetValue(expKey, out var exp))
            {
                exp = toString.Compile();
                StringExtensionsHelper<T>.ToStringExpressionCache.Add(expKey, exp);
            }

            return string.Join(separator, items.Select(exp));
        }

        public static IEnumerable<T> Destringify<T>(this string str, string separator, Expression<Func<string, T>> fromString)
        {
            ArgumentException.ThrowIfNullOrEmpty(separator, nameof(separator));
            ArgumentException.ThrowIfNullOrEmpty(str, nameof(str));
            ArgumentNullException.ThrowIfNull(fromString, nameof(fromString));

            string expKey = fromString.ToString();
            if (!StringExtensionsHelper<T>.FromStringExpressionCache.TryGetValue(expKey, out var exp))
            {
                exp = fromString.Compile();
                StringExtensionsHelper<T>.FromStringExpressionCache.Add(expKey, exp);
            }

            return str.Split(separator).Select(exp);
        }

        static string StaticToString<T>(T obj) => obj?.ToString() ?? string.Empty;
    }
}

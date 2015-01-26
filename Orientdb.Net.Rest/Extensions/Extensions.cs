using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    internal static class Extensions
    {
        internal static string Utf8String(this byte[] bytes)
        {
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        internal static byte[] Utf8Bytes(this string s)
        {
            return s.IsNullOrEmpty() ? null : Encoding.UTF8.GetBytes(s);
        }

        internal static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        internal static void ThrowIfNull<T>(this T value, string name)
        {
            if (value == null)
                throw new ArgumentNullException(name);
        }

        internal static string F(this string format, params object[] args)
        {
            format.ThrowIfNull("format");
            return string.Format(format, args);
        }

        internal static void ThrowIfEmpty<T>(this IEnumerable<T> @object, string parameterName)
        {
            @object.ThrowIfNull(parameterName);
            if (!@object.Any())
                throw new ArgumentException("Argument can not be an empty collection", parameterName);
        }

        internal static bool HasAny<T>(this IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list != null && list.Any(predicate);
        }

        internal static bool HasAny<T>(this IEnumerable<T> list)
        {
            return list != null && list.Any();
        }

        internal static void ThrowIfNullOrEmpty(this string @object, string parameterName)
        {
            @object.ThrowIfNull(parameterName);
            if (string.IsNullOrWhiteSpace(@object))
                throw new ArgumentException("Argument can't be null or empty", parameterName);
        }
    }
}
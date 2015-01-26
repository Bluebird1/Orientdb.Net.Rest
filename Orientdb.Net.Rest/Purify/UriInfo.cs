using System;
using System.Globalization;

namespace Orientdb.Net.Purify
{
    internal class UriInfo
    {
        public UriInfo(Uri uri, string source)
        {
            int fragPos = source.IndexOf("#", StringComparison.Ordinal);
            int queryPos = source.IndexOf("?", StringComparison.Ordinal);
            int start = source.IndexOf(uri.Host, StringComparison.Ordinal) + uri.Host.Length;
            int pathEnd = queryPos == -1 ? fragPos : queryPos;

            if (pathEnd == -1)
                pathEnd = source.Length + 1;

            if (start < pathEnd - 1 && source[start] == ':')
            {
                int portLength = uri.Port.ToString(CultureInfo.InvariantCulture).Length;
                start += portLength + 1;
            }

            Path = queryPos > -1 ? source.Substring(start, pathEnd - start) : source.Substring(start);

            Query = fragPos > -1
                ? source.Substring(queryPos, fragPos - queryPos)
                : queryPos > -1
                    ? source.Substring(queryPos, (source.Length - queryPos))
                    : null;

            Source = source;
            if (start < source.Length - 1 && source[start] != ':' && source[start] != '/')
            {
                Source = source.Insert(start, "/");
            }
        }

        public string Path { get; private set; }
        public string Query { get; private set; }

        public string Source { get; private set; }
    }
}
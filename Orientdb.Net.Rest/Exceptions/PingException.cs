using System;

namespace Orientdb.Net.Exceptions
{
    public class PingException : Exception
    {
        public PingException(Uri baseURi, Exception innerException)
            : base("Pinging {0} caused an exception".F(baseURi.ToString()), innerException)
        {
        }
    }
}
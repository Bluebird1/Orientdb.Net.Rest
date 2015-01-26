using System;

namespace Orientdb.Net.Exceptions
{
    public class SniffException : Exception
    {
        public SniffException(MaxRetryException innerException)
            : base("Sniffing known nodes in the cluster caused a maxretry exception of its own", innerException)
        {
        }
    }
}
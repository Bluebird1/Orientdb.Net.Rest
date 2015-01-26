using System;

namespace Orientdb.Net.Exceptions
{
    public class MaxRetryException : Exception
    {
        public MaxRetryException(string message) : base(message)
        {
        }

        public MaxRetryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MaxRetryException(Exception innerException) : base(innerException.Message, innerException)
        {
        }
    }
}
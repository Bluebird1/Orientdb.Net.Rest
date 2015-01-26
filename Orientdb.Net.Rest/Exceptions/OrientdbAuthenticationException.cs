using System;
using System.IO;

namespace Orientdb.Net.Exceptions
{
    public class OrientdbAuthenticationException : Exception
    {
        public OrientdbAuthenticationException(OrientdbResponse<Stream> response)
        {
            Response = response;
        }

        public OrientdbResponse<Stream> Response { get; private set; }

        internal OrientdbServerException ToElasticsearchServerException()
        {
            if (Response == null)
                return null;
            if (Response.HttpStatusCode != null)
                return new OrientdbServerException(Response.HttpStatusCode.Value, "AuthenticationException");

            return null;
        }
    }
}
using Orientdb.Net.Exceptions;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class OrientdbServerError
    {
        public int Status { get; set; }
        public string Error { get; set; }
        public string ExceptionType { get; set; }

        internal static OrientdbServerError Create(OneToOneServerException e)
        {
            if (e == null) return null;
            return new OrientdbServerError
            {
                Status = e.status,
                Error = e.error
            };
        }
    }
}
// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public class BaseResponse : IResponse
    {
        public BaseResponse()
        {
            IsValid = true;
        }

        public virtual bool IsValid { get; internal set; }

        public IOrientdbResponse ConnectionStatus
        {
            get { return ((IResponseWithRequestInformation) this).RequestInformation; }
        }

        public virtual OrientdbServerError ServerError
        {
            get
            {
                if (IsValid || ConnectionStatus == null || ConnectionStatus.OriginalException == null)
                    return null;
                var e = ConnectionStatus.OriginalException as OrientdbServerException;
                if (e == null)
                    return null;
                return new OrientdbServerError
                {
                    Status = e.Status,
                    Error = e.Message,
                    ExceptionType = e.ExceptionType
                };
            }
        }

        IOrientdbResponse IResponseWithRequestInformation.RequestInformation { get; set; }
    }
}
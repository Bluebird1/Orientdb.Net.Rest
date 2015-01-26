// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public interface IResponse : IResponseWithRequestInformation
    {
        bool IsValid { get; }

        IOrientdbResponse ConnectionStatus { get; }

        OrientdbServerError ServerError { get; }
    }
}
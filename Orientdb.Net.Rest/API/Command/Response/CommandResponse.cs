// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{

    public interface ICommandResponse : IResponse
    {

    }

    public class CommandResponse : BaseResponse, ICommandResponse
    {
        public CommandResponse()
        {
            IsValid = true;
        }
    }
}
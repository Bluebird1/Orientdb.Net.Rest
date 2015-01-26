using System;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.Connection.RequestState
// ReSharper restore CheckNamespace
{
    public interface IRequestTimings : IDisposable
    {
        void Finish(bool success, int? httpStatusCode);
    }
}
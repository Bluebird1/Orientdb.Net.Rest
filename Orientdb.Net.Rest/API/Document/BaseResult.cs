using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace

{
    public class BaseResult<T> : IBaseResult<T>
    {
        public List<T> Result { get; set; }
    }

    public interface IBaseResult<T>
    {
        List<T> Result { get; set; }
    }
}
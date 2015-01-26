
// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace

{
    /// <summary>
    /// the name of the language between those supported. OrientDB distribution comes with "sql" only. 
    /// Gremlin language cannot be executed with query because it cannot guarantee to be idempotent. 
    /// To execute Gremlin use command instead.
    /// </summary>
    public enum CommandLanguage
    {
        Sql = 0,

        Gremlin = 1
    }
}
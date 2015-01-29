// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class CreateEdgeQueryParameters : FluentRequestParameters<CreateEdgeQueryParameters>
    {
        internal string _fromQuery { get; set; }

        internal string _toQuery { get; set; }


        public CreateEdgeQueryParameters FromQuery(string query)
        {
            _fromQuery = query;
            return this;
        }

        public CreateEdgeQueryParameters ToQuery(string query)
        {
            _toQuery = query;
            return this;
        }
         
    }
}
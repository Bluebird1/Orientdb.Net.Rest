using System.Collections.Generic;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        public OrientdbResponse<DynamicDictionary> Command(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "command/{0}/{1}".F(Encoded(database), Encoded(language.ToString().ToLower()));

            return OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("POST", url, query));
        }

        public OrientdbResponse<T> Command<T>(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "command/{0}/{1}".F(Encoded(database), Encoded(language.ToString().ToLower()));

            return DoRequest<T>("POST", url, query);
        }

        public BaseResult<T> BaseResultCommand<T>(string query, string database, CommandLanguage language)
        {
            OrientdbResponse<BaseResult<T>> request = Command<BaseResult<T>>(query, database, language);
            return request.Response;
        }
    }
}
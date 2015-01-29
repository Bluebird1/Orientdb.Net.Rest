using System.Collections.Generic;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        /// <summary>
        /// Execute a command against the database. Returns the records affected or the list of records for queries. Command executed via POST.
        /// </summary>
        /// <param name="query">The query containing the command to execute</param>
        /// <param name="database">database name</param>
        /// <param name="language">The name of the language between those supported. OrientDB distribution comes with "sql" and GraphDB distribution has both "sql" and "gremlin"</param>
        /// <returns><see cref="OrientdbResponse{DynamicDictionary}"/></returns>
        /// <see cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/OrientDB-REST.html#command"/>
        public OrientdbResponse<DynamicDictionary> Command(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "command/{0}/{1}".F(Encoded(database), Encoded(language.ToString().ToLower()));

            return OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("POST", url, query));
        }

        /// <summary>
        /// Execute a command against the database. Returns the records affected or the list of records for queries. Command executed via POST.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query containing the command to execute</param>
        /// <param name="database">database name</param>
        /// <param name="language">The name of the language between those supported. OrientDB distribution comes with "sql" and GraphDB distribution has both "sql" and "gremlin"</param>
        /// <returns><see cref="OrientdbResponse{T}"/></returns>
        public OrientdbResponse<T> Command<T>(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "command/{0}/{1}".F(Encoded(database), Encoded(language.ToString().ToLower()));

            return DoRequest<T>("POST", url, query);
        }

        /// <summary>
        /// Execute a command against the database. Returns the records affected or the list of records for queries. Command executed via POST.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query containing the command to execute</param>
        /// <param name="database">database name</param>
        /// <param name="language">The name of the language between those supported. OrientDB distribution comes with "sql" and GraphDB distribution has both "sql" and "gremlin"</param>
        /// <returns><see cref="BaseResult{T}"/></returns>
        public BaseResult<T> BaseResultCommand<T>(string query, string database, CommandLanguage language)
        {
            OrientdbResponse<BaseResult<T>> request = Command<BaseResult<T>>(query, database, language);
            return request.Response;
        }
    }
}
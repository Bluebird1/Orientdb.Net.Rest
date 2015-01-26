using System.Collections.Generic;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        /// <summary>
        ///     Execute a query against the database. Query means only idempotent commands like SQL SELECT and TRAVERSE. Idempotent
        ///     means the command is read-only and can't change the database. Remember that in IE6 the URL can be maximum of 2,083
        ///     characters. Other browsers supports major length, but if you want to stay compatible with all limit to 2,083
        ///     characters.
        /// </summary>
        /// <param name="query">The text containing the query to execute</param>
        /// <param name="database">The database</param>
        /// <param name="language">
        ///     the name of the language between those supported. OrientDB distribution comes with "sql" only.
        ///     Gremlin language cannot be executed with query because it cannot guarantee to be idempotent. To execute Gremlin use
        ///     command instead.
        /// </param>
        /// <remarks>
        ///     Syntax: http://&lt;server&gt;:[&lt;port&gt;]/query/&lt;database&gt;/&lt;language&gt;/&lt;query-text&gt;[/&lt;limit
        ///     &gt;][/&lt;fetchPlan&gt;]
        /// </remarks>
        public OrientdbResponse<DynamicDictionary> GetQuery(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "query/{0}/{1}/{2}".F(Encoded(database), Encoded(language.ToString().ToLower()), query);

            return OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("GET", url));
        }

        /// <summary>
        ///     Execute a query against the database. Query means only idempotent commands like SQL SELECT and TRAVERSE. Idempotent
        ///     means the command is read-only and can't change the database. Remember that in IE6 the URL can be maximum of 2,083
        ///     characters. Other browsers supports major length, but if you want to stay compatible with all limit to 2,083
        ///     characters.
        /// </summary>
        /// <param name="query">The text containing the query to execute</param>
        /// <param name="database">The database</param>
        /// <param name="language">
        ///     the name of the language between those supported. OrientDB distribution comes with "sql" only.
        ///     Gremlin language cannot be executed with query because it cannot guarantee to be idempotent. To execute Gremlin use
        ///     command instead.
        /// </param>
        /// <remarks>
        ///     Syntax: http://&lt;server&gt;:[&lt;port&gt;]/query/&lt;database&gt;/&lt;language&gt;/&lt;query-text&gt;[/&lt;limit
        ///     &gt;][/&lt;fetchPlan&gt;]
        /// </remarks>
        /// <see cref="GetQuery"/>
        public OrientdbResponse<T> GetQuery<T>(string query, string database, CommandLanguage language)
        {
            query.ThrowIfNullOrEmpty("query");
            database.ThrowIfNullOrEmpty("database");
            string url = "query/{0}/{1}/{2}".F(Encoded(database), Encoded(language.ToString().ToLower()), query);

            return DoRequest<T>("GET", url, query);
        }

        /// <summary>
        ///     Execute a query against the database. Query means only idempotent commands like SQL SELECT and TRAVERSE. Idempotent
        ///     means the command is read-only and can't change the database. Remember that in IE6 the URL can be maximum of 2,083
        ///     characters. Other browsers supports major length, but if you want to stay compatible with all limit to 2,083
        ///     characters.
        /// </summary>
        /// <param name="query">The text containing the query to execute</param>
        /// <param name="database">The database</param>
        /// <param name="language">
        ///     the name of the language between those supported. OrientDB distribution comes with "sql" only.
        ///     Gremlin language cannot be executed with query because it cannot guarantee to be idempotent. To execute Gremlin use
        ///     command instead.
        /// </param>
        /// <remarks>
        ///     Syntax: http://&lt;server&gt;:[&lt;port&gt;]/query/&lt;database&gt;/&lt;language&gt;/&lt;query-text&gt;[/&lt;limit
        ///     &gt;][/&lt;fetchPlan&gt;]
        /// </remarks>
        /// <see cref="GetQuery&lt;T&gt;"/>
        public BaseResult<T> BaseResultGetQuery<T>(string query, string database, CommandLanguage language)
        {
            OrientdbResponse<BaseResult<T>> request = Command<BaseResult<T>>(query, database, language);
            return request.Response;
        }
    }
}
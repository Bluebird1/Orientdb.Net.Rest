using System;
using System.Collections.Generic;
using System.Linq;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        /// <summary>
        /// Connect to a remote server using basic authentication.
        /// </summary>
        /// <param name="name">database name</param>
        /// <see cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/OrientDB-REST.html#connect"/>
        public bool Connect(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            string url = "connect/{0}".F(Encoded(name));

            OrientdbResponse<DynamicDictionary> request =
                OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("GET", url));
            return request.HttpStatusCode == 204;
        }

        /// <summary>
        /// Disconnect from remote server
        /// </summary>
        /// <see cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/OrientDB-REST.html#disconnect"/>
        public bool Disconnect()
        {
            const string url = "disconnect";
            OrientdbResponse<DynamicDictionary> request =
                OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("GET", url));
            return request.HttpStatusCode == 401;
        }

        /// <summary>
        /// Retrieve all the information about a database.
        /// </summary>
        /// <param name="name">database name</param>
        /// <remarks>
        /// Syntax: http://&lt;server&gt;:[&lt;port&gt;]/database/&lt;database&gt;
        /// </remarks>
        public IGetDatabasePropertiesResponse GetDatabaseProperties(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            string url = "database/{0}".F(Encoded(name));
            return DoRequest<GetDatabasePropertiesResponse>("GET", url).Response;
        }

        /// <summary>
        /// Create a new database. Requires additional authentication to the server.
        /// </summary>
        /// <param name="name">database name</param>
        /// <param name="storageType">storage type</param>
        public ICreateDatabaseResponse CreateDatabase(string name, StorageType storageType)
        {
            name.ThrowIfNullOrEmpty("name");

            if (DatabaseExist(name))
                throw new Exception("The database already exists.");

            string url = "database/{0}/{1}".F(Encoded(name), Encoded(storageType.ToString().ToLower()));
            return DoRequest<CreateDatabaseResponse>("POST", url).Response;
        }

        /// <summary>
        /// Retrieves the available databases.
        /// </summary>
        /// <remarks>
        /// Syntax: http://&lt;server&gt;:&lt;port&gt;/listDatabases
        /// </remarks>
        public IListDatabaseResponse ListDatabase()
        {
            const string url = "listDatabases";
            return DoRequest<ListDatabaseResponse>("GET", url).Response;
        }

        /// <summary>
        /// Check if database exist
        /// </summary>
        /// <param name="name">database name</param>
        public bool DatabaseExist(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            IListDatabaseResponse response = ListDatabase();

            return
                response.Databases.Any(
                    database => String.Equals(database, name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Drop a database. Requires additional authentication to the server.
        /// </summary>
        /// <param name="name">database name</param>
        /// <remarks>
        /// Syntax: http://&lt;server&gt;:[&lt;port&gt;]/database/&lt;databaseName&gt;
        /// </remarks>
        public bool DeteteDatabase(string name)
        {
            name.ThrowIfNullOrEmpty("name");

            if (!DatabaseExist(name))
                return true;

            string url = "database/{0}".F(Encoded(name));

            OrientdbResponse<DynamicDictionary> request =
                OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("DELETE", url));
            return request.HttpStatusCode == 204;
        }
    }
}
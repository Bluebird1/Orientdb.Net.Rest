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
        public bool Connect(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            string url = "connect/{0}".F(Encoded(name));

            OrientdbResponse<DynamicDictionary> request =
                OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("GET", url));
            return request.HttpStatusCode == 204;
        }

        public bool Disconnect()
        {
            const string url = "disconnect";
            OrientdbResponse<DynamicDictionary> request =
                OrientdbResponse.Wrap(DoRequest<Dictionary<string, object>>("GET", url));
            return request.HttpStatusCode == 401;
        }

        public IGetDatabasePropertiesResponse GetDatabaseProperties(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            string url = "database/{0}".F(Encoded(name));
            return DoRequest<GetDatabasePropertiesResponse>("GET", url).Response;
        }

        public ICreateDatabaseResponse CreateDatabase(string name, StorageType storageType)
        {
            name.ThrowIfNullOrEmpty("name");

            if (DatabaseExist(name))
                throw new Exception("The database already exists.");

            string url = "database/{0}/{1}".F(Encoded(name), Encoded(storageType.ToString().ToLower()));
            return DoRequest<CreateDatabaseResponse>("POST", url).Response;
        }

        public IListDatabaseResponse ListDatabase()
        {
            const string url = "listDatabases";
            return DoRequest<ListDatabaseResponse>("GET", url).Response;
        }

        public bool DatabaseExist(string name)
        {
            name.ThrowIfNullOrEmpty("name");
            IListDatabaseResponse response = ListDatabase();

            return
                response.Databases.Any(
                    database => String.Equals(database, name, StringComparison.CurrentCultureIgnoreCase));
        }

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
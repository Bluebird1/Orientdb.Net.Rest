using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        public bool InsertEdge<T>(string database, ref T document, Func<CreateEdgeQueryParameters, CreateEdgeQueryParameters> requestParameters = null)
        {
            database.ThrowIfNullOrEmpty("database");
            document.ThrowIfNull("document");
            requestParameters.ThrowIfNull("CreateEdgeQueryParameters");

            CreateEdgeQueryParameters requestParams = requestParameters(new CreateEdgeQueryParameters());
            requestParams._fromQuery.ThrowIfNullOrEmpty("From Query");
            requestParams._toQuery.ThrowIfNullOrEmpty("To Query");
            
            Type classType = typeof(T);
            string command = "create edge {0} from ({1}) to ({2}) set ".F(typeof(T).Name, requestParams._fromQuery, requestParams._toQuery);
            
            foreach (
                PropertyInfo propertyInfo in
                    classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    OrientdbProperty oprop = propertyInfo.GetOrientdbPropertyAttribute();
                    if (oprop == null)
                    {
                        string propertyName = propertyInfo.Name;
                        object propertyValue = propertyInfo.GetValue(document, null);

                        if (propertyInfo.PropertyType == typeof (DateTime))
                            propertyValue = ((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss");

                        command = "{0} {1} = '{2}',".F(command, propertyName, propertyValue);
                    }
                }
            }
            if (command.EndsWith(","))
                command = command.Remove(command.Length - 1);

            OrientdbResponse<BaseResult<T>> request = Command<BaseResult<T>>(command, database, CommandLanguage.Sql);
            if (request.Success)
                document = request.Response.Result.First();

            return request.Success;
        }

        public bool InsertEdge<TT, TTT, T>(string database, ref T document, ref TT from, ref TTT to)
        {
            database.ThrowIfNullOrEmpty("database");
            from.ThrowIfNull("from Vertex");
            to.ThrowIfNull("to Vertex");

            if(from.GetType().BaseType != typeof (ODocument))
                throw new InvalidOperationException(
                    "Inconsistent type specified - type for from<T> must type ODocument");

            if (to.GetType().BaseType != typeof(ODocument))
                throw new InvalidOperationException(
                    "Inconsistent type specified - type for to<T> must type ODocument");


            string command = "create edge {0} from {1} to {2} set ".F(typeof(T).Name, (from as ODocument).ORID, (to as ODocument).ORID);


            Type classType = typeof(T);
            foreach (
                PropertyInfo propertyInfo in
                    classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    OrientdbProperty oprop = propertyInfo.GetOrientdbPropertyAttribute();
                    if (oprop == null)
                    {
                        string propertyName = propertyInfo.Name;
                        object propertyValue = propertyInfo.GetValue(document, null);

                        if (propertyInfo.PropertyType == typeof(DateTime))
                            propertyValue = ((DateTime)propertyValue).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                        command = "{0} {1} = '{2}',".F(command, propertyName, propertyValue);
                    }
                }
            }
            if (command.EndsWith(","))
                command = command.Remove(command.Length - 1);

            OrientdbResponse<BaseResult<T>> request = Command<BaseResult<T>>(command, database, CommandLanguage.Sql);
            if (request.Success)
                document = request.Response.Result.First();

            return request.Success;
        }
    }
}

using System;
using System.Globalization;
using System.Reflection;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        public bool InsertEdge<T>(string database, T document, Func<CreateEdgeQueryParameters, CreateEdgeQueryParameters> requestParameters = null)
        {
            database.ThrowIfNullOrEmpty("database");
            document.ThrowIfNull("document");
            requestParameters.ThrowIfNull("CreateEdgeQueryParameters");

            CreateEdgeQueryParameters requestParams = requestParameters(new CreateEdgeQueryParameters());
            requestParams._fromQuery.ThrowIfNullOrEmpty("From Query");
            requestParams._toQuery.ThrowIfNullOrEmpty("To Query");
            
            Type classType = typeof(T);
            string command = "create edge {0} ".F(typeof(T).Name);

            command = "{0} from ({1})".F(command, requestParams._fromQuery);
            command = "{0} to ({1}) ".F(command, requestParams._toQuery);
            command = "{0} set ".F(command);

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
                            propertyValue = ((DateTime) propertyValue).ToString("yyyy-MM-dd HH:mm:ss");

                        command = "{0} {1} = '{2}',".F(command, propertyName, propertyValue);
                    }
                }
            }
            if (command.EndsWith(","))
                command = command.Remove(command.Length - 1);

            OrientdbResponse<DynamicDictionary> response = Command(command, database, CommandLanguage.Sql);
            return response.Success;
        }
    }
}

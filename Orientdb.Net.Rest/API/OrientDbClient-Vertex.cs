using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Orientdb.Net.API;
using Orientdb.Net.Serialization;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        public bool InsertVertex<T>(string database, List<T> documentList)
        {
            database.ThrowIfNullOrEmpty("database");
            documentList.ThrowIfNull("document");
            
            var resultList = new List<T>();
            foreach (T document in documentList)
            {
                T value = document;
                if (InsertVertex(database, ref value))
                    resultList.Add(value);
            }
            documentList.Clear();
            documentList.AddRange(resultList);

            return true;
        }

        public bool InsertVertex<T>(string database, ref T document)
        {
            database.ThrowIfNullOrEmpty("database");
            document.ThrowIfNull("document");

            Type classType = typeof (T);
            string command = "create vertex {0} set ".F(typeof (T).Name);

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

        public bool DeleteVertex<T>(string database, T document)
        {
            database.ThrowIfNullOrEmpty("database");
            document.ThrowIfNull("document");

            Type classType = typeof (T);
            if (classType.BaseType != typeof (ODocument))
                return false;

            return DeleteVertex(database, (document as ODocument).ORID);
        }

        public bool DeleteVertex(string database, ORID orid)
        {
            database.ThrowIfNullOrEmpty("database");
            orid.ThrowIfNull("orid");

            string command = "DELETE VERTEX {0}".F(orid.RID);

            OrientdbResponse<DynamicDictionary> response = Command(command, database, CommandLanguage.Sql);
            return response.Success;
        }
    }
}
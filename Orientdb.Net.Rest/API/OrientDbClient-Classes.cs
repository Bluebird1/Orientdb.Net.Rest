using System;
using System.Linq;
using System.Reflection;
using Orientdb.Net.API;

// ReSharper disable CheckNamespace

namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public partial class OrientdbClient
    {
        /// <summary>
        ///     Gets informations about requested class.
        /// </summary>
        public IGetClassPropertiesResponse GetClassProperties(string database, string className)
        {
            database.ThrowIfNullOrEmpty("database");
            className.ThrowIfNullOrEmpty("className");
            string url = "class/{0}/{1}".F(Encoded(database), Encoded(className));
            return DoRequest<GetClassPropertiesResponse>("GET", url).Response;
        }

        /// <summary>
        ///     Testing whether the class already exists.
        /// </summary>
        /// <returns>is exists</returns>
        public bool ClassExist(string database, string className)
        {
            database.ThrowIfNullOrEmpty("database");
            className.ThrowIfNullOrEmpty("className");

            IGetDatabasePropertiesResponse databaseProperties = GetDatabaseProperties(database);
            return databaseProperties.Classes.Any(orienClass => orienClass.Name == className);
        }

        /// <summary>
        ///     The Drop Class command removes a class from the schema.
        ///     NOTE: Pay attention to maintain the schema coherent. For example avoid to
        ///     remove classes that are super classes of others. The associated cluster won't be deleted.
        /// </summary>
        /// <seealso cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/SQL-Drop-Class.html" />
        public bool DropClass(string database, string className)
        {
            database.ThrowIfNullOrEmpty("database");
            className.ThrowIfNullOrEmpty("className");

            if (!ClassExist(database, className))
                return true;

            string command = "DROP CLASS {0}".F(className);
            OrientdbResponse<DynamicDictionary> response = Command(command, database, CommandLanguage.Sql);
            return response.Success;
        }

        /// <summary>
        ///     The Drop Class command removes a class from the schema.
        /// </summary>
        /// <see cref="DropClass" />
        public bool DropClass<T>(string database)
        {
            database.ThrowIfNullOrEmpty("database");
            return DropClass(database, typeof (T).Name);
        }

        /// <summary>
        ///     The Create Class command creates a new class in the schema. NOTE: If a cluster
        ///     with the same name exists in the database will be used as default cluster.
        /// </summary>
        /// <remarks>
        ///     Syntax: CREATE CLASS &lt;class&gt; [EXTENDS &lt;super-class&gt;]
        /// </remarks>
        /// <seealso cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/SQL-Create-Class.html" />
        public bool CreateClass<T>(string database,
            Func<CreateClassRequestParameters, CreateClassRequestParameters> requestParameters = null)
        {
            database.ThrowIfNullOrEmpty("database");

            string className = typeof (T).Name;

            CreateClassRequestParameters requestParams = null;

            if (ClassExist(database, className))
                throw new Exception("The class {0} already exists.".F(className));

            string command = "CREATE CLASS {0}".F(className);

            if (requestParameters != null)
                requestParams = requestParameters(new CreateClassRequestParameters());


            if (requestParams != null)
            {
                if (!string.IsNullOrEmpty(requestParams._extends))
                    command = "{0} EXTENDS {1}".F(command, requestParams._extends);

                if (requestParams._abstract)
                    command = "{0} ABSTRACT".F(command);
            }

            OrientdbResponse<DynamicDictionary> response = Command(command, database, CommandLanguage.Sql);
            if (!response.Success)
                return false;


            return CreateProperties<T>(database);
        }

        /// <summary>
        ///     The Create Property command creates a new property in the schema. An existing class
        ///     is required to perform this command.
        /// </summary>
        /// <remarks>
        ///     Syntax: CREATE PROPERTY &lt;class&gt;.&lt;property&gt; &lt;type&gt; [&lt;linked-type&gt;|&lt;linked-class&gt;]
        /// </remarks>
        /// <seealso cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/SQL-Create-Property.html" />
        public bool CreateProperties<T>(string database)
        {
            database.ThrowIfNullOrEmpty("database");
            Type classType = typeof (T);
            string className = typeof (T).Name;

            if (classType != null && classType != typeof (T))
                throw new InvalidOperationException(
                    "Inconsistent type specified - type for CreateProperties<T> must match type for Class<T>");

            foreach (
                PropertyInfo pi in
                    classType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
            {
                if (pi.CanRead && pi.CanWrite)
                {
                    string propertyTypeName;
                    string propertyName = pi.Name;
                    
                    
                    OrientdbProperty oprop = pi.GetOrientdbPropertyAttribute();
                    
                    if (oprop != null)
                    {    
                        if (!oprop.Deserializable && !oprop.Serializable)
                            continue;
                        if (oprop.LinkedType == LinkedType.Link)
                        {
                            if (string.IsNullOrEmpty(oprop.LinkedClass))
                                continue;

                            if (oprop.IsOut)
                                propertyName = "out";
                            if (oprop.IsIn)
                                propertyName = "in";

                            propertyTypeName = " LINK {0}".F(oprop.LinkedClass);
                        }
                        else
                            continue;
                    }
                    else
                    {
                        if (pi.PropertyType == typeof (DateTime))
                            propertyTypeName = "date";
                        else if (pi.PropertyType == typeof (int))
                            propertyTypeName = "integer";
                        else if (pi.PropertyType == typeof (bool))
                            propertyTypeName = "boolean";
                        else if (pi.PropertyType == typeof (short))
                            propertyTypeName = "short";
                        else if (pi.PropertyType == typeof (long))
                            propertyTypeName = "long";
                        else if (pi.PropertyType == typeof (float))
                            propertyTypeName = "float";
                        else if (pi.PropertyType == typeof (double))
                            propertyTypeName = "double";
                        else if (pi.PropertyType == typeof (string))
                            propertyTypeName = "string";
                        else if (pi.PropertyType == typeof (byte[]))
                            propertyTypeName = "binary";
                        else if (pi.PropertyType == typeof (byte))
                            propertyTypeName = "byte";
                        else if (pi.PropertyType.BaseType == typeof (Enum))
                            propertyTypeName = "integer";
                        else
                            continue;
                    }

                    string command = "CREATE PROPERTY {0}.{1} {2}".F(className, propertyName, propertyTypeName);
                    OrientdbResponse<DynamicDictionary> response = Command(command, database, CommandLanguage.Sql);
                    if (!response.Success)
                        return false;
                }
            }
            return true;
        }
    }
}
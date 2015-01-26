using System.Linq;
using System.Reflection;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    public static class PropertyInfoExtension
    {
        public static OrientdbProperty GetOrientdbPropertyAttribute(this PropertyInfo property)
        {
            return property.GetCustomAttributes(typeof(OrientdbProperty), true).OfType<OrientdbProperty>().FirstOrDefault();
        }
    }
}
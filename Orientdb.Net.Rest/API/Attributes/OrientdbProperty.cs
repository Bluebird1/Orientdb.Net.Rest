using System;

// ReSharper disable CheckNamespace
namespace Orientdb.Net
// ReSharper restore CheckNamespace
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OrientdbProperty : Attribute
    {
        public OrientdbProperty()
        {
            Alias = "";
            LinkedClass = "";
            Serializable = true;
            Deserializable = true;
            LinkedType = LinkedType.None;
        }

        public string Alias { get; set; }

        public bool Serializable { get; set; }

        public bool Deserializable { get; set; }

        public bool IsOut { get; set; }

        public bool IsIn { get; set; }

        public string LinkedClass { get; set; }

        public LinkedType LinkedType { get; set; }

        public string PropertyMapping
        {
            get
            {
                if (LinkedType == LinkedType.LinkBag)
                {

                    if (string.IsNullOrEmpty(LinkedClass))
                        return string.Empty;
                    string mapping = string.Empty;

                    if (IsOut)
                        mapping = "out";
                    if (IsIn)
                        mapping = "in";

                    return "{0}_{1}".F(mapping, LinkedClass);
                }
                if (LinkedType == LinkedType.Link)
                {
                    if (IsOut)
                        return "out";

                    if (IsIn)
                        return "in";
                }


                return string.Empty;
            }
        }
    }

    public enum LinkedType
    {
        None,

        Link,

        LinkList,

        LinkSet,

        LinkMap,

        LinkBag
    }
}
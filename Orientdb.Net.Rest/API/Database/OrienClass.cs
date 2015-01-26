using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class OrienClass
    {
        public string Name { get; set; }

        public string SuperClass { get; set; }

        public string Alias { get; set; }

        public bool Abstract { get; set; }

        public bool Strictmode { get; set; }

        public int[] Clusters { get; set; }

        public int DefaultCluster { get; set; }

        public string ClusterSelection { get; set; }
        
        public int Records { get; set; }

        public List<ClassProperty> Properties { get; set; }

        public List<ClassIndex> Indexes { get; set; }

    }
}
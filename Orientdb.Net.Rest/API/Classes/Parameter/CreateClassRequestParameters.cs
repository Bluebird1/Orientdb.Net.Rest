// ReSharper disable CheckNamespace
namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class CreateClassRequestParameters : FluentRequestParameters<CreateClassRequestParameters> 
    {
        internal string _extends { get; set; }
        internal bool _abstract { get; set; }

        /// <summary>
        /// Optional, is the super-class to extend
        /// </summary>
        public CreateClassRequestParameters Extends(string extends)
        {
            _extends = extends;
            return this;
        }

        /// <summary>
        /// Create class as abstract class
        /// </summary>
        /// <see cref="http://www.orientechnologies.com/docs/last/orientdb.wiki/Concepts.html#abstract-class"/>
        public CreateClassRequestParameters Abstract(bool abstracts)
        {
            _abstract = abstracts;
            return this;
        } 
    }
}
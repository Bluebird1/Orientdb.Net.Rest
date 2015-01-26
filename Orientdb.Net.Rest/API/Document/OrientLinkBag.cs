 // ReSharper disable CheckNamespace

namespace Orientdb.Net.API
// ReSharper restore CheckNamespace
{
    public class OrientLinkBag
    {
        public OrientLinkBag(string orid)
        {
            Orid = new ORID(orid);
        }

        public ORID Orid { get; set; }
    }
}
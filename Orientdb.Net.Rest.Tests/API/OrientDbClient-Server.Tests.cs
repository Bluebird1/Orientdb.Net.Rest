using System;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class ServerTests
    {
        [Test]
        public void GetServerPropertiesTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            IGetServerPropertiesResponse response = orientdbClient.GetServerProperties();

            Assert.IsNotNull(response);
        }
    }
}
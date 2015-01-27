using System;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void CreateMemoryDatabaseTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            ICreateDatabaseResponse response = orientdbClient.CreateDatabase(Guid.NewGuid().ToString(), StorageType.Memory);

            Assert.IsNotNull(response);
        }

        [Test]
        public void CreatePlocalDatabaseTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            ICreateDatabaseResponse response = orientdbClient.CreateDatabase(Guid.NewGuid().ToString(), StorageType.Plocal);

            Assert.IsNotNull(response);
        }

        [Test]
        public void DeleteDatabaseTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            bool response = orientdbClient.DeteteDatabase(Guid.NewGuid().ToString());

            Assert.IsTrue(response);
        }

        [Test]
        public void ListDatabasesTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            IListDatabaseResponse response = orientdbClient.ListDatabase();
            Assert.IsNotNull(response);
        }

        [Test]
        [Ignore]
        public void GetDatabasePropertiesTest()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            configuration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");

            var orientdbClient = new OrientdbClient(configuration);
            IGetDatabasePropertiesResponse response = orientdbClient.GetDatabaseProperties("Foo");
            Assert.IsNotNull(response);
            
        }
    }
}
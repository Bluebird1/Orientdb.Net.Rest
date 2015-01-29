using System;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class QueryTests
    {
        private OrientdbClient _orientdbClient;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        private void CreateDatabase()
        {
            var adminConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            adminConfiguration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");
            var adminOrientdbClient = new OrientdbClient(adminConfiguration);
            adminOrientdbClient.CreateDatabase(_databaseName, StorageType.Memory);
            adminOrientdbClient.Disconnect();
        }

        private void DeleteDatabase()
        {
            var adminConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            adminConfiguration.SetBasicAuthentication("root", "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");
            var adminOrientdbClient = new OrientdbClient(adminConfiguration);
            adminOrientdbClient.DeteteDatabase(_databaseName);
            adminOrientdbClient.Disconnect();
        }

        [SetUp]
        public void Init()
        {
            CreateDatabase();

            var connectionConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            connectionConfiguration.SetBasicAuthentication("admin", "admin");
            _orientdbClient = new OrientdbClient(connectionConfiguration);
            _orientdbClient.Connect(_databaseName);

            _orientdbClient.CreateClass<Foo>(_databaseName);
        }

        [TearDown]
        public void TearDown()
        {
            _orientdbClient.Disconnect();
            DeleteDatabase();
        }

        [Test]
        public void GetQueryTest()
        {
            OrientdbResponse<BaseResult<Foo>> response = _orientdbClient.GetQuery<BaseResult<Foo>>("select * from Foo", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.HttpStatusCode);
        }

        [Test]
        public void BaseResultGetQueryTest()
        {
            BaseResult<Foo> response = _orientdbClient.BaseResultGetQuery<Foo>("select * from Foo", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
        }


        internal class Foo : ODocument
        {
            public int FooInt { get; set; }

            public bool FooBoolean { get; set; }

            public short FooShort { get; set; }

            public long FooLong { get; set; }

            public float FooFloat { get; set; }

            public double FooDouble { get; set; }

            public DateTime FooDate { get; set; }

            public string FooString { get; set; }

            public BarEnum FooEnum { get; set; }
        }

        internal enum BarEnum
        {
            BarEnum1,

            BarEnum2
        }
    }
}
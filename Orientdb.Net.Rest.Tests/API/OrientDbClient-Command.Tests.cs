using System;
using System.Net;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class CommandTests
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
        }

        [TearDown]
        public void TearDown()
        {
            _orientdbClient.Disconnect();
            DeleteDatabase();
        }

        [Test]
        public void OrientdbResponseOfDynamicDictionaryTest()
        {
            OrientdbResponse<DynamicDictionary> response = _orientdbClient.Command("select from OUser", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.HttpStatusCode);
        }

        [Test]
        public void OrientdbResponseOfTTest()
        {
            OrientdbResponse<BaseResult<OUser>> response = _orientdbClient.Command<BaseResult<OUser>>("select from OUser", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
            Assert.AreEqual(200, response.HttpStatusCode);
            Assert.AreEqual(3, response.Response.Result.Count);
        }


        [Test]
        public void BaseResultCommandTest()
        {
            BaseResult<OUser> response = _orientdbClient.BaseResultCommand<OUser>("select from OUser", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
            Assert.AreEqual(3, response.Result.Count);
        }

        class OUser
        {
            public string Name { get; set; }

            public string Password { get; set; }

            public string Status { get; set; }
        }
    }
}
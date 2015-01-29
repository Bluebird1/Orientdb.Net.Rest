using System;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class ClassTests
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
        public void GetClassPropertiesTests()
        {
            IGetClassPropertiesResponse classPropertiesResponse = _orientdbClient.GetClassProperties(_databaseName, "OIdentity");
            Assert.IsNotNull(classPropertiesResponse);
            Assert.AreEqual("OIdentity", classPropertiesResponse.Name);
        }

        [Test]
        public void CreateClassDefaultTest()
        {
            bool response = _orientdbClient.CreateClass<Foo>(_databaseName);
            Assert.IsTrue(response);
        }

        [Test]
        public void CreateClassExtendsVertexTest()
        {
            bool response = _orientdbClient.CreateClass<Bar>(_databaseName, parameters => parameters.Extends("V"));
            Assert.IsTrue(response);

            _orientdbClient.DropClass<Bar>(_databaseName);
        }

        [Test]
        public void CreateClassExtendsEdgeTest()
        {
            bool response = _orientdbClient.CreateClass<Bar>(_databaseName, parameters => parameters.Extends("E"));
            Assert.IsTrue(response);

            _orientdbClient.DropClass<Bar>(_databaseName);
        }

        [Test]
        public void CreateClassAbstractTest()
        {
            bool response = _orientdbClient.CreateClass<Bar>(_databaseName, parameters => parameters.Extends("V").Abstract(true));
            Assert.IsTrue(response);
            _orientdbClient.DropClass<Bar>(_databaseName);
        }

        [Test]
        public void DropClassTest()
        {
            _orientdbClient.CreateClass<Foo>(_databaseName);

            bool response = _orientdbClient.DropClass(_databaseName, "Foo");
            Assert.IsTrue(response);
        }

        [Test]
        public void DropClassGenericTest()
        {
            _orientdbClient.CreateClass<Foo>(_databaseName);

            bool response = _orientdbClient.DropClass<Foo>(_databaseName);
            Assert.IsTrue(response);
        }

        internal class Bar : ODocument
        {
            
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
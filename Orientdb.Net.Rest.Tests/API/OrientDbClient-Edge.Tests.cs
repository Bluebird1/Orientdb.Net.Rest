using System;
using System.Collections.Generic;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class EdgeTests
    {
        [SetUp]
        public void Init()
        {
            CreateDatabase();

            var connectionConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            connectionConfiguration.SetBasicAuthentication("admin", "admin");
            _orientdbClient = new OrientdbClient(connectionConfiguration);
            _orientdbClient.Connect(_databaseName);

            CreateClasses();
        }

        [TearDown]
        public void TearDown()
        {
            _orientdbClient.Disconnect();
            DeleteDatabase();
        }

        private OrientdbClient _orientdbClient;
        private readonly string _databaseName = Guid.NewGuid().ToString();

        private void CreateDatabase()
        {
            var adminConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            adminConfiguration.SetBasicAuthentication("root",
                "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");
            var adminOrientdbClient = new OrientdbClient(adminConfiguration);
            adminOrientdbClient.CreateDatabase(_databaseName, StorageType.Memory);
            adminOrientdbClient.Disconnect();
        }

        private void CreateClasses()
        {
            _orientdbClient.CreateClass<Person>(_databaseName, p => p.Extends("V"));
            _orientdbClient.CreateClass<Country>(_databaseName, p => p.Extends("V"));
            _orientdbClient.CreateClass<Livesin>(_databaseName, p => p.Extends("E"));
        }

        private void DeleteDatabase()
        {
            var adminConfiguration = new ConnectionConfiguration(new Uri("http://localhost:2480"));
            adminConfiguration.SetBasicAuthentication("root",
                "F3647CD476BD50D14892033CF160E748038BD62DDB7292690086B5F52151B7C0");
            var adminOrientdbClient = new OrientdbClient(adminConfiguration);
            adminOrientdbClient.DeteteDatabase(_databaseName);
            adminOrientdbClient.Disconnect();
        }

        internal class Person : ODocument
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Gender { get; set; }

            [OrientdbProperty(IsOut = true, LinkedType = LinkedType.LinkBag, LinkedClass = "Livesin")]
            public OrientLinkBag LivesIn { get; set; }
        }

        internal class Country : ODocument
        {
            public string Name { get; set; }

            [OrientdbProperty(IsIn = true, LinkedType = LinkedType.LinkBag, LinkedClass = "Livesin")]
            public OrientLinkBag LivesIn { get; set; }
        }

        internal class Livesin : ODocument
        {
            public DateTime Since { get; set; }

            [OrientdbProperty(IsOut = true, LinkedType = LinkedType.Link, LinkedClass = "Person")]
            public ORID OutPerson { get; set; }

            [OrientdbProperty(IsIn = true, LinkedType = LinkedType.Link, LinkedClass = "Country")]
            public ORID InCountry { get; set; }
        }

        [Test]
        public void InsertEdgeTest()
        {
            var persons = new List<Person>
            {
                new Person {Name = "Carey", Surname = "Mulligan", Gender = "female"},
                new Person {Name = "Keira", Surname = "Knightley", Gender = "female"},
                new Person {Name = "Chris", Surname = "Pine", Gender = "male"}
            };

            var countries = new List<Country>
            {
                new Country {Name = "Westminster"}
            };

            _orientdbClient.InsertVertex(_databaseName, persons);
            _orientdbClient.InsertVertex(_databaseName, countries);

            var livesin = new Livesin {Since = DateTime.Now};

            bool result = _orientdbClient.InsertEdge(_databaseName, ref livesin,
                p =>
                    p.FromQuery("select * from Person where Name = 'Carey'")
                        .ToQuery("select from Country where Name = 'Westminster'"));

            Assert.IsTrue(result);
            Assert.IsNotNull(livesin.ORID);
            Assert.IsNotNull(livesin.InCountry);
            Assert.IsNotNull(livesin.OutPerson);
        }
    }
}
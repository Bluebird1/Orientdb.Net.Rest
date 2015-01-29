using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NUnit.Framework;
using Orientdb.Net;
using Orientdb.Net.API;
using Orientdb.Net.Connection;

namespace Orient.Test.Unit.API
{
    [TestFixture]
    public class VertexTests
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

        private void CreateClasses()
        {
            _orientdbClient.CreateClass<Actor>(_databaseName, p => p.Extends("V"));
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

            CreateClasses();
        }

        [TearDown]
        public void TearDown()
        {
            _orientdbClient.Disconnect();
            DeleteDatabase();
        }

        [Test]
        public void InsertVertexDocumentTest()
        {
            var person = new Actor { Name = "Carey", Surname = "Mulligan", Gender = "female" };
            bool insertVertexDocument = _orientdbClient.InsertVertex<Actor>(_databaseName, ref person);
            Assert.IsTrue(insertVertexDocument);
            Assert.IsNotNull(person.ORID);
        }

        [Test]
        public void InsertVertexListTest()
        {
            var persons = new List<Actor>
            {
                new Actor { Name = "Carey", Surname = "Mulligan", Gender = "female" },
                new Actor { Name = "Keira", Surname = "Knightley", Gender = "female" },
                new Actor { Name = "Chris", Surname = "Pine", Gender = "male" },
                new Actor { Name = "Imogen", Surname = "Poots", Gender = "female" },
                new Actor { Name = "Megan", Surname = "Fox", Gender = "female" },
                new Actor { Name = "Matt", Surname = "Damon", Gender = "male" },
                new Actor { Name = "Kate", Surname = "Winslet", Gender = "female" },
                new Actor { Name = "Charlize", Surname = "Theron", Gender = "female" },
                new Actor { Name = "Ben", Surname = "Whishaw", Gender = "male" },
            };
            bool insertVertexDocument = _orientdbClient.InsertVertex<Actor>(_databaseName, persons);
            Assert.IsTrue(insertVertexDocument);

            foreach (Actor person in persons)
            {
                Assert.IsNotNull(person.ORID);
            }


            BaseResult<Actor> response = _orientdbClient.BaseResultGetQuery<Actor>("select * from Actor", _databaseName, CommandLanguage.Sql);
            Assert.IsNotNull(response);
            Assert.AreEqual(persons.Count, response.Result.Count);
            Assert.IsInstanceOf<List<Actor>>(response.Result);
        }

        internal class Actor : ODocument
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Gender { get; set; }
        }

    }
}
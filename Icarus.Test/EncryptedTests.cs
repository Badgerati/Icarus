/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using NUnit.Framework;
using Icarus.Core;
using System.IO;
using System;
using Haxxor.Framework.Core.Interfaces;
using Haxxor.Framework;

namespace Icarus.Test
{
    [TestFixture]
    public class EncryptedTests
    {

        private const string _dataStore = "Test";
        private const string _collection = "EncStore";

        private IEncryptionModule _encryptionModule = HaxxorFactory.GetByType(EncryptionType.AES256);


        [TestFixtureSetUp]
        public void SetupBase()
        {
            IcarusClient.Instance.Initialise(".");
            IcarusClient.Instance.IsAccessEveryone = false;
            IcarusClient.Instance.IsEncryptionEnabled = true;
            IcarusClient.Instance.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            var path = Path.GetFullPath(".\\Test\\" + _collection + ".json");
            if (File.Exists(path))
            {
                using (var file = File.CreateText(path))
                {
                    file.WriteLine(_encryptionModule.Encrypt("{}", false));
                }

                IcarusClient.Instance.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Refresh(false);
            }

            path = Path.GetFullPath(".\\Test\\InnerTest\\" + _collection + ".json");
            if (IcarusClient.Instance.Locations.ContainsKey("test2") && File.Exists(path))
            {
                using (var file = File.CreateText(path))
                {
                    file.WriteLine(_encryptionModule.Encrypt("{}", false));
                }

                IcarusClient.Instance.GetDataStore("InnerTest", "test2").GetCollection<SomeObject>(_collection).Refresh(false);
            }
        }

        #region Check to see if isNew

        [Test]
        public void CheckIsNew_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello", Temp = "Anything" };

            var expectedDataStore = !Directory.Exists(".\\Test");
            var expectedCollection = !File.Exists(".\\Test\\" + _collection + ".json");

            var isNewDataStore = true;
            var isNewCollection = true;

            var item = icarus.GetDataStore(_dataStore, out isNewDataStore).GetCollection<SomeObject>(_collection, out isNewCollection).Insert(obj);
            Assert.AreEqual(1, item._id);

            Assert.AreEqual(expectedDataStore, isNewDataStore, "Data store check incorrect");
            Assert.AreEqual(expectedCollection, isNewCollection, "Collection check incorrect");
        }

        #endregion

        #region Two Icarus Locations

        [Test]
        public void TwoIcarusLocations_Insert_Success()
        {
            IcarusClient.Instance.Initialise(".\\Test", "test2");

            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello", Temp = "Anything" };

            var item = icarus.GetDataStore("InnerTest", "test2").GetCollection<SomeObject>(_collection).Insert(obj);
            Assert.AreEqual(1, item._id);

            var path = Path.GetFullPath(".\\Test\\InnerTest\\" + _collection + ".json");
            Assert.IsTrue(File.Exists(path));
        }

        #endregion

        #region Toggle Encryption

        [Test]
        public void Toggle_Encryption_Success()
        {
            // insert data encrypted
            var icarus = IcarusClient.Instance;

            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello", Temp = "Anything" };

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(obj);
            Assert.AreEqual(1, item._id);

            // turn off encryption
            icarus.GetDataStore(_dataStore).Clear();
            icarus.Clear();
            icarus.IsEncryptionEnabled = false;

            item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection, false).Find(1);
            Assert.NotNull(item);
            Assert.AreEqual(1, item._id);

            icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Persist();

            // re-enable encryption
            icarus.GetDataStore(_dataStore).Clear();
            icarus.Clear();
            icarus.IsEncryptionEnabled = true;

            item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection, true).Find(1);
            Assert.NotNull(item);
            Assert.AreEqual(1, item._id);

            icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Persist();
        }

        #endregion

        #region Insert Tests

        [Test]
        public void Insert_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello", Temp = "Anything" };

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(obj);
            Assert.AreEqual(1, item._id);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Insert_Fail_NoItem()
        {
            var icarus = IcarusClient.Instance;
            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(null);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Insert_Fail_NullReference()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello" };
            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(obj);
        }

        #endregion

        #region Insert Many Tests

        [Test]
        public void InsertMany_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);
        }

        [Test]
        public void InsertMany_Fail_NoItems()
        {
            var icarus = IcarusClient.Instance;
            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(null);
            Assert.IsNull(items);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void InsertMany_Fail_NullReference()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello" };
            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj });
        }

        #endregion

        #region Find Tests

        [Test]
        public void Find_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find(1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello1", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);
        }

        [Test]
        public void Find_Success_Nothing()
        {
            var icarus = IcarusClient.Instance;
            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find(1);
            Assert.IsNull(item);
        }

        [Test]
        public void Find_Cache_Success()
        {
            var icarus = IcarusClient.Instance;
            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = collection.InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = collection.Find(1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello1", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);
        }

        [Test]
        [TestCase(1, "SomeInt", IcarusEqualityFilter.Equal, false)]
        [TestCase(3, "SomeInt", IcarusEqualityFilter.Equal, true)]
        [TestCase(2, "SomeInt", IcarusEqualityFilter.LessThan, false)]
        [TestCase(1, "SomeInt", IcarusEqualityFilter.LessThan, true)]
        [TestCase(1, "SomeInt", IcarusEqualityFilter.LessThanOrEqual, false)]
        [TestCase(0, "SomeInt", IcarusEqualityFilter.LessThanOrEqual, true)]
        [TestCase(1, "SomeInt", IcarusEqualityFilter.GreaterThan, false)]
        [TestCase(2, "SomeInt", IcarusEqualityFilter.GreaterThan, true)]
        [TestCase(3, "SomeInt", IcarusEqualityFilter.GreaterThanOrEqual, true)]
        [TestCase(2, "SomeInt", IcarusEqualityFilter.GreaterThanOrEqual, false)]
        [TestCase(2, "SomeInt", IcarusEqualityFilter.NotEqual, false)]
        [TestCase("hello1", "SomeString", IcarusEqualityFilter.Equal, true)]
        public void Find_Filter_Success(object value, string field, IcarusEqualityFilter filter, bool isNull)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("SomeInt", value, filter);

            if (isNull)
            {
                Assert.IsNull(item);
            }
            else
            {
                Assert.IsNotNull(item);
            }
        }

        [Test]
        [TestCase(IcarusEqualityFilter.Equal, true)]
        [TestCase(IcarusEqualityFilter.Equal, false)]
        [TestCase(IcarusEqualityFilter.NotEqual, true)]
        [TestCase(IcarusEqualityFilter.NotEqual, false)]
        public void Find_Filter_Boolean_Success(IcarusEqualityFilter filter, bool value)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeBool = true, Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeBool = false, Temp = "Anything2" };

            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var items = collection.InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = collection.Find("SomeBool", value, filter);
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item.SomeInt);

            if (filter == IcarusEqualityFilter.Equal)
            {
                Assert.AreEqual(value, item.SomeBool);
                Assert.AreEqual(value ? 1 : 2, item._id);
            }
            else
            {
                Assert.AreEqual(!value, item.SomeBool);
                Assert.AreEqual(value ? 2 : 1, item._id);
            }
        }

        [Test]
        [TestCase(IcarusEqualityFilter.LessThan, true)]
        [TestCase(IcarusEqualityFilter.GreaterThan, false)]
        public void Find_Filter_SuccessOnDates(IcarusEqualityFilter filter, bool isNull)
        {
            var icarus = IcarusClient.Instance;
            var now = DateTime.Now;

            var obj1 = new SomeObject() { SomeDate = now, Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeDate = now.AddDays(1), Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = default(SomeObject);

            switch (filter)
            {
                case IcarusEqualityFilter.Equal:
                    item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("SomeDate", now, filter);
                    break;

                case IcarusEqualityFilter.NotEqual:
                    item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("SomeDate", now.AddDays(-1), filter);
                    break;

                case IcarusEqualityFilter.GreaterThan:
                    item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("SomeDate", now.AddMinutes(5), filter);
                    break;
            }

            if (isNull)
            {
                Assert.IsNull(item);
            }
            else
            {
                Assert.IsNotNull(item);
            }
        }

        [Test]
        [TestCase(3, IcarusEqualityFilter.LessThan)]
        [TestCase(3, IcarusEqualityFilter.LessThanOrEqual)]
        [TestCase(0, IcarusEqualityFilter.GreaterThan)]
        [TestCase(0, IcarusEqualityFilter.GreaterThanOrEqual)]
        [TestCase(3, IcarusEqualityFilter.NotEqual)]
        [ExpectedException(typeof(IcarusException))]
        public void Find_Filter_FailForMultiple(object value, IcarusEqualityFilter filter)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("SomeInt", value, filter);
        }

        [Test]
        public void Find_JsonPath_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("$[?(@.SomeInt == 2)]");
            Assert.IsNotNull(item);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Find_JsonPath_Boolean_Success(bool value)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeBool = true, Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeBool = false, Temp = "Anything2" };

            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var items = collection.InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = collection.Find("$[?(@.SomeBool == " + value.ToString().ToLower() + ")]");
            Assert.IsNotNull(item);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(value, item.SomeBool);
            Assert.AreEqual(value ? 1 : 2, item._id);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Find_JsonPath_FailForMultiple()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 2, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find("$[?(@.SomeInt == 2)]");
        }

        #endregion

        #region Find Many Test

        [Test]
        public void FindMany_Ids_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).FindMany(new[] { 1L, 2L });
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);

            Assert.AreEqual("Hello1", items[0].SomeString);
            Assert.AreEqual(1, items[0].SomeInt);
            Assert.AreEqual(1, items[0]._id);

            Assert.AreEqual("Hello2", items[1].SomeString);
            Assert.AreEqual(1, items[1].SomeInt);
            Assert.AreEqual(2, items[1]._id);
        }

        [Test]
        public void FindMany_Ids_Success_Nothing()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(obj1);

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).FindMany(new[] { 1L, 2L });
            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);

            Assert.AreEqual("Hello1", items[0].SomeString);
            Assert.AreEqual(1, items[0].SomeInt);
            Assert.AreEqual(1, items[0]._id);

            Assert.IsNull(items[1]);
        }

        [Test]
        [TestCase(1, IcarusEqualityFilter.Equal, 1, false)]
        [TestCase(3, IcarusEqualityFilter.Equal, 0, true)]
        [TestCase(3, IcarusEqualityFilter.LessThan, 2, false)]
        [TestCase(2, IcarusEqualityFilter.LessThan, 1, false)]
        [TestCase(1, IcarusEqualityFilter.LessThan, 0, true)]
        [TestCase(3, IcarusEqualityFilter.LessThanOrEqual, 2, false)]
        [TestCase(1, IcarusEqualityFilter.LessThanOrEqual, 1, false)]
        [TestCase(0, IcarusEqualityFilter.LessThanOrEqual, 0, true)]
        [TestCase(1, IcarusEqualityFilter.GreaterThan, 1, false)]
        [TestCase(0, IcarusEqualityFilter.GreaterThan, 2, false)]
        [TestCase(2, IcarusEqualityFilter.GreaterThan, 0, true)]
        [TestCase(3, IcarusEqualityFilter.GreaterThanOrEqual, 0, true)]
        [TestCase(0, IcarusEqualityFilter.GreaterThanOrEqual, 2, false)]
        [TestCase(2, IcarusEqualityFilter.GreaterThanOrEqual, 1, false)]
        [TestCase(2, IcarusEqualityFilter.NotEqual, 1, false)]
        [TestCase(3, IcarusEqualityFilter.NotEqual, 2, false)]
        public void FindMany_Filter_Success(object value, IcarusEqualityFilter filter, int expectedAmount, bool isNull)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).FindMany("SomeInt", value, filter);

            if (isNull)
            {
                Assert.IsNull(items2);
            }
            else
            {
                Assert.IsNotNull(items2);
                Assert.AreEqual(expectedAmount, items2.Count);
            }
        }

        [Test]
        public void FindMany_JsonPath_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).FindMany("$[?(@.SomeInt == 2)]");
            Assert.IsNotNull(items2);
            Assert.AreEqual(1, items2.Count);
            Assert.AreEqual(2, items2[0]._id);
        }

        #endregion

        #region Remove Tests

        [Test]
        public void Remove_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item2 = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Remove(1);
            Assert.IsNotNull(item2);
            Assert.AreEqual("Hello1", item2.SomeString);
            Assert.AreEqual(1, item2.SomeInt);

            item2 = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find(1);
            Assert.IsNull(item2);
        }

        [Test]
        public void Remove_ByItem_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var items = collection.InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item2 = collection.Remove(obj1);
            Assert.IsNotNull(item2);
            Assert.AreEqual("Hello1", item2.SomeString);
            Assert.AreEqual(1, item2.SomeInt);

            item2 = collection.Find(1);
            Assert.IsNull(item2);
        }

        #endregion

        #region Remove Many Tests

        [Test]
        public void RemoveMany_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).RemoveMany(new[] { 1L, 2L });
            Assert.IsNotNull(items2);
            Assert.AreEqual(2, items2.Count);
            Assert.AreEqual("Hello1", items2[0].SomeString);
            Assert.AreEqual(1, items2[0].SomeInt);
            Assert.AreEqual(1, items2[0]._id);

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find(1);
            Assert.IsNull(item);
        }

        [Test]
        public void RemoveMany_ByItem_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var items = collection.InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = collection.RemoveMany(new[] { obj1, obj2 });
            Assert.IsNotNull(items2);
            Assert.AreEqual(2, items2.Count);
            Assert.AreEqual("Hello1", items2[0].SomeString);
            Assert.AreEqual(1, items2[0].SomeInt);
            Assert.AreEqual(1, items2[0]._id);

            var item = collection.Find(1);
            Assert.IsNull(item);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };

            var item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Insert(obj1);
            Assert.AreEqual(1, item._id);

            obj1.SomeString = "Updated";
            item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Update(obj1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello1", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);

            item = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection).Find(1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Updated", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);
        }

        #endregion

        #region Update Many Tests

        [Test]
        public void UpdateMany_Success()
        {
            var icarus = IcarusClient.Instance;
            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = collection.InsertMany(new SomeObject[] { obj1, obj2 });
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            obj1.SomeString = "Updated";
            obj2.SomeString = "Updated";
            items = collection.UpdateMany(new SomeObject[] { obj1, obj2 });
            Assert.IsNotNull(items);

            Assert.AreEqual("Hello1", items[0].SomeString);
            Assert.AreEqual(1, items[0].SomeInt);
            Assert.AreEqual(1, items[0]._id);

            var item = collection.Find(1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Updated", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);
        }

        #endregion

        #region Large Datasets

        [Test]
        [TestCase(10, 6)]
        //[TestCase(100, 60)]
        //[TestCase(1000, 600)]
        //[TestCase(10000, 6000)]
        //[TestCase(100000, 60000)]
        public void LargeDataSet(int amount, int toFind)
        {
            var icarus = IcarusClient.Instance;
            var collection = icarus.GetDataStore(_dataStore).GetCollection<SomeObject>(_collection);

            for (var i = 1; i <= amount; i++)
            {
                var _obj = new SomeObject() { SomeInt = i, SomeString = "Hello" + i, Temp = "Anything" + i };
                var _item = collection.Insert(_obj, false);
                Assert.AreEqual(i, _item._id);
            }

            collection.Persist();
            collection.Refresh();

            var item = collection.Find(toFind);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello" + toFind, item.SomeString);
            Assert.AreEqual(toFind, item.SomeInt);
            Assert.AreEqual(toFind, item._id);
        }

        #endregion

    }


    public class SomeObject : IcarusObject
    {
        public int SomeInt;
        public string SomeString;

        private string _temp;
        public string Temp
        {
            get { return _temp.ToString(); }
            set { _temp = value; }
        }

        public DateTime SomeDate;
        public bool SomeBool;
    }

}

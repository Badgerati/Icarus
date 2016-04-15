/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using NUnit.Framework;
using Icarus.Core;
using System.IO;

namespace Icarus.Test
{
    [TestFixture]
    public class BasicTests
    {

        [TestFixtureSetUp]
        public void SetupBase()
        {
            IcarusClient.Instance.Initialise(".");
        }

        [TearDown]
        public void Teardown()
        {
            var path = Path.GetFullPath(".\\Test\\Store.json");
            if (File.Exists(path))
            {
                using (var file = File.CreateText(path))
                {
                    file.WriteLine("{}");
                }

                IcarusClient.Instance.GetDataStore("Test").GetCollection<SomeObject>("Store").Refresh(false);
            }
        }

        #region Insert Tests

        [Test]
        public void Insert_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello", Temp = "Anything" };

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Insert(obj);
            Assert.AreEqual(1, item._id);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Insert_Fail_NoItem()
        {
            var icarus = IcarusClient.Instance;
            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Insert(null);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Insert_Fail_NullReference()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello" };
            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Insert(obj);
        }

        #endregion

        #region Insert Many Tests

        [Test]
        public void InsertMany_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);
        }

        [Test]
        public void InsertMany_Fail_NoItems()
        {
            var icarus = IcarusClient.Instance;
            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(null);
            Assert.IsNull(items);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void InsertMany_Fail_NullReference()
        {
            var icarus = IcarusClient.Instance;
            var obj = new SomeObject() { SomeInt = 1, SomeString = "Hello" };
            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj });
        }

        #endregion

        #region Find Tests

        [Test]
        public void Find_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find(1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello1", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);
        }

        [Test]
        public void Find_Success_Nothing()
        {
            var icarus = IcarusClient.Instance;
            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find(1);
            Assert.IsNull(item);
        }

        [Test]
        public void Find_Cache_Success()
        {
            var icarus = IcarusClient.Instance;
            var collection = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store");

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
        [TestCase(1, IcarusEqualityFilter.Equal, false)]
        [TestCase(3, IcarusEqualityFilter.Equal, true)]
        [TestCase(2, IcarusEqualityFilter.LessThan, false)]
        [TestCase(1, IcarusEqualityFilter.LessThan, true)]
        [TestCase(1, IcarusEqualityFilter.LessThanOrEqual, false)]
        [TestCase(0, IcarusEqualityFilter.LessThanOrEqual, true)]
        [TestCase(1, IcarusEqualityFilter.GreaterThan, false)]
        [TestCase(2, IcarusEqualityFilter.GreaterThan, true)]
        [TestCase(3, IcarusEqualityFilter.GreaterThanOrEqual, true)]
        [TestCase(2, IcarusEqualityFilter.GreaterThanOrEqual, false)]
        [TestCase(2, IcarusEqualityFilter.NotEqual, false)]
        public void Find_Filter_Success(object value, IcarusEqualityFilter filter, bool isNull)
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find("SomeInt", value, filter);

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

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find("SomeInt", value, filter);
        }

        [Test]
        public void Find_JsonPath_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find("$[?(@.SomeInt == 2)]");
            Assert.IsNotNull(item);
        }

        [Test]
        [ExpectedException(typeof(IcarusException))]
        public void Find_JsonPath_FailForMultiple()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 2, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 2, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find("$[?(@.SomeInt == 2)]");
        }

        #endregion

        #region Find Many Test

        [Test]
        public void FindMany_Ids_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };
            var obj2 = new SomeObject() { SomeInt = 1, SomeString = "Hello2", Temp = "Anything2" };

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").FindMany(new[] { 1L, 2L });
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
            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Insert(obj1);

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").FindMany(new[] { 1L, 2L });
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

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").FindMany("SomeInt", value, filter);

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

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").FindMany("$[?(@.SomeInt == 2)]");
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

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var item2 = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Remove(1);
            Assert.IsNotNull(item2);
            Assert.AreEqual("Hello1", item2.SomeString);
            Assert.AreEqual(1, item2.SomeInt);

            item2 = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find(1);
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

            var items = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").InsertMany(new[] { obj1, obj2 });
            Assert.AreEqual(2, items.Count);
            Assert.AreEqual(1, items[0]._id);
            Assert.AreEqual(2, items[1]._id);

            var items2 = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").RemoveMany(new[] { 1L, 2L });
            Assert.IsNotNull(items2);
            Assert.AreEqual(2, items2.Count);
            Assert.AreEqual("Hello1", items2[0].SomeString);
            Assert.AreEqual(1, items2[0].SomeInt);
            Assert.AreEqual(1, items2[0]._id);

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find(1);
            Assert.IsNull(item);
        }

        #endregion

        #region Update Tests

        [Test]
        public void Update_Success()
        {
            var icarus = IcarusClient.Instance;
            var obj1 = new SomeObject() { SomeInt = 1, SomeString = "Hello1", Temp = "Anything1" };

            var item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Insert(obj1);
            Assert.AreEqual(1, item._id);

            obj1.SomeString = "Updated";
            item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Update(obj1);
            Assert.IsNotNull(item);
            Assert.AreEqual("Hello1", item.SomeString);
            Assert.AreEqual(1, item.SomeInt);
            Assert.AreEqual(1, item._id);

            item = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store").Find(1);
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
            var collection = icarus.GetDataStore("Test").GetCollection<SomeObject>("Store");

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
    }

}

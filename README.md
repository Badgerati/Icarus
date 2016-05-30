Icarus
======

[![Build Status](https://travis-ci.org/Badgerati/Icarus.svg?branch=master)](https://travis-ci.org/Badgerati/Icarus)
[![Build status](https://ci.appveyor.com/api/projects/status/4p7lnp05lebirjxm?svg=true)](https://ci.appveyor.com/project/Badgerati/icarus)
[![Code Climate](https://codeclimate.com/github/Badgerati/Icarus/badges/gpa.svg)](https://codeclimate.com/github/Badgerati/Icarus)
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/Badgerati/Icarus/master/LICENSE.txt)

Icarus is a simple JSON DataStore tool, no running services just reference the library and point Icarus to a location. All data will be saved at the location in plain JSON format. You can also specify to encrypt all contents of collections as well. Icarus uses JSON.NET behind the scenes.

All data is stored on memory when your application starts, for quicker queries and processing.

Notes:
 * Icarus is in beta, so currently is designed for small to medium datasets (about 1 - 50,000 records per collection). If you're using large datasets, it might be best to use something like MongoDB. (you can risk using Icarus if you like though, I won't stop you!)
 * Icarus has not been tested on parallel processed yet, so use at risk.

Features
========

* Data is stored in JSON format.
* Ability to encrypt data so it isn't store in plain JSON.
* All data is stored into some specified location.
* DataStores can have multiple collections.
* Fast and clean.
* Able to Insert, Update, Remove and Find.
* Ability to cache data for extra speed.

Performance Tips
================
If you find that inserting/updating or removing records is slow, then each of the methods has an optional `persist` parameter that you can pass. By default this is enabled, so every time you insert/update or remove a record it is persisted to storage. If, however, you pass this in as false then your calls will dramatically increase in speed.

Please note however, if you're choosing to disable persisting then you will **NEED** to persist the collection to storage every now and then, and definitely when your application closes. I can't imagine you'd want to lose everything, right?

(If you're working with medium to large datasets, it might be wise to have persisting disabled and to persist once in a while instead)

Installing Icarus
=================

Icarus can be installed via NuGet:

```bash
Install-Package Icarus.Core
```

You can also clone the repository and build the `Icarus.Core` project, and use the `Icarus.Core.dll`.

Usage
=====

Using Icarus might feel somewhat similar to MongoDB. You can insert, update, remove and find your data inside of collections for each data store you create.

Initialise the IcarusClient
---------------------------
The main point of entry for everything is the `IcarusClient`. This is a lazy initialised class for accessing your data stores and collections. To initialise the client you use:

```C#
IcarusClient.Instance.Initialise("<SOME_LOCATION>");
```

The `<SOME_LOCATION>` is where you would place the location to where you want Icarus to write everything. The `Initialise` method should really only be called once (it can be called multiple times if you wish), so its best calling it when your application spins up.

Creating Data Stores and Collections
-----------------------------------
A data store contains many collections, and Icarus can manage multiple data stores. Just think of a data store as being a database, and the collections being a database's tables.

The only location you need to setup manually is the location where Icarus will write everything. This is the location that will be used to initialise the `IcarusClient`.

To create your data stores and collections is very simple, just use them in code. If the data store or collection doesn't exist, Icarus will create them for you!

To get your data store use:

```C#
var datastore = IcarusClient.Instance.GetDataStore("<DATA_STORE_NAME>");
```

and to get your collection from the data store use:

```C#
var collection = datastore.GetCollection<OBJECT_TYPE>("<COLLECTION_NAME>");
```

The `<OBJECT_TYPE>` for the collection is the generic type that Icarus will use when deserializing the JSON objects. It is important to note that this generic type MUST inherit the `IcarusObject` type.

Encrypting Collections
---------------------
The encryption will be done via AES256, using the Haxxor framework I wrote a while back. The key and IV are randomly generated each time, and stored with the data for decryption later on.

The default for encryption is disabled. If the `IsEncryptionEnabled` in the `IcarusClient` is set to true, then this acts like a total override, enabling encryption for all data stores and collections. If it is left as false (the default), the encryption can be enabled on a per collection basis:

```C#
var store = IcarusClient.Instance.GetDataStore(<DATA_STORE_NAME>);
var collection = store.GetCollection<OBJECT_TYPE>(<COLLECTION_NAME>, isEncrypted: <true_or_false>);
```

When retrieving the collection from a data store, you can specify the encryption type; again the default is false for encryption.

Also, it's possible to enable/disable encryption after it was already disabled/enabled and it will store appropriately. As a note however, when toggling you may have to wait for the client's/data store's cache to flush out. There are `Clear` methods to do this manually.

IcasusObject
------------
All objects that will be stored with Icarus must inherit the `IcarusObject` class. This class contains the `_id` property that Icarus will use to set the object's unique ID against. It is imperative that you do not alter this value at all.

```C#
public class Person : IcarusObject
{
}
```

The `_id` is a public value, so can be used as you wish.

Inserting Objects
-----------------
There are two methods to insert data into your collections:

 * `Insert(<ITEM>)`
 * `InsertMany(<ITEM_ARRAY>)`

To insert an object, just do:

```C#
var person = new Person();
person.Name = "Bob";
person.Age = 21;

var collection = datastore.GetCollection<Person>("Person");
var item = collection.Insert(person);
```

The insert method will insert the object into the data store, and will return the item back with `_id` that Icarus has assigned it back. These unique IDs are what you will mainly pass throughout Icarus when removing and updating. If you ever need the ID, best thing to do is find the object with Icarus first and it will always return the ID for you (assuming the object exists in the collection, of course).

The InsertMany method is basically just like insert, but will obviously let you insert multiple objects instead:

```C#
var person1 = new Person();
person1.Name = "Bob";
person1.Age = 21;

var person2 = new Person();
person2.Name = "Claire";
person2.Age = 22;

var collection = datastore.GetCollection<Person>("Person");
var items = collection.InsertMany(new[] { person1, person2 });
```

This time, the InsertMany method will return a list of the objects inserted.

Finding Objects
---------------
There are 6 ways of finding objects with Icarus:

 * `Find(<ID>)`
 * `FindMany(<ID_ARRAY>)`
 * `Find(<JSON_PATH>)`
 * `FindMany(<JSON_PATH>)`
 * `Find(<FIELD_NAME>, <VALUE>, <EQUALITY_FILTER>)`
 * `FindMany(<FIELD_NAME>, <VALUE>, <EQUALITY_FILTER>)`

The first two are just finding objects by their unique ID. The latter ones are more advanced, two are for finding objects by manually specifying a JSONPath; and the last two is pass one of the object's field names, it's expected value and the type of equality to filter (to the field equals the value, less than, greater than, etc).

To find a single object by it's ID you can do:

```C#
var collection = datastore.GetCollection<Person>("Person");
var person = collection.Find(1);
```

To find multiple objects by their IDs you can use the following:

```C#
var collection = datastore.GetCollection<Person>("Person");
var persons = collection.FindMany(new[] { 1, 2, 3 });
```

The value returned by FindMany by IDs is a list of the objects found, with their `_id` set (This is true for all of the FindMany methods).

To find multiple objects by some JSONPath you can use the following:

```C#
var collection = datastore.GetCollection<Person>("Person");
var persons = collection.FindMany("$[?(@.Name == 'Bob')]");
```

This allows you to have far more advanced control over Icarus' queries. You can pass in what ever JSONPath you want to select objects. Something to note is that all objects are stored within a JSON array so will always start with `$[?(`.

The final method is to find multiple objects by field and name, for this you could use:

```C#
var collection = datastore.GetCollection<Person>("Person");
var persons = collection.FindMany("Name", "Bob", IcarusEqualityFilter.NotEqual);
```

This method will construct a simple JSONPath for you if you aren't familiar with them. The objects returned here will be all objects that have the field `"Name"` that are not equal to the value `"Bob"`. The equality filter can be:

 * Equal
 * NotEqual
 * LessThan
 * GreaterThan
 * LessThanOrEqual
 * GreaterThanOrEqual

Removing Objects
----------------
Like with inserting, there are two methods of removal:

 * `Remove(<ID>)`
 * `RemoveMany(<ID_ARRAY>)`

To remove an object you will need the unique ID given by Icarus. If you haven't got it, then you will need to call one of the find methods first.

To remove an object do:

```C#
var collection = datastore.GetCollection<Person>("Person");
var person = collection.FindMany("Name", "Bob", IcarusEqualityFilter.Equal).Single();
collection.Remove(person._id);
```

This will remove the object with passed unique ID. The remove method will also return the object that was removed.

The RemoveMany method is basically the same, just it take an array of IDs and returns a list of removed objects:

```C#
var collection = datastore.GetCollection<Person>("Person");
var persons = collection.FindMany("Age", 11, IcarusEqualityFilter.Equal);
collection.RemoveMany(persons.Select(p => p._id).ToArray());
```

Updating Objects
----------------
The update calls will update a passed item in the collection, so long as the object has already been previously inserted. There are two update methods:

 * `Update(<ITEM>)`
 * `UpdateMany(<ITEM_ARRAY>)`

To update one item you can use the following:

```C#
var collection = datastore.GetCollection<Person>("Person");
var person = collection.FindMany("Name", "Bob", IcarusEqualityFilter.Equal).Single();

person.Name = "John";
collection.Update(person);
```

The update methods will each return the objects before the object was updated.

To update multiple objects you can use the following:

```C#
var collection = datastore.GetCollection<Person>("Person");
var persons = collection.FindMany("Age", 11, IcarusEqualityFilter.Equal);

for (var i = 0; i < persons.Count; i++)
{
	persons[i].Age = 12;
}

collection.UpdateMany(persons);
```

To Do
=====

* Look into making Icarus work better with massive datasets.
* Get Icarus working with parallel processes.

Bugs and Feature Requests
=========================

For any bugs you may find or features you wish to request, please create an [issue](https://github.com/Badgerati/Icarus/issues "Issues") in GitHub.


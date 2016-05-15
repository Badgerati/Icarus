/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;

namespace Icarus.Core
{
    public class IcarusCollection<T> : IIcarusCollection<T> where T : IIcarusObject
    {

        #region Constants

        private const string FileExtension = ".json";
        private const string NextPrimaryIdKey = "NextPrimaryId";
        private const string DataKey = "Data";
        private const string PrimaryIdKey = "_id";

        private const long DefaultNextPrimaryId = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName { get; private set; }

        /// <summary>
        /// Gets the collection location.
        /// </summary>
        /// <value>
        /// The collection location.
        /// </value>
        public string CollectionLocation { get; private set; }

        /// <summary>
        /// Gets the data store location.
        /// </summary>
        /// <value>
        /// The data store location.
        /// </value>
        public string DataStoreLocation { get; private set; }

        /// <summary>
        /// Gets the next primary identifier.
        /// </summary>
        /// <value>
        /// The next primary identifier.
        /// </value>
        public long NextPrimaryId
        {
            get { return _nextPrimaryId; }
            private set { _nextPrimaryId = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled. Default is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [caching enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool CachingEnabled { get; set; }

        #endregion

        #region Fields

        private bool _isAccessEveryone;

        private long _nextPrimaryId = DefaultNextPrimaryId;
        private IDictionary<long, JToken> _primaryIndex = default(IDictionary<long, JToken>);

        private JObject _json = default(JObject);
        private JArray _data
        {
            get { return (JArray)_json[DataKey]; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IcarusCollection" /> class.
        /// </summary>
        /// <param name="dataStoreLocation">The data store location.</param>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> [Icarus data is accessible by everyone].</param>
        public IcarusCollection(string dataStoreLocation, string collectionName, bool isAccessEveryone = false)
        {
            var path = Path.Combine(dataStoreLocation, collectionName + FileExtension);
            _isAccessEveryone = isAccessEveryone;

            // Create collection if it doesn't exist
            if (!File.Exists(path))
            {
                using (var file = File.CreateText(path))
                {
                    file.WriteLine("{}");
                }
            }

            // toggle access control to everyone
            var security = File.GetAccessControl(path);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow);

            if (_isAccessEveryone)
            {
                security.AddAccessRule(rule);
            }
            else
            {
                security.RemoveAccessRule(rule);
            }

            File.SetAccessControl(path, security);

            // set collection locations
            CachingEnabled = true;
            CollectionName = collectionName;
            CollectionLocation = path;
            DataStoreLocation = dataStoreLocation;

            // Load contents of file
            using (var reader = File.OpenText(CollectionLocation))
            {
                _json = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }

            // Set next primary ID
            if (_json.HasValues)
            {
                NextPrimaryId = _json.Value<long>(NextPrimaryIdKey);
            }
            else
            {
                NextPrimaryId = DefaultNextPrimaryId;
                _json.Add(DataKey, new JArray());
            }

            // Indexes
            _primaryIndex = new Dictionary<long, JToken>(10);
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Inserts the specified item.
        /// </summary>
        /// <param name="item">The item to insert.</param>
        /// <param name="persist">if set to <c>true</c> [persist] to the DataStore.</param>
        /// <returns>
        /// The item with its unique ID.
        /// </returns>
        /// <exception cref="IcarusException">
        /// Cannot insert null item.
        /// or
        /// Attempting to insert item that has already been inserted, _id:
        /// or
        /// An exception occurred while trying to insert the item.
        /// </exception>
        public T Insert(T item, bool persist = true)
        {
            if (Equals(item, default(T)))
            {
                throw new IcarusException("Cannot insert null item.");
            }

            if (item._id != 0)
            {
                throw new IcarusException("Attempting to insert item that has already been inserted, _id: " + item._id);
            }

            item._id = NextPrimaryId++;
            var _jsonObj = default(JObject);
            var _error = false;

            try
            {
                // Serialize and add the object
                _jsonObj = JObject.FromObject(item);
                _data.Add(_jsonObj);

                // If we pass serialization, add object to index cache
                if (CachingEnabled)
                {
                    _primaryIndex.Add(item._id, _jsonObj);
                }

                // Return the item
                return item;
            }
            catch (Exception ex)
            {
                // Cleanup
                _error = true;
                NextPrimaryId--;
                _primaryIndex.Remove(item._id);
                item._id = 0;

                if (_jsonObj != default(JObject))
                {
                    _data.Remove(_jsonObj);
                }

                throw new IcarusException("An exception occurred while trying to insert the item.", ex);
            }
            finally
            {
                if (persist && !_error)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Inserts all the specified items.
        /// </summary>
        /// <param name="items">The items to insert.</param>
        /// <param name="persist">if set to <c>true</c> [persist] to the DataStore.</param>
        /// <returns>
        /// The inserted items with their unique IDs set.
        /// </returns>
        /// <exception cref="IcarusException">Attempting to insert item that has already been inserted, _id:</exception>
        public IList<T> InsertMany(T[] items, bool persist = true)
        {
            if (items == default(T[]) || !items.Any())
            {
                return default(IList<T>);
            }

            if (items.Any(x => x._id != 0))
            {
                throw new IcarusException("Attempting to insert item that has already been inserted, _id: " + items.Where(x => x._id != 0).First()._id);
            }

            try
            {
                return items.Select(x => Insert(x, false)).ToList();
            }
            finally
            {
                if (persist)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Finds the item with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search.</param>
        /// <returns>
        /// The item if found, else null.
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        public T Find(long id)
        {
            try
            {
                // Search for a token matching the ID
                var _item = GetTokenById(id);
                var _castItem = default(T);

                if (_item != default(JToken))
                {
                    _castItem = CastToObject(_item);

                    // If caching, and not in index then add to index
                    if (CachingEnabled && !_primaryIndex.ContainsKey(id))
                    {
                        _primaryIndex.Add(id, _item);
                    }
                }

                return _castItem;
            }
            catch (Exception ex)
            {
                throw new IcarusException("An exception was thrown while retrieving from the collection.", ex);
            }
        }

        /// <summary>
        /// Finds the items with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers to search.</param>
        /// <returns>
        /// A list of items found with their identifiers, null is returned if no item matches the ID
        /// </returns>
        public IList<T> FindMany(long[] ids)
        {
            if (ids == default(long[]) || !ids.Any())
            {
                return default(IList<T>);
            }

            return ids.Select(x => Find(x)).ToList();
        }

        /// <summary>
        /// Finds an item using the specified json path from the Icarus DataStore.
        /// </summary>
        /// <param name="jsonPath">The json path to search.</param>
        /// <returns>
        /// An item found with their identifiers
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        public T Find(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                return default(T);
            }

            try
            {
                // Search for a tokens matching the JSONPath
                var _item = GetTokenByJsonPath(jsonPath);
                var _castItem = default(T);

                if (_item != default(JToken))
                {
                    _castItem = CastToObject(_item);

                    // If caching, and not in index then add to index
                    if (CachingEnabled && !_primaryIndex.ContainsKey(_castItem._id))
                    {
                        _primaryIndex.Add(_castItem._id, _item);
                    }
                }

                return _castItem;
            }
            catch (Exception ex)
            {
                throw new IcarusException("An exception was thrown while retrieving from the collection.", ex);
            }
        }

        /// <summary>
        /// Finds items using the specified json path from the Icarus DataStore.
        /// </summary>
        /// <param name="jsonPath">The json path to search.</param>
        /// <returns>
        /// A list of items found with their identifiers
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        public IList<T> FindMany(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                return default(IList<T>);
            }

            try
            {
                // Search for a tokens matching the JSONPath
                var _items = GetTokensByJsonPath(jsonPath);

                if (_items != default(IEnumerable<JToken>) && _items.Any())
                {
                    return _items.Select(x => CastToObject(x)).ToList();
                }

                return default(IList<T>);
            }
            catch (Exception ex)
            {
                throw new IcarusException("An exception was thrown while retrieving from the collection.", ex);
            }
        }

        /// <summary>
        /// Finds an item using the specified field name, value and equality filter.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="filter">The filter to use when searching.</param>
        /// <returns>
        /// An item found with their identifiers
        /// </returns>
        public T Find(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return default(T);
            }

            return Find(ConstructJsonPathFromFieldValuePair(fieldName, value, filter));
        }

        /// <summary>
        /// Finds the items using the specified field name, value and equality filter.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="filter">The filter to use when searching.</param>
        /// <returns>
        /// A list of items found with their identifiers
        /// </returns>
        public IList<T> FindMany(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return default(IList<T>);
            }

            return FindMany(ConstructJsonPathFromFieldValuePair(fieldName, value, filter));
        }

        /// <summary>
        /// Returns all objects within the collection.
        /// </summary>
        /// <returns>All objects in the collection.</returns>
        public IList<T> All()
        {
            return FindMany("$[?(@._id > 0)]");
        }

        /// <summary>
        /// Updates an item with the specified identifier.
        /// </summary>
        /// <param name="item">The item to be used when updating the item with passed identifier.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The item before updating.
        /// </returns>
        /// <exception cref="IcarusException">Cannot update an object with null.
        /// or
        /// An exception was thrown while updating an item in the Icarus DataStore.</exception>
        public T Update(T item, bool persist = true)
        {
            if (Equals(item, default(T)))
            {
                throw new IcarusException("Cannot update an object with null.");
            }

            if (item._id == 0)
            {
                throw new IcarusException("Attempting to update an item that has not yet been inserted into the collection");
            }

            var _item = default(JToken);
            var _newItem = default(JObject);
            var _castItem = default(T);
            var _error = false;

            try
            {
                _item = GetTokenById(item._id);
                if (_item == default(JToken))
                {
                    return default(T);
                }

                _newItem = JObject.FromObject(item);

                if (_newItem != default(JToken))
                {
                    _castItem = CastToObject(_item);
                    _item.Replace(_newItem);

                    if (CachingEnabled)
                    {
                        if (_primaryIndex.ContainsKey(item._id))
                        {
                            _primaryIndex[item._id] = _newItem;
                        }
                        else
                        {
                            _primaryIndex.Add(item._id, _newItem);
                        }
                    }

                    return _castItem;
                }

                return default(T);
            }
            catch (Exception ex)
            {
                // Cleanup
                _error = true;

                if (_item != default(JToken) && _newItem != default(JToken))
                {
                    _newItem.Replace(_item);
                }

                if (_primaryIndex.ContainsKey(item._id))
                {
                    _primaryIndex[item._id] = _item;
                }

                throw new IcarusException("An exception was thrown while updating an item in the collection.", ex);
            }
            finally
            {
                if (persist && !_error)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Updates the items with the specified identifier.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items before updating.
        /// </returns>
        /// <exception cref="IcarusException">Attempting to update an item that has not yet been inserted into the collection, _id:</exception>
        public IList<T> UpdateMany(T[] items, bool persist = true)
        {
            if (items == default(T[]) || !items.Any())
            {
                return default(IList<T>);
            }

            if (items.Any(x => x._id != 0))
            {
                throw new IcarusException("Attempting to update an item that has not yet been inserted into the collection, _id: " + items.Where(x => x._id != 0).First()._id);
            }

            try
            {
                return items.Select(x => Update(x, false)).ToList();
            }
            finally
            {
                if (persist)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Removes an item from the Icarus DataStore with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the item to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The item that was removed, null is returned if the item doesn't exist.
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while removing an item from the collection.</exception>
        public T Remove(long id, bool persist = true)
        {
            var _item = default(JToken);
            var _castItem = default(T);
            var _error = false;

            try
            {
                _item = GetTokenById(id);

                if (_item != default(JToken))
                {
                    _castItem = CastToObject(_item);
                    _primaryIndex.Remove(id);
                    _data.Remove(_item);

                    return _castItem;
                }

                return default(T);
            }
            catch (Exception ex)
            {
                // Cleanup
                _error = true;

                if (_item != default(JToken) && GetTokenById(id) == default(JToken))
                {
                    _data.Add(_item);
                }

                throw new IcarusException("An exception was thrown while removing an item from the collection.", ex);
            }
            finally
            {
                if (persist && !_error)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Removes an item from the Icarus DataStore.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The item that was removed, null is returned if the item doesn't exist.
        /// </returns>
        public T Remove(T item, bool persist = true)
        {
            if (Equals(item, default(T)))
            {
                return default(T);
            }

            return Remove(item._id, persist);
        }

        /// <summary>
        /// Removes the items from the Icarus DataStore with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers for items to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items that were removed, null is returned if the item doesn't exist.
        /// </returns>
        public IList<T> RemoveMany(long[] ids, bool persist = true)
        {
            if (ids == default(long[]) || !ids.Any())
            {
                return default(IList<T>);
            }

            try
            {
                return ids.Select(x => Remove(x, false)).ToList();
            }
            finally
            {
                if (persist)
                {
                    Persist();
                }
            }
        }

        /// <summary>
        /// Removes the items from the Icarus DataStore.
        /// </summary>
        /// <param name="items">The items to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items that were removed, null is returned if the item doesn't exist.
        /// </returns>
        public IList<T> RemoveMany(T[] items, bool persist = true)
        {
            if (items == default(T[]) || !items.Any())
            {
                return default(IList<T>);
            }

            return RemoveMany(items.Select(x => x._id).ToArray(), persist);
        }

        /// <summary>
        /// Persists this instance to the Icarus DataStore.
        /// If you're calling Inserts and Updates with persisting disabled,
        /// then it would be wise to call this method one even now and then.
        /// </summary>
        public void Persist()
        {
            try
            {
                // Update the next primary ID
                _json[NextPrimaryIdKey] = NextPrimaryId;

                // Write everything to the data store
                using (var writer = File.CreateText(CollectionLocation))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        _json.WriteTo(jsonWriter);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new IcarusException("An exception was thrown while persisting data to the Icarus DataStore.", ex);
            }
        }

        /// <summary>
        /// Clears the cache, regardless if caching is disabled.
        /// </summary>
        public void ClearCache()
        {
            _primaryIndex.Clear();
        }

        /// <summary>
        /// Refreshes this instance, updating values from the Icarus DataStore.
        /// </summary>
        /// <param name="persistFirst">if set to <c>true</c> persist the data before refreshing.</param>
        public void Refresh(bool persistFirst = true)
        {
            if (persistFirst)
            {
                Persist();
            }

            // Load contents of file
            using (var reader = File.OpenText(CollectionLocation))
            {
                _json = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
            }

            // Set next primary ID
            if (_json.HasValues)
            {
                NextPrimaryId = _json.Value<long>(NextPrimaryIdKey);
            }
            else
            {
                NextPrimaryId = DefaultNextPrimaryId;
                _json.Add(DataKey, new JArray());
            }

            // Indexes
            _primaryIndex = new Dictionary<long, JToken>(10);
        }

        #endregion

        #region Private Helpers

        private T CastToObject(JToken item)
        {
            if (item == default(JToken))
            {
                return default(T);
            }

            return item.ToObject<T>();
        }

        private JToken GetTokenById(long id)
        {
            return CachingEnabled && _primaryIndex.ContainsKey(id)
                ? _primaryIndex[id]
                : GetTokenByJsonPath("$[?(@." + PrimaryIdKey + " == " + id + ")]");
        }

        private IEnumerable<JToken> GetTokensByJsonPath(string jsonPath)
        {
            return _data.SelectTokens(jsonPath);
        }

        private JToken GetTokenByJsonPath(string jsonPath)
        {
            return _data.SelectToken(jsonPath);
        }

        private string ConstructJsonPathFromFieldValuePair(string fieldName, object value, IcarusEqualityFilter filter)
        {
            return "$[?(@." + fieldName + " " + MapEqualityFilter(filter) + " " + MapValueObjectType(value) + ")]";
        }

        private string MapEqualityFilter(IcarusEqualityFilter filter)
        {
            switch (filter)
            {
                default:
                case IcarusEqualityFilter.Equal:
                    return "==";

                case IcarusEqualityFilter.GreaterThan:
                    return ">";

                case IcarusEqualityFilter.GreaterThanOrEqual:
                    return ">=";

                case IcarusEqualityFilter.LessThan:
                    return "<";

                case IcarusEqualityFilter.LessThanOrEqual:
                    return "<=";

                case IcarusEqualityFilter.NotEqual:
                    return "!=";
            }
        }

        private string MapValueObjectType(object value)
        {
            if (value is string)
            {
                return "\'" + value + "\'";
            }
            else if (value is DateTime)
            {
                return "\'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK") + "\'";
            }
            else if (value is bool)
            {
                return value.ToString().ToLowerInvariant();
            }

            return value.ToString();
        }

        #endregion

    }
}

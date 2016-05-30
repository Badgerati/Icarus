/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System.Collections.Generic;

namespace Icarus.Core
{

    public interface IIcarusCollection { }

    public interface IIcarusCollection<T> : IIcarusCollection where T : IIcarusObject
    {

        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        string CollectionName { get; }

        /// <summary>
        /// Gets the collection location.
        /// </summary>
        /// <value>
        /// The collection location.
        /// </value>
        string CollectionLocation { get; }

        /// <summary>
        /// Gets the data store location.
        /// </summary>
        /// <value>
        /// The data store location.
        /// </value>
        string DataStoreLocation { get; }

        /// <summary>
        /// Gets the next primary identifier.
        /// </summary>
        /// <value>
        /// The next primary identifier.
        /// </value>
        long NextPrimaryId { get; }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled. Default is true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [caching enabled]; otherwise, <c>false</c>.
        /// </value>
        bool CachingEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is encryted.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is encryted; otherwise, <c>false</c>.
        /// </value>
        bool IsEncryted { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is access everyone.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is access everyone; otherwise, <c>false</c>.
        /// </value>
        bool IsAccessEveryone { get; }


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
        T Insert(T item, bool persist = true);

        /// <summary>
        /// Inserts all the specified items.
        /// </summary>
        /// <param name="items">The items to insert.</param>
        /// <param name="persist">if set to <c>true</c> [persist] to the DataStore.</param>
        /// <returns>
        /// The inserted items with their unique IDs set.
        /// </returns>
        /// <exception cref="IcarusException">Attempting to insert item that has already been inserted, _id:</exception>
        IList<T> InsertMany(T[] items, bool persist = true);

        /// <summary>
        /// Finds the item with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search.</param>
        /// <returns>
        /// The item if found, else null.
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        T Find(long id);

        /// <summary>
        /// Finds an item using the specified json path from the Icarus DataStore.
        /// </summary>
        /// <param name="jsonPath">The json path to search.</param>
        /// <returns>
        /// An item found with their identifiers
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        T Find(string jsonPath);

        /// <summary>
        /// Finds an item using the specified field name, value and equality filter.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="filter">The filter to use when searching.</param>
        /// <returns>
        /// An item found with their identifiers
        /// </returns>
        T Find(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal);

        /// <summary>
        /// Finds the items with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers to search.</param>
        /// <returns>
        /// A list of items found with their identifiers, null is returned if no item matches the ID
        /// </returns>
        IList<T> FindMany(long[] ids);

        /// <summary>
        /// Finds items using the specified json path from the Icarus DataStore.
        /// </summary>
        /// <param name="jsonPath">The json path to search.</param>
        /// <returns>
        /// A list of items found with their identifiers
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while retrieving from the collection.</exception>
        IList<T> FindMany(string jsonPath);

        /// <summary>
        /// Finds the items using the specified field name, value and equality filter.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="filter">The filter to use when searching.</param>
        /// <returns>
        /// A list of items found with their identifiers
        /// </returns>
        IList<T> FindMany(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal);

        /// <summary>
        /// Returns all objects within the collection.
        /// </summary>
        /// <returns>All objects in the collection.</returns>
        IList<T> All();

        /// <summary>
        /// Removes an item from the Icarus DataStore with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the item to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The item that was removed, null is returned if the item doesn't exist.
        /// </returns>
        /// <exception cref="IcarusException">An exception was thrown while removing an item from the collection.</exception>
        T Remove(long id, bool persist = true);

        /// <summary>
        /// Removes an item from the Icarus DataStore.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The item that was removed, null is returned if the item doesn't exist.
        /// </returns>
        T Remove(T item, bool persist = true);

        /// <summary>
        /// Removes the items from the Icarus DataStore with the specified identifiers.
        /// </summary>
        /// <param name="ids">The identifiers for items to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items that were removed, null is returned if the item doesn't exist.
        /// </returns>
        IList<T> RemoveMany(long[] ids, bool persist = true);

        /// <summary>
        /// Removes the items from the Icarus DataStore.
        /// </summary>
        /// <param name="items">The items to be removed.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items that were removed, null is returned if the item doesn't exist.
        /// </returns>
        IList<T> RemoveMany(T[] items, bool persist = true);

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
        T Update(T item, bool persist = true);

        /// <summary>
        /// Updates the items with the specified identifier.
        /// </summary>
        /// <param name="items">The items to be updated.</param>
        /// <param name="persist">if set to <c>true</c> [persist].</param>
        /// <returns>
        /// The items before updating.
        /// </returns>
        /// <exception cref="IcarusException">Attempting to update an item that has not yet been inserted into the collection, _id:</exception>
        IList<T> UpdateMany(T[] items, bool persist = true);

        /// <summary>
        /// Persists this instance to the Icarus DataStore.
        /// If you're calling Inserts and Updates with persisting disabled,
        /// then it would be wise to call this method one even now and then.
        /// </summary>
        void Persist();

        /// <summary>
        /// Clears the cache, regardless if caching is disabled.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Refreshes this instance, updating values from the Icarus DataStore.
        /// </summary>
        /// <param name="persistFirst">if set to <c>true</c> persist the data before refreshing.</param>
        void Refresh(bool persistFirst = true);

    }

}

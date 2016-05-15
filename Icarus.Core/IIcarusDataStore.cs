/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

namespace Icarus.Core
{

    public interface IIcarusDataStore
    {

        /// <summary>
        /// Gets the name of the data store.
        /// </summary>
        /// <value>
        /// The name of the data store.
        /// </value>
        string DataStoreName { get; }

        /// <summary>
        /// Gets the data store location.
        /// </summary>
        /// <value>
        /// The data store location.
        /// </value>
        string DataStoreLocation { get; }


        /// <summary>
        /// Gets the collection from the Icarus DataStore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>
        /// The collection from the DataStore.
        /// </returns>
        IIcarusCollection<T> GetCollection<T>(string collectionName) where T : IIcarusObject;

    }

}

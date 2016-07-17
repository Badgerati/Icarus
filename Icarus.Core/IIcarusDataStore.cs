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
        /// Gets the collection from the data store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="isEncryted">if set to <c>true</c> the collection be encrypted.</param>
        /// <returns>
        /// The collection from the data store.
        /// </returns>
        IIcarusCollection<T> GetCollection<T>(string collectionName, bool isEncryted = false) where T : IIcarusObject;

        /// <summary>
        /// Gets the collection from the data store.
        /// The isNew out flag will be true if the collection had to be created, false otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="isNew">if set to <c>true</c> then the collection was created as a part of this call.</param>
        /// <param name="isEncryted">if set to <c>true</c> the collection be encrypted.</param>
        /// <returns>
        /// The collection from the data store.
        /// </returns>
        IIcarusCollection<T> GetCollection<T>(string collectionName, out bool isNew, bool isEncryted = false) where T : IIcarusObject;

        /// <summary>
        /// Clears this instances cache of collections.
        /// </summary>
        void Clear();

    }

}

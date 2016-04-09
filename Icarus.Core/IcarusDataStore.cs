/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System.Collections.Generic;
using System.IO;

namespace Icarus.Core
{
    public class IcarusDataStore : IIcarusDataStore
    {

        #region Properties

        /// <summary>
        /// Gets the name of the data store.
        /// </summary>
        /// <value>
        /// The name of the data store.
        /// </value>
        public string DataStoreName { get; private set; }

        /// <summary>
        /// Gets the data store location.
        /// </summary>
        /// <value>
        /// The data store location.
        /// </value>
        public string DataStoreLocation { get; private set; }

        #endregion

        #region Fields

        private IDictionary<string, IIcarusCollection> _collections;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IcarusDataStore" /> class.
        /// </summary>
        /// <param name="icarusLocation">The icarus location.</param>
        /// <param name="dataStoreName">Name of the data store.</param>
        public IcarusDataStore(string icarusLocation, string dataStoreName)
        {
            var path = Path.Combine(icarusLocation, dataStoreName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            DataStoreName = dataStoreName;
            DataStoreLocation = path;

            _collections = new Dictionary<string, IIcarusCollection>();
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Gets the collection from the Icarus DataStore.
        /// </summary>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>The collection from the DataStore.</returns>
        public IIcarusCollection<T> GetCollection<T>(string collectionName) where T : IIcarusObject
        {
            if (!_collections.ContainsKey(collectionName))
            {
                _collections.Add(collectionName, new IcarusCollection<T>(DataStoreLocation, collectionName));
            }

            return (IIcarusCollection<T>)_collections[collectionName];
        }

        #endregion

    }
}

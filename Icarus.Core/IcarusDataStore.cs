/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

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
        private bool _isAccessEveryone;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IcarusDataStore" /> class.
        /// </summary>
        /// <param name="icarusLocation">The icarus location.</param>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> [Icarus data is accessible by everyone].</param>
        public IcarusDataStore(string icarusLocation, string dataStoreName, bool isAccessEveryone = false)
        {
            var path = Path.Combine(icarusLocation, dataStoreName);
            _isAccessEveryone = isAccessEveryone;

            // create store if it does not exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // toggle access control to everyone
            var security = Directory.GetAccessControl(path);
            var everyone = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var rule = new FileSystemAccessRule(everyone, FileSystemRights.Modify | FileSystemRights.Synchronize, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);

            if (_isAccessEveryone)
            {
                security.AddAccessRule(rule);
            }
            else
            {
                security.RemoveAccessRule(rule);
            }

            Directory.SetAccessControl(path, security);

            // set store locations
            DataStoreName = dataStoreName;
            DataStoreLocation = path;

            _collections = new Dictionary<string, IIcarusCollection>();
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Gets the collection from the Icarus DataStore.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns>
        /// The collection from the DataStore.
        /// </returns>
        public IIcarusCollection<T> GetCollection<T>(string collectionName) where T : IIcarusObject
        {
            if (!_collections.ContainsKey(collectionName))
            {
                _collections.Add(collectionName, new IcarusCollection<T>(DataStoreLocation, collectionName, _isAccessEveryone));
            }

            return (IIcarusCollection<T>)_collections[collectionName];
        }

        #endregion

    }
}

/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;
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
            var path = string.Empty;
            _isAccessEveryone = isAccessEveryone;

            // create store if it does not exist
            if (!Exists(icarusLocation, dataStoreName, out path))
            {
                Directory.CreateDirectory(path);
            }

            // toggle access control to everyone
            try
            {
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
            }
            catch (UnauthorizedAccessException)
            {
                // Not ideal, and needs logging, but if we fail to set the access controls we shouldn't bomb out
            }

            // set store locations
            DataStoreName = dataStoreName;
            DataStoreLocation = path;

            _collections = new Dictionary<string, IIcarusCollection>();
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Gets the collection from the data store.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <param name="isEncryted">if set to <c>true</c> the collection be encrypted.</param>
        /// <returns>
        /// The collection from the data store.
        /// </returns>
        public IIcarusCollection<T> GetCollection<T>(string collectionName, bool isEncryted = false) where T : IIcarusObject
        {
            var isNew = false;
            return GetCollection<T>(collectionName, out isNew, isEncryted: isEncryted);
        }

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
        public IIcarusCollection<T> GetCollection<T>(string collectionName, out bool isNew, bool isEncryted = false) where T : IIcarusObject
        {
            isNew = false;

            if (!_collections.ContainsKey(collectionName))
            {
                var path = string.Empty;
                isNew = !IcarusCollection<T>.Exists(DataStoreLocation, collectionName, out path);

                _collections.Add(collectionName,
                    new IcarusCollection<T>(
                        DataStoreLocation,
                        collectionName,
                        _isAccessEveryone,
                        (IcarusClient.Instance.IsEncryptionEnabled || isEncryted)));
            }

            return (IIcarusCollection<T>)_collections[collectionName];
        }

        /// <summary>
        /// Clears this instances cache of collections.
        /// </summary>
        public void Clear()
        {
            if (_collections == default(IDictionary<string, IIcarusCollection>))
            {
                return;
            }

            _collections.Clear();
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Checks to see if the data store specified exists at the specified icarus location.
        /// </summary>
        /// <param name="icarusLocation">The icarus location.</param>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <param name="fullpath">The full path to the data store is returned.</param>
        /// <returns>
        /// Returns true if the data store exists, false otherwise.
        /// </returns>
        public static bool Exists(string icarusLocation, string dataStoreName, out string fullpath)
        {
            fullpath = Path.Combine(icarusLocation, dataStoreName);
            return Directory.Exists(fullpath);
        }

        #endregion

    }
}

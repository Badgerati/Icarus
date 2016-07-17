/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Icarus.Core
{

    public class IcarusClient : IIcarus
    {

        #region Constants

        public const string DefaultTag = "default";

        #endregion

        #region Lazy Initialiser

        private static Lazy<IIcarus> _lazy = new Lazy<IIcarus>(() => new IcarusClient());
        public static IIcarus Instance
        {
            get { return _lazy.Value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default/first location of Icarus from the locations dictionary.
        /// </summary>
        /// <value>
        /// The default/first location of Icarus.
        /// </value>
        public string Location
        {
            get
            {
                if (!Locations.Any())
                {
                    return string.Empty;
                }

                return Locations.ContainsKey(DefaultTag)
                    ? Locations[DefaultTag]
                    : Locations[Locations.Keys.First()];
            }
            private set
            {
                if (Locations.ContainsKey(DefaultTag))
                {
                    Locations[DefaultTag] = value;
                }
                else
                {
                    Locations.Add(DefaultTag, value);
                }
            }
        }

        /// <summary>
        /// Gets the locations used by Icarus, which is a key-value pair.
        /// The key is the location-tag specified during initialise, and the
        /// value is the location for that tag.
        /// If you never specified a tag, then it will be the "default" tag.
        /// </summary>
        /// <value>
        /// The locations used by Icarus.
        /// </value>
        public IDictionary<string, string> Locations { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should create data
        /// stores/collections that is accessible by everyone.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is accessible by everyone; otherwise, <c>false</c>.
        /// </value>
        public bool IsAccessEveryone { get; set; }

        /// <summary>
        /// Gets a value indicating whether encryption is enabled, this is a master override.
        /// If this value is true, all collections will be encrypted; if false it's up to the
        /// user to determine if encrypted is enabled at a collection level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [encryption enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEncryptionEnabled { get; set; }

        #endregion

        #region Fields

        private IDictionary<string, IIcarusDataStore> _dataStores;

        #endregion

        #region Constructor

        private IcarusClient()
        {
            _dataStores = new Dictionary<string, IIcarusDataStore>();
            Locations = new Dictionary<string, string>();
            IsAccessEveryone = false;
            IsEncryptionEnabled = false;
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Initialises the Icarus instance. You can specify multiple paths by using
        /// the location tag parameter.
        /// </summary>
        /// <param name="icarusLocation">The location of where the Icarus data stores are held.</param>
        /// <param name="locationTag">The location tag, if left blank will be the default tag.</param>
        /// <returns>
        /// The IcarusClient for chaining
        /// </returns>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="DirectoryNotFoundException">If Icarus store cannot be found.</exception>
        public IIcarus Initialise(string icarusLocation, string locationTag = "")
        {
            var path = Path.GetFullPath(icarusLocation);
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(string.Format("Icarus location does not exist: {0}", icarusLocation));
            }

            if (string.IsNullOrEmpty(locationTag))
            {
                locationTag = DefaultTag;
            }

            if (Locations.ContainsKey(locationTag))
            {
                Locations[locationTag] = path;
            }
            else
            {
                Locations.Add(locationTag, path);
            }

            return this;
        }

        /// <summary>
        /// Gets the data store from Icarus, you can optionally speicify the location tag to use.
        /// If the location tag is left blank, the default tag will be used.
        /// </summary>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <param name="locationTag">The location tag, if left blank will be the default tag.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> the Icarus datastore is accessible by everyone.</param>
        /// <returns>
        /// The data store.
        /// </returns>
        public IIcarusDataStore GetDataStore(string dataStoreName, string locationTag = "", bool isAccessEveryone = false)
        {
            var isNew = false;
            return GetDataStore(dataStoreName, out isNew, locationTag: locationTag, isAccessEveryone: isAccessEveryone);
        }

        /// <summary>
        /// Gets the data store from Icarus, you can optionally speicify the location tag to use.
        /// If the location tag is left blank, the default tag will be used.
        /// The isNew out flag will be true if teh data store had to be created, false otherwise.
        /// </summary>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <param name="isNew">if set to <c>true</c> then the data store was created as a part of this call.</param>
        /// <param name="locationTag">The location tag, if left blank will be the default tag.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> the Icarus datastore is accessible by everyone.</param>
        /// <returns>
        /// The data store.
        /// </returns>
        public IIcarusDataStore GetDataStore(string dataStoreName, out bool isNew, string locationTag = "", bool isAccessEveryone = false)
        {
            isNew = false;

            if (!Locations.Any())
            {
                return default(IIcarusDataStore);
            }

            if (string.IsNullOrEmpty(locationTag))
            {
                locationTag = Locations.ContainsKey(DefaultTag)
                    ? DefaultTag
                    : Locations.Keys.First();
            }

            var storeTagKey = string.Format("{0}|{1}", locationTag, dataStoreName);

            if (!_dataStores.ContainsKey(storeTagKey))
            {
                var path = string.Empty;
                isNew = !IcarusDataStore.Exists(Locations[locationTag], dataStoreName, out path);

                _dataStores.Add(storeTagKey,
                    new IcarusDataStore(
                        Locations[locationTag],
                        dataStoreName,
                        (IsAccessEveryone || isAccessEveryone)));
            }

            return _dataStores[storeTagKey];
        }

        /// <summary>
        /// Clears this instances cache of data stores.
        /// </summary>
        public void Clear()
        {
            if (_dataStores == default(IDictionary<string, IIcarusDataStore>))
            {
                return;
            }

            _dataStores.Clear();
        }

        #endregion

    }

}

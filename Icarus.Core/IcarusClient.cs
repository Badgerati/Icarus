/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace Icarus.Core
{

    public class IcarusClient : IIcarus
    {

        #region Lazy Initialiser

        private static Lazy<IIcarus> _lazy = new Lazy<IIcarus>(() => new IcarusClient());
        public static IIcarus Instance
        {
            get { return _lazy.Value; }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the location of Icarus.
        /// </summary>
        /// <value>
        /// The location of Icarus.
        /// </value>
        public string IcarusLocation { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance should create data stores/collections as access for everyone.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is access everyone; otherwise, <c>false</c>.
        /// </value>
        public bool IsAccessEveryone { get; private set; }

        #endregion

        #region Fields

        private IDictionary<string, IIcarusDataStore> _dataStores;

        #endregion

        #region Constructor

        private IcarusClient()
        {
            _dataStores = new Dictionary<string, IIcarusDataStore>();
            IsAccessEveryone = false;
        }

        #endregion

        #region Public Helpers

        /// <summary>
        /// Initialises this Icarus instance.
        /// </summary>
        /// <param name="icarusLocation">The location of where the Icarus data stores are held.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> [Icarus data is accessible by everyone].</param>
        /// <returns>The IcarusClient for chaining</returns>
        /// <exception cref="DirectoryNotFoundException">If Icarus store cannot be found.</exception>
        public IIcarus Initialise(string icarusLocation, bool isAccessEveryone = false)
        {
            var path = Path.GetFullPath(icarusLocation);
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(string.Format("Icarus location does not exist: {0}", icarusLocation));
            }

            IcarusLocation = path;
            IsAccessEveryone = isAccessEveryone;

            return this;
        }

        /// <summary>
        /// Gets the DataStore from Icarus.
        /// </summary>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <returns>The DataStore.</returns>
        public IIcarusDataStore GetDataStore(string dataStoreName)
        {
            if (!_dataStores.ContainsKey(dataStoreName))
            {
                _dataStores.Add(dataStoreName, new IcarusDataStore(IcarusLocation, dataStoreName, IsAccessEveryone));
            }

            return _dataStores[dataStoreName];
        }

        #endregion

    }

}

/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System;
using System.Collections.Generic;

namespace Icarus.Core
{
    public interface IIcarus
    {

        /// <summary>
        /// Gets the default/first location of Icarus from the locations dictionary.
        /// </summary>
        /// <value>
        /// The default/first location of Icarus.
        /// </value>
        string Location { get; }

        /// <summary>
        /// Gets the locations used by Icarus, which is a key-value pair.
        /// The key is the location-tag specified during initialise, and the
        /// value is the location for that tag.
        /// If you never specified a tag, then it will be the "default" tag.
        /// </summary>
        /// <value>
        /// The locations used by Icarus.
        /// </value>
        IDictionary<string, string> Locations { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should create data
        /// stores/collections that is accessible by everyone.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is accessible by everyone; otherwise, <c>false</c>.
        /// </value>
        bool IsAccessEveryone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether encryption is enabled, this is a master override.
        /// If this value is true, all collections will be encrypted; if false it's up to the
        /// user to determine if encrypted is enabled at a collection level.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [encryption enabled]; otherwise, <c>false</c>.
        /// </value>
        bool IsEncryptionEnabled { get; set; }


        /// <summary>
        /// Initialises this Icarus instance.
        /// </summary>
        /// <param name="icarusLocation">The location of where the Icarus data stores are held.</param>
        /// <param name="locationTag">The location tag, if left blank will be the default tag.</param>
        /// <returns>
        /// The IcarusClient for chaining
        /// </returns>
        /// <exception cref="DirectoryNotFoundException">If Icarus store cannot be found.</exception>
        IIcarus Initialise(string icarusLocation, string locationTag = "");

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
        IIcarusDataStore GetDataStore(string dataStoreName, string locationTag = "", bool isAccessEveryone = false);

        /// <summary>
        /// Gets the data store from Icarus, you can optionally speicify the location tag to use.
        /// If the location tag is left blank, the default tag will be used.
        /// The isNew out flag will be true if the data store had to be created, false otherwise.
        /// </summary>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <param name="isNew">if set to <c>true</c> then the data store was created as a part of this call.</param>
        /// <param name="locationTag">The location tag, if left blank will be the default tag.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> the Icarus datastore is accessible by everyone.</param>
        /// <returns>
        /// The data store.
        /// </returns>
        IIcarusDataStore GetDataStore(string dataStoreName, out bool isNew, string locationTag = "", bool isAccessEveryone = false);

        /// <summary>
        /// Clears this instances cache of data stores.
        /// </summary>
        void Clear();

    }
}

﻿/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

namespace Icarus.Core
{
    public interface IIcarus
    {

        /// <summary>
        /// Gets the location of Icarus.
        /// </summary>
        /// <value>
        /// The location of Icarus.
        /// </value>
        string IcarusLocation { get; }

        /// <summary>
        /// Gets a value indicating whether this instance should create data stores/collections as access for everyone.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is access everyone; otherwise, <c>false</c>.
        /// </value>
        bool IsAccessEveryone { get; }


        /// <summary>
        /// Initialises this Icarus instance.
        /// </summary>
        /// <param name="icarusLocation">The location of where the Icarus data stores are held.</param>
        /// <param name="isAccessEveryone">if set to <c>true</c> [Icarus data is accessible by everyone].</param>
        /// <returns>The IcarusClient for chaining</returns>
        /// <exception cref="DirectoryNotFoundException">If Icarus store cannot be found.</exception>
        IIcarus Initialise(string icarusLocation, bool isAccessEveryone = false);

        /// <summary>
        /// Gets the DataStore from Icarus.
        /// </summary>
        /// <param name="dataStoreName">Name of the data store.</param>
        /// <returns>The DataStore.</returns>
        IIcarusDataStore GetDataStore(string dataStoreName);

    }
}

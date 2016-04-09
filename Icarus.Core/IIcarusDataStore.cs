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

        string DataStoreName { get; }
        string DataStoreLocation { get; }

        IIcarusCollection<T> GetCollection<T>(string collectionName) where T : IIcarusObject;

    }

}

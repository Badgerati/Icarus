/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

using System.Collections.Generic;

namespace Icarus.Core
{

    public interface IIcarusCollection { }

    public interface IIcarusCollection<T> : IIcarusCollection where T : IIcarusObject
    {
        
        string CollectionName { get; }
        string CollectionLocation { get; }
        string DataStoreLocation { get; }
        long NextPrimaryId { get; }
        bool CachingEnabled { get; set; }
        
        T Insert(T item, bool persist = true);
        IList<T> InsertMany(T[] items, bool persist = true);

        T Find(long id);
        T Find(string jsonPath);
        T Find(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal);
        IList<T> FindMany(long[] ids);
        IList<T> FindMany(string jsonPath);
        IList<T> FindMany(string fieldName, object value, IcarusEqualityFilter filter = IcarusEqualityFilter.Equal);

        T Remove(long id, bool persist = true);
        IList<T> RemoveMany(long[] ids, bool persist = true);

        T Update(T item, bool persist = true);
        IList<T> UpdateMany(T[] items, bool persist = true);

        void Persist();
        void ClearCache();
        void Refresh(bool persistFirst = true);

    }

}

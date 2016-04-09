/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

namespace Icarus.Core
{
    public interface IIcarus
    {

        string IcarusLocation { get; }

        IIcarus Initialise(string icarusLocation);
        IIcarusDataStore GetDataStore(string dataStoreName);

    }
}

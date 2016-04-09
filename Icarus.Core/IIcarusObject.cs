/*
Icarus is a simple JSON datastore.

Copyright (c) 2015, Matthew Kelly (Badgerati)
Company: Cadaeic Studios
License: MIT (see LICENSE for details)
 */

namespace Icarus.Core
{
    public interface IIcarusObject
    {

        /// <summary>
        /// Gets or sets the _id. DO NOT MANUALLY EDIT THIS VALUE.
        /// </summary>
        /// <value>
        /// The _id.
        /// </value>
        long _id { get; set; }

    }
}

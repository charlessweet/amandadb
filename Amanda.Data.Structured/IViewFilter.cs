using Amanda.Data.Structured;
using System.Collections.Generic;

namespace Amanda.Data.Structured
{
    public interface IViewFilter
    {
        /// <summary>
        /// This is exposed for dynamic option adding.  The intent is that 
        /// as a filter developer, this list is primed with options and 
        /// reasonable default values so that whoever is consuming this 
        /// can use it.
        /// </summary>
        /// <remarks>
        /// In implementations, you may wish to expose individual strongly-typed
        /// properties to map to view options.  It's important that you also
        /// map those properties into this collection, so that these options
        /// may be automatically displayed by some tool eventually.
        /// </remarks>
        ChangeOptions Options { get; }

        /// <summary>
        /// This method does the actual filtering, and is called by AmandaView.
        /// </summary>
        InstanceSet Apply(InstanceSet originalInstances);
    }
}
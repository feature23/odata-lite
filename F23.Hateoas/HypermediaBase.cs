using System.Collections.Generic;
using Newtonsoft.Json;

namespace F23.Hateoas
{
    /// <summary>
    /// A base class for including hypermedia (HATEOAS) links.
    /// </summary>
    public abstract class HypermediaBase
    {
        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public IList<HypermediaLink> Links { get; set; }
    }
}

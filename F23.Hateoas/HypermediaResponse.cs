using System.Collections.Generic;
using Newtonsoft.Json;

namespace F23.Hateoas
{
    public class HypermediaResponse
    {
        public HypermediaResponse(object content)
        {
            Content = content;
        }

        [JsonProperty("content", NullValueHandling = NullValueHandling.Include)]
        public object Content { get; }

        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public IList<HypermediaLink> Links { get; set; }

        public void Add(HypermediaLink link)
        {
            if (Links == null)
                Links = new List<HypermediaLink>();

            Links.Add(link);
        }
    }
}

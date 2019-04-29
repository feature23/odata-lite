using Newtonsoft.Json;

namespace F23.Hateoas
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HypermediaLink
    {
        public HypermediaLink()
        {
        }

        public HypermediaLink(string rel, string href)
            : this(rel, href, null)
        {
        }

        public HypermediaLink(string rel, string href, string method)
        {
            Rel = rel;
            Href = href;
            Method = method;
        }

        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}
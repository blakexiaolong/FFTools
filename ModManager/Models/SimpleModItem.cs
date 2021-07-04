using Newtonsoft.Json;

namespace ModManager.Models
{
    public class SimpleModItem
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Category")]
        public string Category { get; set; }

        [JsonProperty(PropertyName = "FullPath")]
        public string FullPath { get; set; }
    }

}

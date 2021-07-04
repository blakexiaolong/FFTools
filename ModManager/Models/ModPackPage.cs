using Newtonsoft.Json;

namespace ModManager.Models
{
    public class ModPackPage
    {
        [JsonProperty(PropertyName = "PageIndex")]
        public string PageIndex { get; set; }
        [JsonProperty(PropertyName = "ModGroups")]
        public ModGroup[] ModGroups { get; set; }
    }

}

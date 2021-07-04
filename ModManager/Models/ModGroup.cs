using Newtonsoft.Json;

namespace ModManager.Models
{
    public class ModGroup
    {
        [JsonProperty(PropertyName = "GroupName")]
        public string GroupName { get; set; }
        [JsonProperty(PropertyName = "SelectionType")]
        public string SelectionType { get; set; }
        [JsonProperty(PropertyName = "OptionList")]
        public Option[] OptionList { get; set; }
        public class Option
        {
            [JsonProperty(PropertyName = "Name")]
            public string Name { get; set; }
            [JsonProperty(PropertyName = "Description")]
            public string Description { get; set; }
            [JsonProperty(PropertyName = "ImagePath")]
            public string ImagePath { get; set; }
            [JsonProperty(PropertyName = "ModsJsons")]
            public SimpleModItem[] ModsJsons { get; set; }
            [JsonProperty(PropertyName = "GroupName")]
            public string GroupName { get; set; }
            [JsonProperty(PropertyName = "SelectionType")]
            public string SelectionType { get; set; }
            [JsonProperty(PropertyName = "IsChecked")]
            public string IsChecked { get; set; }
        }
    }
}

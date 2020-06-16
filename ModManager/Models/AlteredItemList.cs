using System.Collections.Generic;

namespace ModManager.Models
{
    public class AlteredItemList : Dictionary<string, List<string>>
    {
        public void Add(string itemName, string fileName)
        {
            if (!ContainsKey(itemName)) Add(itemName, new List<string>());
            if (!this[itemName].Contains(fileName)) this[itemName].Add(fileName);
        }
        public List<string> AlteredFiles(string itemName) => this[itemName];
    }
}

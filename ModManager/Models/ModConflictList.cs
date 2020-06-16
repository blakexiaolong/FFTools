using System.Collections.Generic;

namespace ModManager.Models
{
    public class ModConflictList : Dictionary<Mod, AlteredItemList>
    {
        public void Add(Mod mod, string itemName, string fileName)
        {
            if (!ContainsKey(mod)) Add(mod, new AlteredItemList());
            this[mod].Add(itemName, fileName);
        }
    }
}

using MyToolkit.WorkflowEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ModManager.Models
{
    public enum ModCategory
    {
        Unknown
    }
    public class Mod : INotifyPropertyChanged
    {
        private bool _isEnabled;
        private bool _hasConflict;
        private string _activeModConflicts;

        public string FullPath { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool HasConflict
        {
            get => _hasConflict;
            set
            {
                if (value != _hasConflict)
                {
                    _hasConflict = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string ActiveModConflicts
        {
            get => _activeModConflicts;
            set
            {
                if (value != _activeModConflicts)
                {
                    _activeModConflicts = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public AlteredItemList AlteredItemsList { get; set; }
        public ModConflictList ModConflicts { get; set; }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
        public ModCategory Category { get; set; }
        [JsonProperty(PropertyName = "Author")]
        public string Author { get; set; }
        [JsonProperty(PropertyName = "Version")]
        public string Version { get; set; }
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
        public BitmapImage Image { get; set; }
        public string TagsFromFolder { get; set; }
        [JsonProperty(PropertyName = "SimpleModsList")]
        public SimpleModItem[] SimpleModsList { get; set; }
        [JsonProperty(PropertyName = "ModPackPages")]
        public ModPackPage[] ModPackPages { get; set; }

        public class SimpleModItem
        {
            [JsonProperty(PropertyName = "Name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "Category")]
            public string Category { get; set; }

            [JsonProperty(PropertyName = "FullPath")]
            public string FullPath { get; set; }
        }
        public class ModPackPage
        {
            [JsonProperty(PropertyName = "PageIndex")]
            public string PageIndex { get; set; }
            [JsonProperty(PropertyName = "ModGroups")]
            public ModGroup[] ModGroups { get; set; }
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

        public Mod()
        {
            AlteredItemsList = new AlteredItemList();
            ModConflicts = new ModConflictList();
        }

        public void ScanForConflicts()
        {
            if (IsEnabled)
            {
                IEnumerable<string> enabledMods = ModConflicts.Where(x => x.Key.IsEnabled).Select(x => x.Key.Name);
                HasConflict = ModConflicts.Any(x => x.Key.IsEnabled);
                ActiveModConflicts = string.Join(", ", enabledMods);
            }
            else
            {
                HasConflict = false;
                ActiveModConflicts = "";
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public void SaveAlteredItemsList()
        {
            if (SimpleModsList != null)
            {
                IEnumerable<string> itemNames = SimpleModsList.Select(x => x.Name).Distinct();
                AddToAlteredItemsList(itemNames, SimpleModsList);
            }

            if (ModPackPages != null)
            {
                IEnumerable<SimpleModItem> modItems = ModPackPages.SelectMany(x => x.ModGroups).SelectMany(x => x.OptionList).SelectMany(x => x.ModsJsons);
                IEnumerable<string> itemNames = modItems.Select(x => x.Name).Distinct();
                AddToAlteredItemsList(itemNames, modItems);
            }
        }

        public class OutputMod
        {
            public string Name { get; set; }
            public ModCategory Category { get; set; }
            public bool IsEnabled { get; set; }
        }
        public OutputMod GetOutputMod() => new OutputMod
        {
            Name = Name,
            Category = Category,
            IsEnabled = IsEnabled
        };
        
        private void AddToAlteredItemsList(IEnumerable<string> itemNames, IEnumerable<SimpleModItem> modItems)
        {
            foreach (string itemName in itemNames)
                foreach (string fileName in modItems.Where(x => x.Name.Equals(itemName)).Select(x => x.FullPath))
                    AlteredItemsList.Add(itemName, fileName);
        }
    }
}

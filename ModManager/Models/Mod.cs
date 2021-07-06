using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ModManager.Models
{
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
        public List<string> TagsFromFolder { get; set; }
        public string DisplayFolders { get => string.Join(",", TagsFromFolder); }
        public string TopFolder { get => TagsFromFolder.FirstOrDefault(); }
        [JsonProperty(PropertyName = "SimpleModsList")]
        public SimpleModItem[] SimpleModsList { get; set; }
        [JsonProperty(PropertyName = "ModPackPages")]
        public ModPackPage[] ModPackPages { get; set; }

        
        public Mod()
        {
            AlteredItemsList = new AlteredItemList();
            ModConflicts = new ModConflictList();
        }

        public OutputMod GetOutputMod() => new OutputMod { Name = Name, Category = Category, IsEnabled = IsEnabled };

        public void ImportFromFile(string file)
        {
            switch (Path.GetExtension(file))
            {
                case ".ttmp":
                    using (ZipArchive archive = ZipFile.OpenRead(file))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.Name.EndsWith(".mpl"))
                            {
                                using (StreamReader reader = new StreamReader(entry.Open()))
                                {
                                    string[] folderTags = file.Remove(0, Properties.Settings.Default.ModImportPath.Length + 1).Split(Path.DirectorySeparatorChar);

                                    string modName = Path.GetFileNameWithoutExtension(file);
                                    string readFile = reader.ReadToEnd();
                                    string replacedFile = readFile.Replace("\n", "\n,").Trim(',');
                                    string imagePath = Directory.GetFiles(Path.GetDirectoryName(file)).Where(x => Path.GetFileNameWithoutExtension(file) == "0").FirstOrDefault();

                                    try
                                    {
                                        Name = modName;
                                        FullPath = file;
                                        TagsFromFolder = folderTags.Take(folderTags.Length - 1).ToList();
                                        SimpleModsList = JsonConvert.DeserializeObject<Mod>(replacedFile).SimpleModsList;

                                        if (imagePath != default)
                                        {
                                            Image = new BitmapImage(new Uri(imagePath));
                                        }
                                    }
                                    catch
                                    {
                                        Name = modName;
                                        FullPath = file;
                                        TagsFromFolder = folderTags.Take(folderTags.Length - 1).ToList();
                                        SimpleModsList = JsonConvert.DeserializeObject<SimpleModItem[]>($"[{replacedFile}]");

                                        if (imagePath != default)
                                        {
                                            Image = new BitmapImage(new Uri(imagePath));
                                        }
                                    }
                                    SaveAlteredItemsList();
                                }
                            }
                        }
                    }
                    break;
                case ".ttmp2":
                    using (ZipArchive archive = ZipFile.OpenRead(file))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.Name.EndsWith(".mpl"))
                            {
                                using (StreamReader reader = new StreamReader(entry.Open()))
                                {
                                    Mod mod = JsonConvert.DeserializeObject<Mod>(reader.ReadToEnd());
                                    Name = mod.Name;
                                    Author = mod.Author;
                                    Version = mod.Version;
                                    Description = mod.Description;
                                    SimpleModsList = mod.SimpleModsList;
                                    ModPackPages = mod.ModPackPages;

                                    string[] folderTags = file.Remove(0, Properties.Settings.Default.ModImportPath.Length + 1).Split(Path.DirectorySeparatorChar);

                                    if (Name != "") Name = Path.GetFileNameWithoutExtension(file);
                                    FullPath = file;
                                    TagsFromFolder = folderTags.Take(folderTags.Length - 1).ToList();
                                    SaveAlteredItemsList();
                                }
                            }
                        }
                    }
                    break;
            }
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

        private void AddToAlteredItemsList(IEnumerable<string> itemNames, IEnumerable<SimpleModItem> modItems)
        {
            foreach (string itemName in itemNames)
                foreach (string fileName in modItems.Where(x => x.Name.Equals(itemName)).Select(x => x.FullPath))
                    AlteredItemsList.Add(itemName, fileName);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}

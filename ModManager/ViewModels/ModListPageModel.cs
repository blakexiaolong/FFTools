using MyToolkit.Mvvm;
using System.Collections.Generic;
using ModManager.Models;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using static ModManager.Models.Mod;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ModManager.ViewModels
{
    public class ModListPageModel : ViewModelBase
    {
        private string _status;
        private int _progress;
        private int _progressMax;
        private bool _enableForms;
        private ObservableCollection<Mod> _mods = new ObservableCollection<Mod>();

        public string Status { get => _status; set => Set(ref _status, value); }
        public int Progress { get => _progress; set => Set(ref _progress, value); }
        public int ProgressMax { get => _progressMax; set => Set(ref _progressMax, value); }
        public bool EnableForms { get => _enableForms; set => Set(ref _enableForms, value); }

        public ModListPageModel()
        {
            if (Directory.Exists(Properties.Settings.Default.ModImportPath)) ImportMods();
            else EnableForms = true;
        }

        public ObservableCollection<Mod> Mods
        {
            get => _mods;
            set => Set(ref _mods, value);
        }
        public Dictionary<string, Dictionary<string, List<Mod>>> CategoryTreeMods
        {
            get
            {
                Dictionary<string, Dictionary<string, List<Mod>>> tree = new Dictionary<string, Dictionary<string, List<Mod>>>();

                IEnumerable<string> categories = Mods.Where(z => z.IsEnabled && z.SimpleModsList != null).SelectMany(x => x.SimpleModsList.Select(y => y.Category)).Distinct();
                foreach (string category in categories)
                {
                    IEnumerable<string> itemsInCategory = Mods.Where(z => z.IsEnabled && z.SimpleModsList != null).SelectMany(x => x.SimpleModsList.Where(y => y.Category == category).Select(y => y.Name)).Distinct();
                    foreach (string item in itemsInCategory)
                    {
                        IEnumerable<Mod> mods = Mods.Where(x => x.IsEnabled && x.SimpleModsList != null && x.SimpleModsList.Any(y => y.Name == item));
                        if (!mods.Any()) continue;

                        if (!tree.ContainsKey(category)) tree.Add(category, new Dictionary<string, List<Mod>>());
                        if (!tree[category].ContainsKey(item)) tree[category].Add(item, new List<Mod>());
                        tree[category][item].AddRange(mods);
                    }
                }
                return tree;
            }
        }
        public void UpdateSettings()
        {
            bool settingChanged = false;
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                settingChanged = true;
            }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ModImportPath))
            {
                Properties.Settings.Default.ModImportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TexTools", "ModPacks");
                settingChanged = true;
            }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ModExportPath))
            {
                Properties.Settings.Default.ModExportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TexTools", "ModOrganizerOutput");
                settingChanged = true;
            }
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.PresetFilePath))
            {
                Properties.Settings.Default.PresetFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TexTools", "ModOrganizerPreset.json");
                settingChanged = true;
            }

            if (settingChanged)
            {
                Properties.Settings.Default.Save();
            }

            Status = "Settings Updated";
        }
        public void LoadPresets()
        {
            EnableForms = false;
            Status = "Loading Presets";
            int foundModsCount = 0;
            string[] enabledModNames = JsonConvert.DeserializeObject<string[]>(File.ReadAllText(Properties.Settings.Default.PresetFilePath));
            if (enabledModNames != null)
            {
                Progress = 0;
                ProgressMax = enabledModNames.Length;
                foreach (string name in enabledModNames)
                {
                    Mod foundMod = _mods.FirstOrDefault(x => x.Name.Equals(name));
                    if (foundMod == default) continue;
                    else
                    {
                        foundMod.IsEnabled = true;
                        Mods.Remove(foundMod);
                        Mods.Insert(foundModsCount, foundMod);
                    }
                    foundModsCount++;
                    Progress++;
                }
                ScanForConflicts();
            }
            Status = "Presets Loaded";
            EnableForms = true;
        }
        public void SavePresets()
        {
            EnableForms = false;
            Status = "Saving Presets";
            Progress = 0;
            ProgressMax = 1;
            string presetData = JsonConvert.SerializeObject(_mods.Where(x => x.IsEnabled).Select(x => x.Name));
            Directory.CreateDirectory(Path.GetDirectoryName(Properties.Settings.Default.PresetFilePath));
            File.WriteAllText(Properties.Settings.Default.PresetFilePath, presetData);
            Status = "Presets Saved";
            Progress = 1;
            EnableForms = true;
        }

        public void EnableAllMods()
        {
            EnableForms = false;
            foreach (Mod mod in Mods) mod.IsEnabled = true;
            ScanForConflicts();
            EnableForms = true;
        }
        public void DisableAllMods()
        {
            EnableForms = false;
            foreach (Mod mod in Mods) mod.IsEnabled = false;
            ScanForConflicts();
            EnableForms = true;
        }

        public void ImportMods()
        {
            EnableForms = false;
            Status = "Importing Mods";

            Mods.Clear();
            LoadModsFromFile(Properties.Settings.Default.ModImportPath);
            GetModConflicts();

            Status = $"{Mods.Count()} Mods Imported";
            EnableForms = true;
        }
        public void LoadModsFromFile(string filePath)
        {
            Progress = 0;
            IEnumerable<string> ttmp2files = Directory.EnumerateFiles(filePath, "*.ttmp2", SearchOption.AllDirectories);
            IEnumerable<string> ttmpfiles = Directory.EnumerateFiles(filePath, "*.ttmp", SearchOption.AllDirectories);
            ProgressMax = ttmp2files.Count() + ttmpfiles.Count();

            foreach (string file in Directory.EnumerateFiles(filePath, "*.ttmp2", SearchOption.AllDirectories))
            {
                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.EndsWith(".mpl"))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                Mod mod = JsonConvert.DeserializeObject<Mod>(reader.ReadToEnd());
                                string fileName = file.Split(Path.DirectorySeparatorChar).Last();
                                int fileSplits = file.Split('.').Count();
                                mod.Name = string.Join(".", fileName.Split('.').Take(fileSplits - 1));
                                mod.FullPath = file;
                                string[] folderTags = file.Split(Path.DirectorySeparatorChar);
                                mod.TagsFromFolder = string.Join(", ", folderTags.Take(folderTags.Length - 1).Except(filePath.Split(Path.DirectorySeparatorChar)));
                                mod.SaveAlteredItemsList();
                                Mods.Add(mod);
                                Progress++;
                            }
                        }
                    }
                }
            }
            foreach (string file in Directory.EnumerateFiles(filePath, "*.ttmp", SearchOption.AllDirectories))
            {
                using (ZipArchive archive = ZipFile.OpenRead(file))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.Name.EndsWith(".mpl"))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string fileName = file.Split(Path.DirectorySeparatorChar).Last();
                                int fileSplits = file.Split('.').Count();
                                string[] folderTags = file.Split(Path.DirectorySeparatorChar);
                                Mod mod = new Mod
                                {
                                    Name = string.Join(".", fileName.Split('.').Take(fileSplits - 1)),
                                    FullPath = file,
                                    TagsFromFolder = string.Join(", ", folderTags.Take(folderTags.Length - 1).Except(filePath.Split(Path.DirectorySeparatorChar))),
                                    SimpleModsList = JsonConvert.DeserializeObject<SimpleModItem[]>($"[{reader.ReadToEnd().Replace("\n", "\n,")}]"),
                                };
                                mod.SaveAlteredItemsList();
                                Mods.Add(mod);
                                Progress++;
                            }
                        }
                    }
                }
            }
        }
        public void GetModConflicts()
        {
            AlteredItemList filesWithConflicts = new AlteredItemList();
            AlteringModsList alteredDataList = new AlteringModsList();

            foreach (Mod mod in Mods)
            {
                foreach (KeyValuePair<string, List<string>> alteredItem in mod.AlteredItemsList)
                {
                    foreach (string alteredFile in alteredItem.Value)
                    {
                        alteredDataList.Add(alteredItem.Key, alteredFile, mod);
                        IEnumerable<Mod> conflictingMods = alteredDataList[alteredItem.Key][alteredFile].FindAll(x => x != mod);
                        foreach (Mod conflictingMod in conflictingMods)
                        {
                            conflictingMod.ModConflicts.Add(mod, alteredItem.Key, alteredFile);
                            mod.ModConflicts.Add(conflictingMod, alteredItem.Key, alteredFile);
                        }
                    }
                }
            }
        }
        public void ScanForConflicts()
        {
            EnableForms = false;
            foreach (Mod mod in Mods) mod.ScanForConflicts();
            EnableForms = true;
        }

        public async void ExportMods()
        {
            EnableForms = false;
            Status = "Exporting Mods";
            string folderPath = Path.Combine(Properties.Settings.Default.ModExportPath, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            Directory.CreateDirectory(folderPath);
            List<Mod> enabledMods = _mods.Where(x => x.IsEnabled).ToList();
            Progress = 0;
            ProgressMax = enabledMods.Count();
            foreach (Mod enabledMod in enabledMods)
            {
                File.Copy(enabledMod.FullPath, Path.Combine(folderPath, $"{Progress + 1:D3}. {enabledMod.Name}"));
                Progress++;
                //TODO: maybe remove this? idk it has a nice effect
                await Task.Delay(10);
            }
            Status = "Mods Exported";
            EnableForms = true;
        }

        public class AlteringModsList : Dictionary<string, Dictionary<string, List<Mod>>>
        {
            public void Add(string itemName, string fileName, Mod mod)
            {
                if (!ContainsKey(itemName)) this[itemName] = new Dictionary<string, List<Mod>>();
                if (!this[itemName].ContainsKey(fileName)) this[itemName][fileName] = new List<Mod>();
                if (!this[itemName][fileName].Contains(mod)) this[itemName][fileName].Add(mod);
            }
        }
    }
}

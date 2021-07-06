using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModManager.ViewModels;
using ModManager.Views.Dialogs;
using ModManager.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;

namespace ModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Model.UpdateSettings();
            Model.ImportMods();
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvOrganizer.ItemsSource);
            PropertyGroupDescription propertyGroupDescription = new PropertyGroupDescription("TopFolder");
            view.GroupDescriptions.Add(propertyGroupDescription);
            view.Filter = UserFilter;
        }

        public ModListPageModel Model
        {
            get => (ModListPageModel)Resources["ViewModel"];
        }

        private void ImportModsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Properties.Settings.Default.ModImportPath))
            {
                MessageBox.Show($"Files not found at {Properties.Settings.Default.ModImportPath}. Please check your settings.",
                    "Files Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
            else Model.ImportMods();
        }

        private void IsEnabledCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ((Mod)((CheckBox)e.OriginalSource).DataContext).IsEnabled = ((CheckBox)e.OriginalSource).IsChecked ?? false;
            Model.ScanForConflicts();
            var m = Model.Mods.Where(x => x.HasConflict).ToList();
        }
        private void SavePresetButton_Click(object sender, RoutedEventArgs e)
        {
            bool writeFiles = true;
            if (File.Exists(Properties.Settings.Default.PresetFilePath))
            {
                writeFiles = MessageBox.Show("Preset files already exist, do you really want to overrwite it?",
                    "Really overwrite preset files?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No)
                    == MessageBoxResult.Yes;
            }
            if (writeFiles) Model.SavePresets();
        }
        private void LoadPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Properties.Settings.Default.PresetFilePath))
            {
                MessageBox.Show($"File not found at {Properties.Settings.Default.PresetFilePath}. Please check your settings.",
                    "File Not Found", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
            else Model.LoadPresets();
        }
        private void EditPathsButton_Click(object sender, RoutedEventArgs e)
        {
            new PathSettings().Show();
        }
        private void ExportModsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Model.Mods.Any(x => x.IsEnabled))
            {
                MessageBox.Show("No mods are currently selected. Please select some before exporting.",
                    "No Mods Selected", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
            }
            else if (Model.Mods.Any(x => x.HasConflict) && MessageBoxResult.Yes != MessageBox.Show(
                "There are conflicting mods selected. Are you sure about your selection?",
                "Conflicting Mods Selected", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No)) { }
            else Model.ExportMods();
        }

        private void EnableAllButton_Click(object sender, RoutedEventArgs e)
        {
            Model.EnableAllMods();
        }

        private void DisableAllButton_Click(object sender, RoutedEventArgs e)
        {
            Model.DisableAllMods();
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            new ModImporter().Show();
            Model.GetModConflicts();
        }

        private void CategoryView_GotFocus(object sender, RoutedEventArgs e)
        {
            CategoryTreeView.Items.Clear();
            foreach (var category in Model.CategoryTreeMods)
            {
                TreeViewItem categoryItem = new TreeViewItem { Header = category.Key };
                foreach (var item in category.Value)
                {
                    TreeViewItem categorySubItem = new TreeViewItem { Header = item.Key };
                    foreach (var mod in item.Value)
                    {
                        categorySubItem.Items.Insert(GetInsertIndex(categorySubItem.Items, mod.Name), new TreeViewItem { Header = mod.Name });
                    }
                    categoryItem.Items.Insert(GetInsertIndex(categoryItem.Items, item.Key), categorySubItem);
                }
                CategoryTreeView.Items.Insert(GetInsertIndex(CategoryTreeView.Items, category.Key), categoryItem);
            }
        }

        private int GetInsertIndex(ItemCollection collection, string key)
        {
            int index = collection.Count;
            for (int i = 0; i < collection.Count; i++)
            {
                if (string.Compare((string)(collection[i] as TreeViewItem).Header, key) < 0) continue;
                index = i;
                break;
            }
            return index;
        }

        private void FilterBox_TextChanged(object sender, System.Windows.Input.KeyEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lvOrganizer.ItemsSource).Refresh();
        }
        private void OrganizerViewItem_Selected(object sender, RoutedEventArgs e)
        {
            Mod selectedMod = (Mod)((ListViewItem)e.OriginalSource).DataContext;
            IEnumerable<Mod> conflictingMods = selectedMod.ModConflicts.Select(x => x.Key);

            SolidColorBrush enabledBrush = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
            SolidColorBrush disabledBrush = new SolidColorBrush(Color.FromArgb(100, 0, 0, 255));
            foreach (Mod conflictingMod in conflictingMods)
            {
                ListBoxItem listBoxItem = lvOrganizer.ItemContainerGenerator.ContainerFromItem(conflictingMod) as ListBoxItem;
                if (listBoxItem == default) continue;
                listBoxItem.Background = conflictingMod.IsEnabled ? enabledBrush : disabledBrush;
            }
        }
        private void OrganizerViewItem_Unselected(object sender, RoutedEventArgs e)
        {
            foreach(Mod mod in Model.Mods)
            {
                ListBoxItem listBoxItem = lvOrganizer.ItemContainerGenerator.ContainerFromItem(mod) as ListBoxItem;
                if (listBoxItem == default) continue;
                listBoxItem.Background = Brushes.Transparent;
            }
        }
        private void OrganizerViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Mod selectedMod = ((ListViewItem)sender).DataContext as Mod;

            DetailedModView detailedModView = new DetailedModView();
            detailedModView.Title = selectedMod.Name;
            detailedModView.TitleBlock.Text = $"{selectedMod.Name} by {selectedMod.Author ?? "?"}";
            detailedModView.SubtitleBlock.Text = $"{selectedMod.Category} Mod ({string.Join(",", selectedMod.DisplayFolders)})";
            detailedModView.DescriptionBlock.Text = selectedMod.Description;
            foreach (var alteredItem in selectedMod.AlteredItemsList)
            {
                TreeViewItem modItem = new TreeViewItem { Header = alteredItem.Key };
                foreach (var subitem in alteredItem.Value)
                {
                    modItem.Items.Add(new TreeViewItem { Header = subitem });
                }
                detailedModView.AlteredItemsTree.Items.Add(modItem);
            }
            detailedModView.FlipView.ItemsSource = new[] { selectedMod.Image };
            detailedModView.FlipView.SelectedIndex = 0;
            detailedModView.Show();
        }

        private bool UserFilter(object item)
        {
            string fText = FilterBox.Text;
            if (string.IsNullOrEmpty(fText))
                return true;

            Mod mod = item as Mod;
            IEnumerable<string> compareFields = new List<string>
            {
                mod.Name ?? "",
                mod.Author ?? "",
                mod.Category.ToString() ?? "",
                mod.Description ?? "",
                mod.DisplayFolders ?? ""
            }.Concat(mod.AlteredItemsList.Select(x => x.Key));
            return compareFields.Any(x => x.IndexOf(fText, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}

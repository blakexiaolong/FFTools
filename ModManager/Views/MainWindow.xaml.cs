using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ModManager.ViewModels;
using ModManager.Views.Dialogs;
using ModManager.Models;

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
            Model.SavePresets();
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
                "Conflicting Mods Selected", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No))
            {

            }
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

        private void CategoryView_GotFocus(object sender, RoutedEventArgs e)
        {
            CategoryTreeView.Items.Clear();
            foreach (var category in Model.CategoryTreeMods)
            {
                TreeViewItem categoryItem = new TreeViewItem { Header = category.Key };
                foreach (var item in category.Value)
                {
                    TreeViewItem itemItem = new TreeViewItem { Header = item.Key };
                    foreach (var mod in item.Value)
                    {
                        itemItem.Items.Add(new TreeViewItem { Header = mod.Name });
                    }
                    categoryItem.Items.Add(itemItem);
                }
                CategoryTreeView.Items.Add(categoryItem);
            }
        }
    }
}

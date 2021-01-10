using ModManager.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ModManager.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for ModImporter.xaml
    /// </summary>
    public partial class ModImporter : Window
    {
        private ModImporterViewModel _viewModel;
        public ModImporter()
        {
            _viewModel = new ModImporterViewModel();
            InitializeComponent();
            FolderSelector.ItemsSource = _viewModel.Directories;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.GetModData(UrlBox.Text);

            TitleBox.Text = $"{_viewModel.ModData.Name} by {_viewModel.ModData.Author}";
            DescriptionBox.Text = _viewModel.ModData.Description;
            FilesBox.Text = string.Join("\n", _viewModel.ModData.Files);
            ImageBox.Source = new BitmapImage(new Uri(_viewModel.ModData.Image));
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ModImport(FolderSelector.SelectedIndex);
            //new NewModSelector(newMods[i], 1, 1).ShowDialog();
            //if (newMods[i].IsEnabled) GetModConflicts();
        }
    }
}

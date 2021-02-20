using ModManager.Models;
using ModManager.ViewModels;
using System;
using System.Collections.Generic;
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
        private const string PLACEHOLDER_TEXT = "Enter XMA URL Here...";

        public ModImporter()
        {
            _viewModel = new ModImporterViewModel();
            InitializeComponent();
            UrlBox.Text = PLACEHOLDER_TEXT;
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
            List<Mod> mods = _viewModel.ModImport(FolderSelector.SelectedIndex);
            new NewModSelector(mods, () => { }).ShowDialog();
        }

        private void UrlBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UrlBox.Text == PLACEHOLDER_TEXT) UrlBox.Text = "";
        }

        private void UrlBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UrlBox.Text)) UrlBox.Text = PLACEHOLDER_TEXT;
        }
    }
}

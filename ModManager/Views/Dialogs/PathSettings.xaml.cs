namespace ModManager.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for PathSettings.xaml
    /// </summary>
    public partial class PathSettings : System.Windows.Window
    {

        public PathSettings()
        {
            InitializeComponent();
            ModImportPathBox.Text = Properties.Settings.Default.ModImportPath;
            ModExportPathBox.Text = Properties.Settings.Default.ModExportPath;
            PresetFileLocationBox.Text = Properties.Settings.Default.PresetFilePath;
        }

        private void BrowseModImportButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetLocation(ModImportPathBox);
        }
        private void BrowseModExportButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetLocation(ModExportPathBox);
        }
        private void BrowsePresetFileButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetLocation(PresetFileLocationBox, "ModOrganizerPreset.json");
        }
        private void SetLocation(System.Windows.Controls.TextBox textBox, string fileName = null)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                switch (dialog.ShowDialog())
                {
                    case System.Windows.Forms.DialogResult.OK:
                        if (string.IsNullOrEmpty(fileName))
                        {
                            textBox.Text = dialog.SelectedPath;
                            textBox.ToolTip = dialog.SelectedPath;
                        }
                        else
                        {
                            textBox.Text = System.IO.Path.Combine(dialog.SelectedPath, fileName);
                            textBox.ToolTip = System.IO.Path.Combine(dialog.SelectedPath, fileName);
                        }
                        break;
                    case System.Windows.Forms.DialogResult.Cancel:
                        break;
                    default:
                        textBox.Text = null;
                        textBox.ToolTip = null;
                        break;
                }
            }
        }

        private void CancelSettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
        private void SaveSettingsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Properties.Settings.Default.ModImportPath = ModImportPathBox.Text;
            Properties.Settings.Default.ModExportPath = ModExportPathBox.Text;
            Properties.Settings.Default.PresetFilePath = PresetFileLocationBox.Text;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModManager.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for NewModSelector.xaml
    /// </summary>
    public partial class NewModSelector : Window
    {
        private Models.Mod _mod;
        public NewModSelector(Models.Mod mod, int current, int count)
        {
            InitializeComponent();
            _mod = mod;
            ModTitle.Text = $"{mod.Name ?? "NO MOD NAME"} v{mod.Version ?? "???"} by {mod.Author ?? "???"}";
            ModDescription.Text = mod.Description ?? "???";
            ModCategory.Text = mod.Category.ToString() ?? "Unknown";
            if (mod.Image != default)
            {
                ImageBox.Source = mod.Image;
            }

            foreach (var conflict in mod.ModConflicts)
            {
                ModConflicts.Items.Add($"{conflict.Key.Name} ({string.Join(",", conflict.Value.Keys)})");
            }
            Counter.Text = $"{current} of {count}";
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            _mod.IsEnabled = true;
            _mod.ScanForConflicts();
            Close();
        }

        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            _mod.IsEnabled = false;
            Close();
        }
    }
}

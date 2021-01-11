using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        private int _current;
        private Action _enabledCallback;

        AutoResetEvent _buttonPressedEvent = new AutoResetEvent(false);

        public NewModSelector(List<Models.Mod> mods, Action enabledCallback)
        {
            InitializeComponent();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(SelectModWorker);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler((object sender, RunWorkerCompletedEventArgs e) => Close());
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync(new SelectModArguments()
            {
                Context = SynchronizationContext.Current,
                Mods = mods,
                EnabledCallback = enabledCallback,
                ButtonPressedEvent = _buttonPressedEvent
            });
        }

        private class SelectModArguments
        {
            public SynchronizationContext Context { get; set; }
            public List<Models.Mod> Mods { get; set; }
            public Action EnabledCallback { get; set; }
            public AutoResetEvent ButtonPressedEvent { get; set; }
        }

        private void SelectModWorker(object sender, DoWorkEventArgs e)
        {
            var args = (SelectModArguments)e.Argument;

            _current = 1;
            _enabledCallback = args.EnabledCallback;

            while (_current <= args.Mods.Count)
            {
                _mod = args.Mods[_current - 1];

                args.Context.Send(delegate
                {
                    if (_current == 1) BackButton.IsEnabled = false;
                    else BackButton.IsEnabled = true;

                    ModTitle.Text = $"{_mod.Name ?? "NO MOD NAME"} v{_mod.Version ?? "???"} by {_mod.Author ?? "???"}";
                    ModDescription.Text = _mod.Description ?? "???";
                    ModCategory.Text = _mod.Category.ToString() ?? "Unknown";
                    if (_mod.Image != default)
                    {
                        ImageBox.Source = _mod.Image;
                    }

                    ModConflicts.Items.Clear();
                    foreach (var conflict in _mod.ModConflicts)
                    {
                        ModConflicts.Items.Add($"{conflict.Key.Name} ({string.Join(",", conflict.Value.Keys)})");
                    }
                    Counter.Text = $"{_current} of {args.Mods.Count}";
                }, null);

                args.ButtonPressedEvent.WaitOne();
            }
        }

        private void EnableButton_Click(object sender, RoutedEventArgs e)
        {
            _mod.IsEnabled = true;
            _mod.ScanForConflicts();
            _current++;
            _enabledCallback();
            _buttonPressedEvent.Set();
        }

        private void DisableButton_Click(object sender, RoutedEventArgs e)
        {
            _mod.IsEnabled = false;
            _current++;
            _buttonPressedEvent.Set();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _current--;
            _buttonPressedEvent.Set();
        }
    }
}

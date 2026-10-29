using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinArma.Models;
using WinArma.Services;

namespace WinArma.Views
{
    public class SnippetRow
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public partial class SettingsWindow : Window
    {
        private readonly ObservableCollection<SnippetRow> _snippetRows = new();

        public SettingsWindow()
        {
            InitializeComponent();
            SnippetList.ItemsSource = _snippetRows;
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = System.IO.Path.Combine(baseDir, "app.ico");
                if (System.IO.File.Exists(icoPath))
                {
                    Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(icoPath));
                }
            }
            catch { }

            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            var s = SettingsService.Instance.Current;

            // Hotkey
            SelectComboByTag(HotkeyCombo, s.HotkeyModifier);

            // Startup
            StartupCheckBox.IsChecked = s.RunAtStartup;

            // Search Engine
            SelectComboByContent(SearchEngineCombo, s.DefaultSearchEngine);

            // Modules
            AppSearchCheckBox.IsChecked = s.EnableAppSearch;
            FileSearchCheckBox.IsChecked = s.EnableFileSearch;
            WebSearchCheckBox.IsChecked = s.EnableWebSearch;
            CalcCheckBox.IsChecked = s.EnableCalculator;
            SysCmdCheckBox.IsChecked = s.EnableSystemCommands;

            // Advanced Features
            ClipboardCheckBox.IsChecked = s.EnableClipboardHistory;
            QuickLookCheckBox.IsChecked = s.EnableQuickLook;
            SnippetsCheckBox.IsChecked = s.EnableSnippets;
            LoadSnippetRows(s.Snippets);

            // Max Results
            SelectComboByTag(MaxResultsCombo, s.MaxResultsCount.ToString());

            // Theme
            SelectComboByTag(ThemeCombo, s.Theme);
        }

        private void LoadSnippetRows(IDictionary<string, string>? snippets)
        {
            _snippetRows.Clear();
            HideSnippetError();
            if (snippets == null) return;

            foreach (var kv in snippets)
            {
                _snippetRows.Add(new SnippetRow { Key = kv.Key, Value = kv.Value });
            }
        }

        private void AddSnippet_Click(object sender, RoutedEventArgs e)
        {
            var row = new SnippetRow { Key = "!" };
            _snippetRows.Add(row);
            HideSnippetError();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (SnippetList.ItemContainerGenerator.ContainerFromItem(row) is FrameworkElement container)
                {
                    container.BringIntoView();
                    var keyBox = FindFirstTextBox(container);
                    if (keyBox != null)
                    {
                        keyBox.Focus();
                        keyBox.CaretIndex = keyBox.Text.Length;
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void DeleteSnippet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: SnippetRow row })
            {
                _snippetRows.Remove(row);
                HideSnippetError();
            }
        }

        private static System.Windows.Controls.TextBox? FindFirstTextBox(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is System.Windows.Controls.TextBox tb) return tb;
                var nested = FindFirstTextBox(child);
                if (nested != null) return nested;
            }
            return null;
        }

        private void ShowSnippetError(string message)
        {
            SnippetErrorText.Text = message;
            SnippetErrorText.Visibility = Visibility.Visible;
            SnippetErrorText.BringIntoView();
        }

        private void HideSnippetError()
        {
            SnippetErrorText.Visibility = Visibility.Collapsed;
        }

        // Rows -> dictionary. Returns null (and shows an inline error) when a row is invalid.
        private Dictionary<string, string>? BuildSnippets()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in _snippetRows)
            {
                var key = (row.Key ?? string.Empty).Trim();
                var value = row.Value ?? string.Empty;

                // Untouched new row ("!" and nothing else): drop silently
                if ((key.Length == 0 || key == "!") && value.Length == 0) continue;

                if (key.Length == 0 || key == "!")
                {
                    ShowSnippetError("Bir kısayolun adı boş olamaz (örn. !mail).");
                    return null;
                }

                if (!key.StartsWith("!")) key = "!" + key;

                if (key.Any(char.IsWhiteSpace))
                {
                    ShowSnippetError($"\"{key}\" boşluk içeremez.");
                    return null;
                }

                if (value.Length == 0)
                {
                    ShowSnippetError($"\"{key}\" için içerik boş.");
                    return null;
                }

                if (!result.TryAdd(key, value))
                {
                    ShowSnippetError($"\"{key}\" birden fazla kez tanımlı.");
                    return null;
                }
            }

            return result;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            var def = new AppSettings();
            SelectComboByTag(HotkeyCombo, def.HotkeyModifier);
            StartupCheckBox.IsChecked = false;
            SelectComboByContent(SearchEngineCombo, def.DefaultSearchEngine);
            AppSearchCheckBox.IsChecked = def.EnableAppSearch;
            FileSearchCheckBox.IsChecked = def.EnableFileSearch;
            WebSearchCheckBox.IsChecked = def.EnableWebSearch;
            CalcCheckBox.IsChecked = def.EnableCalculator;
            SysCmdCheckBox.IsChecked = def.EnableSystemCommands;
            ClipboardCheckBox.IsChecked = def.EnableClipboardHistory;
            QuickLookCheckBox.IsChecked = def.EnableQuickLook;
            SnippetsCheckBox.IsChecked = def.EnableSnippets;
            LoadSnippetRows(def.Snippets);
            SelectComboByTag(MaxResultsCombo, def.MaxResultsCount.ToString());
            SelectComboByTag(ThemeCombo, def.Theme);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var snippets = BuildSnippets();
            if (snippets == null) return;

            var current = SettingsService.Instance.Current;
            var s = new AppSettings
            {
                HotkeyModifier = GetSelectedTag(HotkeyCombo, "Alt"),
                HotkeyKey = current.HotkeyKey,
                WindowOpacity = current.WindowOpacity,
                RunAtStartup = StartupCheckBox.IsChecked == true,
                DefaultSearchEngine = GetSelectedContent(SearchEngineCombo, "Google"),
                EnableAppSearch = AppSearchCheckBox.IsChecked == true,
                EnableFileSearch = FileSearchCheckBox.IsChecked == true,
                EnableWebSearch = WebSearchCheckBox.IsChecked == true,
                EnableCalculator = CalcCheckBox.IsChecked == true,
                EnableSystemCommands = SysCmdCheckBox.IsChecked == true,
                EnableClipboardHistory = ClipboardCheckBox.IsChecked == true,
                EnableQuickLook = QuickLookCheckBox.IsChecked == true,
                EnableSnippets = SnippetsCheckBox.IsChecked == true,
                Snippets = snippets,
                MaxResultsCount = int.TryParse(GetSelectedTag(MaxResultsCombo, "8"), out int max) ? max : 8,
                Theme = GetSelectedTag(ThemeCombo, "Dark")
            };

            SettingsService.Instance.SaveSettings(s);
            Close();
        }

        private void SelectComboByTag(System.Windows.Controls.ComboBox combo, string tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private void SelectComboByContent(System.Windows.Controls.ComboBox combo, string content)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (string.Equals(item.Content?.ToString(), content, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private string GetSelectedTag(System.Windows.Controls.ComboBox combo, string fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                return item.Tag.ToString() ?? fallback;
            }
            return fallback;
        }

        private string GetSelectedContent(System.Windows.Controls.ComboBox combo, string fallback)
        {
            if (combo.SelectedItem is ComboBoxItem item && item.Content != null)
            {
                return item.Content.ToString() ?? fallback;
            }
            return fallback;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinArma.Models;
using WinArma.Services;

namespace WinArma.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
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

            // Max Results
            SelectComboByTag(MaxResultsCombo, s.MaxResultsCount.ToString());

            // Theme
            SelectComboByTag(ThemeCombo, s.Theme);
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
            SelectComboByTag(MaxResultsCombo, def.MaxResultsCount.ToString());
            SelectComboByTag(ThemeCombo, def.Theme);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var s = new AppSettings
            {
                HotkeyModifier = GetSelectedTag(HotkeyCombo, "Alt"),
                HotkeyKey = "Space",
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
                Snippets = SettingsService.Instance.Current.Snippets,
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

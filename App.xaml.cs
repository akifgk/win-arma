using System;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;
using WinArma.Services;
using WinArma.Views;

namespace WinArma
{
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;
        private SettingsWindow? _settingsWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var icoPath = System.IO.Path.Combine(baseDir, "app.ico");
                var pngPath = System.IO.Path.Combine(baseDir, "logo.png");
                Utils.IconGenerator.EnsureIconExists(icoPath, pngPath);

                _mainWindow = new MainWindow();
                _mainWindow.Show();
                SetupTrayIcon();

                SettingsService.Instance.SettingsChanged += OnSettingsChanged;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Başlatma hatası: {ex.Message}", "Win-Arma", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSettingsChanged(Models.AppSettings settings)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = $"Win-Arma ({settings.HotkeyModifier} + Space)";
            }
        }

        private void SetupTrayIcon()
        {
            var initialSettings = SettingsService.Instance.Current;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var icoPath = System.IO.Path.Combine(baseDir, "app.ico");

            System.Drawing.Icon appIcon = SystemIcons.Application;
            if (System.IO.File.Exists(icoPath))
            {
                try
                {
                    appIcon = new System.Drawing.Icon(icoPath);
                }
                catch { }
            }

            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = appIcon,
                Visible = true,
                Text = $"Win-Arma ({initialSettings.HotkeyModifier} + Space)"
            };

            var contextMenu = new Forms.ContextMenuStrip();

            var openItem = new Forms.ToolStripMenuItem("Arama Çubuğunu Aç");
            openItem.Font = new Font(openItem.Font, System.Drawing.FontStyle.Bold);
            openItem.Click += (s, e) => _mainWindow?.ToggleVisibility();

            var settingsItem = new Forms.ToolStripMenuItem("⚙️ Ayarlar");
            settingsItem.Click += (s, e) => OpenSettings();

            var aboutItem = new Forms.ToolStripMenuItem("Hakkında");
            aboutItem.Click += (s, e) =>
            {
                System.Windows.MessageBox.Show(
                    "Win-Arma Spotlight\nmacOS Spotlight benzeri hızlı arama ve başlatıcı.\n\n© 2026",
                    "Win-Arma",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            };

            var exitItem = new Forms.ToolStripMenuItem("Çıkış");
            exitItem.Click += (s, e) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                Shutdown();
            };

            contextMenu.Items.Add(openItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => _mainWindow?.ToggleVisibility();
        }

        public void OpenSettings()
        {
            if (_settingsWindow != null && _settingsWindow.IsLoaded)
            {
                _settingsWindow.Activate();
                _settingsWindow.Focus();
                return;
            }

            _settingsWindow = new SettingsWindow();
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }

            base.OnExit(e);
        }
    }
}

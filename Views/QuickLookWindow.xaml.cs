using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinArma.Services;
using WinArma.Utils;

namespace WinArma.Views
{
    public partial class QuickLookWindow : Window
    {
        private readonly string _targetPath;

        public QuickLookWindow(string targetPath)
        {
            InitializeComponent();
            _targetPath = targetPath;
            Loaded += QuickLookWindow_Loaded;
        }

        private void QuickLookWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
            LoadPreview();
        }

        private void ApplyTheme()
        {
            var theme = SettingsService.Instance.Current.Theme;
            if (string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                QuickLookBorder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                QuickLookBorder.BorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
                FileNameText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0F172A"));
                FileMetaText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#64748B"));
                TextViewerBorder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8FAFC"));
                TextViewerBorder.BorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
                PreviewTextBox.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0F172A"));
                InfoViewerBorder.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8FAFC"));
                InfoViewerBorder.BorderBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E2E8F0"));
                InfoTitleText.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0F172A"));
            }
        }

        private void LoadPreview()
        {
            if (string.IsNullOrWhiteSpace(_targetPath)) return;

            bool isDir = Directory.Exists(_targetPath);
            bool isFile = File.Exists(_targetPath);

            if (!isDir && !isFile)
            {
                FileNameText.Text = "Bulunamadı";
                FileMetaText.Text = _targetPath;
                return;
            }

            FileNameText.Text = Path.GetFileName(_targetPath);
            var icon = IconHelper.GetIconForFile(_targetPath);
            if (icon != null)
            {
                FileIconImage.Source = icon;
                FileIconImage.Visibility = Visibility.Visible;
                FileIconSymbol.Visibility = Visibility.Collapsed;
            }
            else
            {
                FileIconSymbol.Text = isDir ? "📁" : "📄";
                FileIconSymbol.Visibility = Visibility.Visible;
                FileIconImage.Visibility = Visibility.Collapsed;
            }

            if (isDir)
            {
                var dirInfo = new DirectoryInfo(_targetPath);
                FileMetaText.Text = $"Klasör • Değiştirilme: {dirInfo.LastWriteTime:dd.MM.yyyy HH:mm}";
                ShowInfoView("Klasör", _targetPath, $"Oluşturulma: {dirInfo.CreationTime:dd.MM.yyyy}");
                return;
            }

            var fileInfo = new FileInfo(_targetPath);
            string ext = fileInfo.Extension.ToLowerInvariant();
            string sizeStr = FormatBytes(fileInfo.Length);
            FileMetaText.Text = $"{sizeStr} • Değiştirilme: {fileInfo.LastWriteTime:dd.MM.yyyy HH:mm}";

            // 1. Check if Image
            var imageExts = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".ico" };
            if (Array.IndexOf(imageExts, ext) >= 0)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(_targetPath);
                    bmp.EndInit();
                    bmp.Freeze();

                    PreviewImage.Source = bmp;
                    FileMetaText.Text = $"{bmp.PixelWidth}x{bmp.PixelHeight} px • {sizeStr}";

                    ImageViewerBorder.Visibility = Visibility.Visible;
                    TextViewerBorder.Visibility = Visibility.Collapsed;
                    InfoViewerBorder.Visibility = Visibility.Collapsed;
                    return;
                }
                catch { }
            }

            // 2. Check if Text / Code file
            var textExts = new[]
            {
                ".txt", ".cs", ".js", ".ts", ".jsx", ".tsx", ".json", ".md", ".xml", ".html",
                ".css", ".py", ".bat", ".cmd", ".ps1", ".log", ".ini", ".yaml", ".yml", ".sql",
                ".c", ".cpp", ".h", ".rs", ".go", ".java", ".sh", ".env", ".gitignore"
            };

            if (Array.IndexOf(textExts, ext) >= 0 || fileInfo.Length < 100 * 1024)
            {
                try
                {
                    using var stream = new FileStream(_targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using var reader = new StreamReader(stream);

                    var lines = new System.Text.StringBuilder();
                    int count = 0;
                    while (!reader.EndOfStream && count < 120)
                    {
                        lines.AppendLine(reader.ReadLine());
                        count++;
                    }

                    if (!reader.EndOfStream)
                    {
                        lines.AppendLine("\n... (önizleme 120 satır ile sınırlandırıldı)");
                    }

                    PreviewTextBox.Text = lines.ToString();
                    TextViewerBorder.Visibility = Visibility.Visible;
                    ImageViewerBorder.Visibility = Visibility.Collapsed;
                    InfoViewerBorder.Visibility = Visibility.Collapsed;
                    return;
                }
                catch { }
            }

            // 3. Fallback: Info View
            ShowInfoView(fileInfo.Extension.ToUpperInvariant() + " Dosyası", _targetPath, $"Boyut: {sizeStr} • Son Değişiklik: {fileInfo.LastWriteTime:dd.MM.yyyy HH:mm}");
        }

        private void ShowInfoView(string title, string path, string details)
        {
            InfoTitleText.Text = title;
            InfoPathText.Text = path;
            InfoDetailsText.Text = details;

            InfoViewerBorder.Visibility = Visibility.Visible;
            ImageViewerBorder.Visibility = Visibility.Collapsed;
            TextViewerBorder.Visibility = Visibility.Collapsed;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F1} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F1} GB";
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchFile();
            Close();
        }

        private void RevealButton_Click(object sender, RoutedEventArgs e)
        {
            RevealInFolder();
            Close();
        }

        private void LaunchFile()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _targetPath,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private void RevealInFolder()
        {
            try
            {
                Process.Start("explorer.exe", $"/select,\"{_targetPath}\"");
            }
            catch { }
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Space)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    RevealInFolder();
                }
                else
                {
                    LaunchFile();
                }
                Close();
                e.Handled = true;
            }
        }
    }
}

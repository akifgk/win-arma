using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    public class ClipboardEntry
    {
        public string Text { get; set; } = string.Empty;
        public DateTime CopiedAt { get; set; } = DateTime.Now;
    }

    public class ClipboardHistoryService : IDisposable
    {
        private static ClipboardHistoryService? _instance;
        public static ClipboardHistoryService Instance => _instance ??= new ClipboardHistoryService();

        private readonly List<ClipboardEntry> _history = new();
        private readonly string _storageFilePath;
        private IntPtr _hWnd = IntPtr.Zero;
        private HwndSource? _hwndSource;
        private bool _isListening = false;
        private readonly object _lock = new();

        public ClipboardHistoryService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinArma");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            _storageFilePath = Path.Combine(folder, "clipboard_history.json");
            LoadHistory();
        }

        public void Attach(IntPtr hWnd, HwndSource hwndSource)
        {
            if (_isListening) return;

            _hWnd = hWnd;
            _hwndSource = hwndSource;
            _hwndSource.AddHook(HwndHook);
            NativeMethods.AddClipboardFormatListener(_hWnd);
            _isListening = true;
        }

        public void Detach()
        {
            if (!_isListening || _hWnd == IntPtr.Zero) return;

            NativeMethods.RemoveClipboardFormatListener(_hWnd);
            _hwndSource?.RemoveHook(HwndHook);
            _isListening = false;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }
            return IntPtr.Zero;
        }

        private void OnClipboardChanged()
        {
            if (!SettingsService.Instance.Current.EnableClipboardHistory) return;

            try
            {
                if (System.Windows.Clipboard.ContainsText())
                {
                    string text = System.Windows.Clipboard.GetText();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        AddEntry(text.Trim());
                    }
                }
            }
            catch
            {
                // Clipboard might be locked by another process
            }
        }

        private void AddEntry(string text)
        {
            lock (_lock)
            {
                // Don't add duplicate if it's already the top entry
                if (_history.Count > 0 && string.Equals(_history[0].Text, text, StringComparison.Ordinal))
                {
                    return;
                }

                // Remove previous identical instance so it moves to top
                _history.RemoveAll(x => string.Equals(x.Text, text, StringComparison.Ordinal));

                _history.Insert(0, new ClipboardEntry
                {
                    Text = text,
                    CopiedAt = DateTime.Now
                });

                if (_history.Count > 50)
                {
                    _history.RemoveRange(50, _history.Count - 50);
                }

                SaveHistory();
            }
        }

        public List<SearchResultItem> Search(string query)
        {
            if (!SettingsService.Instance.Current.EnableClipboardHistory)
            {
                return new List<SearchResultItem>();
            }

            var clean = query.Trim().ToLowerInvariant();
            bool isExplicitPanoCommand = clean == "pano" || clean == "cb" || clean == "clip";
            string filterText = "";

            if (clean.StartsWith("pano ") || clean.StartsWith("cb ") || clean.StartsWith("clip "))
            {
                isExplicitPanoCommand = true;
                int spaceIndex = clean.IndexOf(' ');
                filterText = clean.Substring(spaceIndex + 1).Trim();
            }

            var results = new List<SearchResultItem>();

            lock (_lock)
            {
                var candidates = _history.AsEnumerable();

                if (!string.IsNullOrEmpty(filterText))
                {
                    candidates = candidates.Where(c => c.Text.Contains(filterText, StringComparison.OrdinalIgnoreCase));
                }
                else if (!isExplicitPanoCommand)
                {
                    // If not explicit "pano", only show if search query matches clipboard item
                    if (clean.Length < 3) return results;
                    candidates = candidates.Where(c => c.Text.Contains(clean, StringComparison.OrdinalIgnoreCase)).Take(3);
                }

                int score = isExplicitPanoCommand ? 980 : 350;

                foreach (var entry in candidates.Take(10))
                {
                    string firstLine = entry.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? entry.Text;
                    if (firstLine.Length > 65) firstLine = firstLine.Substring(0, 62) + "...";

                    string timeStr = entry.CopiedAt.ToString("HH:mm");
                    string charCountStr = $"{entry.Text.Length} karakter";

                    var capturedText = entry.Text;

                    results.Add(new SearchResultItem
                    {
                        Title = firstLine,
                        Subtitle = $"{timeStr} • {charCountStr} (Panoya kopyalamak için Enter)",
                        Category = SearchResultCategory.ClipboardItem,
                        IconSymbol = "📋",
                        ActionTarget = capturedText,
                        Score = score--,
                        CustomAction = () =>
                        {
                            try
                            {
                                System.Windows.Clipboard.SetText(capturedText);
                            }
                            catch { }
                        }
                    });
                }
            }

            return results;
        }

        private void LoadHistory()
        {
            try
            {
                if (File.Exists(_storageFilePath))
                {
                    var json = File.ReadAllText(_storageFilePath);
                    var items = JsonSerializer.Deserialize<List<ClipboardEntry>>(json);
                    if (items != null)
                    {
                        _history.AddRange(items);
                    }
                }
            }
            catch { }
        }

        private void SaveHistory()
        {
            try
            {
                var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_storageFilePath, json);
            }
            catch { }
        }

        public void Dispose()
        {
            Detach();
        }
    }
}

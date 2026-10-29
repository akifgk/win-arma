using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    public class AppSearchService
    {
        private readonly List<SearchResultItem> _indexedApps = new();
        private readonly object _lock = new();

        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    _indexedApps.Clear();
                    LoadBuiltInTools();
                    ScanStartMenuDirectories();
                }
            });
        }

        private void LoadBuiltInTools()
        {
            var builtIn = new List<(string Title, string Subtitle, string Target)>
            {
                ("Ayarlar (Windows Settings)", "Sistem Yapılandırması", "ms-settings:"),
                ("Hesap Makinesi (Calculator)", "Windows Uygulaması", "calc.exe"),
                ("Not Defteri (Notepad)", "Metin Düzenleyici", "notepad.exe"),
                ("Görev Yöneticisi (Task Manager)", "Sistem İzleme", "taskmgr.exe"),
                ("Dosya Gezgini (File Explorer)", "Dosya Yönetimi", "explorer.exe"),
                ("Komut İstemi (Command Prompt)", "Terminal (CMD)", "cmd.exe"),
                ("Windows PowerShell", "Terminal", "powershell.exe"),
                ("Windows Terminal", "Modern Terminal", "wt.exe"),
                ("Denetim Masası (Control Panel)", "Sistem Yönetimi", "control.exe"),
                ("Kayıt Defteri Düzenleyicisi (Regedit)", "Gelişmiş Yapılandırma", "regedit.exe"),
                ("Aygıt Yöneticisi (Device Manager)", "Donanım Yönetimi", "devmgmt.msc"),
                ("Hizmetler (Services)", "Arka Plan Hizmetleri", "services.msc"),
                ("Disk Yönetimi (Disk Management)", "Depolama Yönetimi", "diskmgmt.msc"),
                ("Ekran Alıntısı Aracı (Snipping Tool)", "Ekran Görüntüsü Alma", "ms-screenclip:")
            };

            foreach (var item in builtIn)
            {
                _indexedApps.Add(new SearchResultItem
                {
                    Title = item.Title,
                    Subtitle = item.Subtitle,
                    Category = SearchResultCategory.Application,
                    IconSymbol = "⚡",
                    ActionTarget = item.Target,
                    Score = 90
                });
            }
        }

        private void ScanStartMenuDirectories()
        {
            var startMenuPaths = new List<string>
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs")
            };

            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in startMenuPaths.Distinct())
            {
                if (!Directory.Exists(path)) continue;

                try
                {
                    var files = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);

                        // Filter out uninstallation shortcuts or duplicates
                        if (fileName.Contains("uninstall", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Contains("kaldır", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Contains("yardım", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Contains("help", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!seenNames.Add(fileName))
                        {
                            continue;
                        }

                        var icon = IconHelper.GetIconForFile(file);

                        _indexedApps.Add(new SearchResultItem
                        {
                            Title = fileName,
                            Subtitle = file,
                            Category = SearchResultCategory.Application,
                            Icon = icon,
                            IconSymbol = "🚀",
                            ActionTarget = file,
                            Score = 100
                        });
                    }
                }
                catch
                {
                    // Ignore inaccessible directories
                }
            }
        }

        public List<SearchResultItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<SearchResultItem>();

            var trimmed = query.Trim().ToLowerInvariant();
            var results = new List<(SearchResultItem Item, int MatchScore)>();

            lock (_lock)
            {
                foreach (var app in _indexedApps)
                {
                    var title = app.Title.ToLowerInvariant();
                    int score = CalculateMatchScore(title, trimmed);

                    if (score > 0)
                    {
                        var clone = new SearchResultItem
                        {
                            Title = app.Title,
                            Subtitle = app.Subtitle,
                            Category = app.Category,
                            Icon = app.Icon,
                            IconSymbol = app.IconSymbol,
                            ActionTarget = app.ActionTarget,
                            CustomAction = app.CustomAction,
                            Score = score
                        };
                        results.Add((clone, score));
                    }
                }
            }

            return results
                .OrderByDescending(r => r.MatchScore)
                .Select(r => r.Item)
                .Take(8)
                .ToList();
        }

        private int CalculateMatchScore(string title, string query)
        {
            if (title == query) return 1000;
            if (title.StartsWith(query)) return 800 - (title.Length - query.Length);
            
            // Check word starts (e.g. "vs code" -> "Visual Studio Code")
            var words = title.Split(new[] { ' ', '-', '_', '.' }, StringSplitOptions.RemoveEmptyEntries);
            var acronym = new string(words.Where(w => w.Length > 0).Select(w => w[0]).ToArray());
            if (acronym.StartsWith(query)) return 600;

            if (words.Any(w => w.StartsWith(query))) return 500;
            if (title.Contains(query)) return 300 - (title.IndexOf(query));

            // Fuzzy subsequence check
            int qIdx = 0;
            for (int i = 0; i < title.Length && qIdx < query.Length; i++)
            {
                if (title[i] == query[qIdx])
                {
                    qIdx++;
                }
            }

            if (qIdx == query.Length) return 100;

            return 0;
        }
    }
}

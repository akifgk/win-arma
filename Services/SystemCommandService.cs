using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    public class SystemCommandService
    {
        private readonly List<(string Key, string Title, string Subtitle, Action Action)> _commands = new();

        public SystemCommandService()
        {
            _commands.Add(("kilitle", "Bilgisayarı Kilitle", "Oturumu güvenli bir şekilde kilitler", () =>
            {
                NativeMethods.LockWorkStation();
            }));

            _commands.Add(("kapat", "Bilgisayarı Kapat", "Sistemi 30 saniye içinde kapatır (İptal: shutdown /a)", () =>
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/s /t 30") { CreateNoWindow = true, UseShellExecute = false });
            }));

            _commands.Add(("yeniden başlat", "Bilgisayarı Yeniden Başlat", "Sistemi yeniden başlatır", () =>
            {
                Process.Start(new ProcessStartInfo("shutdown.exe", "/r /t 10") { CreateNoWindow = true, UseShellExecute = false });
            }));

            _commands.Add(("çöpü boşalt", "Geri Dönüşüm Kutusunu Boşalt", "Silinmiş tüm geçici dosyaları temizler", () =>
            {
                NativeMethods.SHEmptyRecycleBin(IntPtr.Zero, null, NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI);
            }));

            _commands.Add(("uyku", "Uyku Modu", "Bilgisayarı düşük güç tasarrufu moduna alır", () =>
            {
                Process.Start(new ProcessStartInfo("rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0") { CreateNoWindow = true, UseShellExecute = false });
            }));
        }

        public List<SearchResultItem> MatchCommands(string query)
        {
            var results = new List<SearchResultItem>();
            if (string.IsNullOrWhiteSpace(query)) return results;

            var clean = query.Trim().ToLowerInvariant();

            foreach (var cmd in _commands)
            {
                if (cmd.Key.Contains(clean) || clean.Contains(cmd.Key))
                {
                    results.Add(new SearchResultItem
                    {
                        Title = cmd.Title,
                        Subtitle = cmd.Subtitle,
                        Category = SearchResultCategory.SystemCommand,
                        IconSymbol = "⚙️",
                        Score = 850,
                        CustomAction = cmd.Action
                    });
                }
            }

            return results;
        }
    }
}

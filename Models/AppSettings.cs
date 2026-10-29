using System;
using System.Collections.Generic;

namespace WinArma.Models
{
    public class AppSettings
    {
        // Hotkey Settings
        public string HotkeyModifier { get; set; } = "Alt"; // "Alt", "Ctrl", "Win", "Alt+Shift"
        public string HotkeyKey { get; set; } = "Space";    // "Space", "F1", "F2", etc.

        // Search Engine
        public string DefaultSearchEngine { get; set; } = "Google"; // "Google", "Bing", "DuckDuckGo", "Yandex", "Brave"

        // Search Scope / Modules Enabled
        public bool EnableAppSearch { get; set; } = true;
        public bool EnableFileSearch { get; set; } = true;
        public bool EnableWebSearch { get; set; } = true;
        public bool EnableCalculator { get; set; } = true;
        public bool EnableSystemCommands { get; set; } = true;

        // New Power Features
        public bool EnableClipboardHistory { get; set; } = true;
        public bool EnableQuickLook { get; set; } = true;
        public bool EnableSnippets { get; set; } = true;

        // Snippets Collection (Key -> Text)
        public Dictionary<string, string> Snippets { get; set; } = new(StringComparer.OrdinalIgnoreCase)
        {
            { "!mail", "ornek@sirket.com" },
            { "!iban", "TR00 0000 0000 0000 0000 0000 00" },
            { "!tel", "+90 555 123 4567" },
            { "!adres", "Atatürk Mah. Cumhuriyet Cad. No:1 İstanbul" },
            { "!shrug", @"¯\_(ツ)_/¯" }
        };

        // UI & Appearance
        public int MaxResultsCount { get; set; } = 8;
        public double WindowOpacity { get; set; } = 0.98;
        public string Theme { get; set; } = "Dark"; // "Dark", "Midnight", "Amoled", "Light"

        // Startup
        public bool RunAtStartup { get; set; } = false;

        public string GetSearchEngineUrl(string query)
        {
            var encoded = Uri.EscapeDataString(query);
            return DefaultSearchEngine switch
            {
                "Bing" => $"https://www.bing.com/search?q={encoded}",
                "DuckDuckGo" => $"https://duckduckgo.com/?q={encoded}",
                "Yandex" => $"https://yandex.com/search/?text={encoded}",
                "Brave" => $"https://search.brave.com/search?q={encoded}",
                _ => $"https://www.google.com/search?q={encoded}"
            };
        }
    }
}

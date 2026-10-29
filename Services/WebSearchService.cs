using System;
using System.Collections.Generic;
using System.Net;
using WinArma.Models;

namespace WinArma.Services
{
    public class WebSearchService
    {
        public List<SearchResultItem> GetWebActions(string query)
        {
            var results = new List<SearchResultItem>();
            if (string.IsNullOrWhiteSpace(query)) return results;

            var trimmed = query.Trim();

            // 1. Direct URL detection
            if (IsUrl(trimmed))
            {
                var fullUrl = trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                              trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                              ? trimmed
                              : "https://" + trimmed;

                results.Add(new SearchResultItem
                {
                    Title = $"Web Adresini Aç: {trimmed}",
                    Subtitle = fullUrl,
                    Category = SearchResultCategory.Url,
                    IconSymbol = "🔗",
                    ActionTarget = fullUrl,
                    Score = 950
                });
            }

            // 2. Prefix Shortcuts (e.g., "g react", "y lo-fi", "gh flutter", "b ai")
            if (trimmed.StartsWith("g ", StringComparison.OrdinalIgnoreCase))
            {
                var q = trimmed.Substring(2).Trim();
                results.Add(CreateSearchItem($"Google'da Ara: \"{q}\"", $"https://www.google.com/search?q={WebUtility.UrlEncode(q)}", "🌐"));
                return results;
            }
            if (trimmed.StartsWith("b ", StringComparison.OrdinalIgnoreCase))
            {
                var q = trimmed.Substring(2).Trim();
                results.Add(CreateSearchItem($"Bing'de Ara: \"{q}\"", $"https://www.bing.com/search?q={WebUtility.UrlEncode(q)}", "🌐"));
                return results;
            }
            if (trimmed.StartsWith("y ", StringComparison.OrdinalIgnoreCase))
            {
                var q = trimmed.Substring(2).Trim();
                results.Add(CreateSearchItem($"YouTube'da Ara: \"{q}\"", $"https://www.youtube.com/results?search_query={WebUtility.UrlEncode(q)}", "▶️"));
                return results;
            }
            if (trimmed.StartsWith("gh ", StringComparison.OrdinalIgnoreCase))
            {
                var q = trimmed.Substring(3).Trim();
                results.Add(CreateSearchItem($"GitHub'da Ara: \"{q}\"", $"https://github.com/search?q={WebUtility.UrlEncode(q)}", "🐙"));
                return results;
            }
            if (trimmed.StartsWith("w ", StringComparison.OrdinalIgnoreCase))
            {
                var q = trimmed.Substring(2).Trim();
                results.Add(CreateSearchItem($"Vikipedi'de Ara: \"{q}\"", $"https://tr.wikipedia.org/wiki/Special:Search?search={WebUtility.UrlEncode(q)}", "📖"));
                return results;
            }

            // 3. Default fallback search using configured engine
            var settings = SettingsService.Instance.Current;
            var engineName = settings.DefaultSearchEngine;
            var engineUrl = settings.GetSearchEngineUrl(trimmed);

            results.Add(CreateSearchItem($"{engineName}'da Ara: \"{trimmed}\"", engineUrl, "🌐", 50));

            return results;
        }

        private SearchResultItem CreateSearchItem(string title, string url, string symbol, int score = 900)
        {
            return new SearchResultItem
            {
                Title = title,
                Subtitle = url,
                Category = SearchResultCategory.WebSearch,
                IconSymbol = symbol,
                ActionTarget = url,
                Score = score
            };
        }

        private bool IsUrl(string text)
        {
            if (text.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                text.StartsWith("localhost:", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!text.Contains(" ") && text.Contains("."))
            {
                var commonTlds = new[] { ".com", ".net", ".org", ".edu", ".gov", ".io", ".dev", ".ai", ".co", ".tr", ".me", ".app" };
                foreach (var tld in commonTlds)
                {
                    if (text.EndsWith(tld, StringComparison.OrdinalIgnoreCase) ||
                        text.Contains(tld + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

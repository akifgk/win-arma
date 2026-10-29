using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WinArma.Models;
using WinArma.Utils;

namespace WinArma.Services
{
    public class FileSearchService
    {
        public async Task<List<SearchResultItem>> SearchAsync(string query, int maxResults = 8)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            {
                return new List<SearchResultItem>();
            }

            return await Task.Run(() =>
            {
                var cleanQuery = query.Trim();
                var results = new List<SearchResultItem>();

                // 1. Direct path check (e.g., C:\Users or D:\Projects)
                if (cleanQuery.Contains(":\\") || cleanQuery.Contains(":/"))
                {
                    results.AddRange(SearchDirectoryPath(cleanQuery, maxResults));
                    if (results.Count > 0) return results;
                }

                // 2. Try Windows Search Index (very fast, covers indexed drives)
                try
                {
                    results.AddRange(SearchWindowsIndex(cleanQuery, maxResults));
                }
                catch
                {
                    // If Windows Search service is unavailable, fallback to common folders
                }

                // 3. Fallback: Quick scan of common folders if index returned few results
                if (results.Count < maxResults)
                {
                    var fallbackResults = SearchCommonFolders(cleanQuery, maxResults - results.Count);
                    foreach (var item in fallbackResults)
                    {
                        if (!results.Any(r => string.Equals(r.ActionTarget, item.ActionTarget, StringComparison.OrdinalIgnoreCase)))
                        {
                            results.Add(item);
                        }
                    }
                }

                return results.Take(maxResults).ToList();
            });
        }

        private List<SearchResultItem> SearchWindowsIndex(string query, int maxResults)
        {
            var results = new List<SearchResultItem>();
            var escapedQuery = query.Replace("'", "''");

            // CollatorDSO is the native Windows Search OLE DB provider
            var connectionString = "Provider=Search.CollatorDSO;Extended Properties='Application=Windows';";
            var sql = $"SELECT TOP {maxResults} System.ItemName, System.ItemPathDisplay, System.ItemType " +
                      $"FROM SystemIndex " +
                      $"WHERE SCOPE='file:' AND System.ItemName LIKE '%{escapedQuery}%' " +
                      $"ORDER BY System.DateModified DESC";

            using var connection = new OleDbConnection(connectionString);
            connection.Open();

            using var command = new OleDbCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var name = reader["System.ItemName"]?.ToString() ?? string.Empty;
                var path = reader["System.ItemPathDisplay"]?.ToString() ?? string.Empty;
                var itemType = reader["System.ItemType"]?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) && !Directory.Exists(path))
                {
                    continue;
                }

                bool isFolder = Directory.Exists(path);
                var icon = IconHelper.GetIconForFile(path);

                results.Add(new SearchResultItem
                {
                    Title = name,
                    Subtitle = path,
                    Category = isFolder ? SearchResultCategory.Folder : SearchResultCategory.File,
                    Icon = icon,
                    IconSymbol = isFolder ? "📁" : "📄",
                    ActionTarget = path,
                    Score = 70
                });
            }

            return results;
        }

        private List<SearchResultItem> SearchCommonFolders(string query, int maxResults)
        {
            var results = new List<SearchResultItem>();
            var targetFolders = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Pictures"),
                @"C:\belgeler"
            };

            foreach (var folder in targetFolders)
            {
                if (!Directory.Exists(folder)) continue;

                try
                {
                    var entries = Directory.EnumerateFileSystemEntries(folder, $"*{query}*", SearchOption.TopDirectoryOnly);
                    foreach (var entry in entries)
                    {
                        var name = Path.GetFileName(entry);
                        bool isFolder = Directory.Exists(entry);
                        var icon = IconHelper.GetIconForFile(entry);

                        results.Add(new SearchResultItem
                        {
                            Title = name,
                            Subtitle = entry,
                            Category = isFolder ? SearchResultCategory.Folder : SearchResultCategory.File,
                            Icon = icon,
                            IconSymbol = isFolder ? "📁" : "📄",
                            ActionTarget = entry,
                            Score = 60
                        });

                        if (results.Count >= maxResults) return results;
                    }
                }
                catch
                {
                    // Skip folders with access restrictions
                }
            }

            return results;
        }

        private List<SearchResultItem> SearchDirectoryPath(string pathQuery, int maxResults)
        {
            var results = new List<SearchResultItem>();
            try
            {
                string dir = pathQuery;
                string filter = "*";

                if (!Directory.Exists(dir))
                {
                    var lastSlash = Math.Max(pathQuery.LastIndexOf('\\'), pathQuery.LastIndexOf('/'));
                    if (lastSlash > 0)
                    {
                        dir = pathQuery.Substring(0, lastSlash + 1);
                        filter = pathQuery.Substring(lastSlash + 1) + "*";
                    }
                }

                if (Directory.Exists(dir))
                {
                    var entries = Directory.EnumerateFileSystemEntries(dir, filter, SearchOption.TopDirectoryOnly);
                    foreach (var entry in entries.Take(maxResults))
                    {
                        var name = Path.GetFileName(entry);
                        bool isFolder = Directory.Exists(entry);
                        results.Add(new SearchResultItem
                        {
                            Title = name,
                            Subtitle = entry,
                            Category = isFolder ? SearchResultCategory.Folder : SearchResultCategory.File,
                            Icon = IconHelper.GetIconForFile(entry),
                            IconSymbol = isFolder ? "📁" : "📄",
                            ActionTarget = entry,
                            Score = 80
                        });
                    }
                }
            }
            catch { }

            return results;
        }
    }
}

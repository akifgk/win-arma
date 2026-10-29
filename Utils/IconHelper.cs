using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WinArma.Utils
{
    public static class IconHelper
    {
        private static readonly ConcurrentDictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);

        public static ImageSource? GetIconForFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;

            if (_cache.TryGetValue(filePath, out var cached))
            {
                return cached;
            }

            try
            {
                if (File.Exists(filePath) || Directory.Exists(filePath))
                {
                    using var icon = Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                        
                        bitmapSource.Freeze();
                        _cache[filePath] = bitmapSource;
                        return bitmapSource;
                    }
                }
            }
            catch
            {
                // Ignore icon extraction failure
            }

            _cache[filePath] = null;
            return null;
        }
    }
}

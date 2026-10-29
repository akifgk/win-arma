using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;
using MediaPen = System.Windows.Media.Pen;
using MediaPoint = System.Windows.Point;

namespace WinArma.Utils
{
    public static class IconGenerator
    {
        public static void EnsureIconExists(string icoPath, string pngPath)
        {
            if (File.Exists(icoPath) && File.Exists(pngPath)) return;

            try
            {
                var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
                var pngBytesList = new List<byte[]>();

                foreach (var size in sizes)
                {
                    var bmp = RenderLogo(size);
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));

                    using var ms = new MemoryStream();
                    encoder.Save(ms);
                    var bytes = ms.ToArray();
                    pngBytesList.Add(bytes);

                    if (size == 256)
                    {
                        File.WriteAllBytes(pngPath, bytes);
                    }
                }

                SaveIco(icoPath, pngBytesList, sizes);
            }
            catch { }
        }

        public static RenderTargetBitmap RenderLogo(int size)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                double s = size;
                double r = s * 0.22; // Squircle corner radius

                // 1. Outer Dark Squircle Background
                var bgGradient = new LinearGradientBrush(
                    (MediaColor)MediaColorConverter.ConvertFromString("#181926"),
                    (MediaColor)MediaColorConverter.ConvertFromString("#0C0D14"),
                    new MediaPoint(0, 0),
                    new MediaPoint(1, 1));

                var borderPen = new MediaPen(
                    new LinearGradientBrush(
                        (MediaColor)MediaColorConverter.ConvertFromString("#38BDF8"),
                        (MediaColor)MediaColorConverter.ConvertFromString("#818CF8"),
                        new MediaPoint(0, 0),
                        new MediaPoint(1, 1)),
                    Math.Max(1.0, s * 0.025));

                dc.DrawRoundedRectangle(bgGradient, borderPen, new Rect(s * 0.04, s * 0.04, s * 0.92, s * 0.92), r, r);

                // 2. Subtle Glow behind search prism
                var glowBrush = new RadialGradientBrush(
                    (MediaColor)MediaColorConverter.ConvertFromString("#2563EB"),
                    Colors.Transparent)
                {
                    RadiusX = 0.5,
                    RadiusY = 0.5,
                    Center = new MediaPoint(0.46, 0.46),
                    GradientOrigin = new MediaPoint(0.46, 0.46),
                    Opacity = 0.45
                };
                dc.DrawEllipse(glowBrush, null, new MediaPoint(s * 0.46, s * 0.46), s * 0.38, s * 0.38);

                // 3. Search Lens Ring
                var lensCenter = new MediaPoint(s * 0.43, s * 0.43);
                double lensRadius = s * 0.24;

                var lensPen = new MediaPen(
                    new LinearGradientBrush(
                        (MediaColor)MediaColorConverter.ConvertFromString("#00F2FE"),
                        (MediaColor)MediaColorConverter.ConvertFromString("#4FACFE"),
                        new MediaPoint(0, 0),
                        new MediaPoint(1, 1)),
                    Math.Max(1.5, s * 0.085));

                var lensFill = new LinearGradientBrush(
                    MediaColor.FromArgb(50, 0, 242, 254),
                    MediaColor.FromArgb(20, 79, 172, 254),
                    new MediaPoint(0, 0),
                    new MediaPoint(1, 1));

                dc.DrawEllipse(lensFill, lensPen, lensCenter, lensRadius, lensRadius);

                // 4. Modern Prism Spark (Chevron / Arrow inside lens)
                var sparkPen = new MediaPen(
                    new LinearGradientBrush(
                        (MediaColor)MediaColorConverter.ConvertFromString("#FFFFFF"),
                        (MediaColor)MediaColorConverter.ConvertFromString("#38BDF8"),
                        new MediaPoint(0, 0),
                        new MediaPoint(1, 1)),
                    Math.Max(1.2, s * 0.065))
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round,
                    LineJoin = PenLineJoin.Round
                };

                // Fast dynamic chevron
                var p1 = new MediaPoint(lensCenter.X - lensRadius * 0.45, lensCenter.Y - lensRadius * 0.35);
                var p2 = new MediaPoint(lensCenter.X + lensRadius * 0.25, lensCenter.Y);
                var p3 = new MediaPoint(lensCenter.X - lensRadius * 0.45, lensCenter.Y + lensRadius * 0.35);

                var sparkGeom = new StreamGeometry();
                using (var ctx = sparkGeom.Open())
                {
                    ctx.BeginFigure(p1, false, false);
                    ctx.LineTo(p2, true, false);
                    ctx.LineTo(p3, true, false);
                }
                sparkGeom.Freeze();
                dc.DrawGeometry(null, sparkPen, sparkGeom);

                // 5. Stylized Lens Handle
                var handlePen = new MediaPen(
                    new LinearGradientBrush(
                        (MediaColor)MediaColorConverter.ConvertFromString("#4FACFE"),
                        (MediaColor)MediaColorConverter.ConvertFromString("#818CF8"),
                        new MediaPoint(0, 0),
                        new MediaPoint(1, 1)),
                    Math.Max(1.8, s * 0.09))
                {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };

                var handleStart = new MediaPoint(lensCenter.X + lensRadius * 0.72, lensCenter.Y + lensRadius * 0.72);
                var handleEnd = new MediaPoint(s * 0.82, s * 0.82);
                dc.DrawLine(handlePen, handleStart, handleEnd);
            }

            var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }

        private static void SaveIco(string outputPath, List<byte[]> pngBytes, int[] sizes)
        {
            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            int count = sizes.Length;

            // ICONDIR Header
            bw.Write((ushort)0);      // Reserved
            bw.Write((ushort)1);      // Type (1 = Icon)
            bw.Write((ushort)count);  // Count

            int offset = 6 + (16 * count);

            // ICONDIRENTRY list
            for (int i = 0; i < count; i++)
            {
                int w = sizes[i];
                bw.Write((byte)(w >= 256 ? 0 : w)); // Width (0 means 256)
                bw.Write((byte)(w >= 256 ? 0 : w)); // Height
                bw.Write((byte)0);                  // Color count
                bw.Write((byte)0);                  // Reserved
                bw.Write((ushort)1);                // Color planes
                bw.Write((ushort)32);               // Bits per pixel
                bw.Write((uint)pngBytes[i].Length); // Size of image data
                bw.Write((uint)offset);             // Offset of image data

                offset += pngBytes[i].Length;
            }

            // Write PNG Image byte blocks
            for (int i = 0; i < count; i++)
            {
                bw.Write(pngBytes[i]);
            }
        }
    }
}

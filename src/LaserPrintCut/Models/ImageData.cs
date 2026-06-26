using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace LaserPrintCutAddin.Models
{
    public class ImageData : IDisposable
    {
        public Bitmap SourceBitmap { get; set; }
        public int Width => SourceBitmap?.Width ?? 0;
        public int Height => SourceBitmap?.Height ?? 0;
        public PixelFormat PixelFormat => SourceBitmap?.PixelFormat ?? PixelFormat.Undefined;
        public double DpiX { get; set; }
        public double DpiY { get; set; }

        public void Dispose()
        {
            SourceBitmap?.Dispose();
        }
    }
}
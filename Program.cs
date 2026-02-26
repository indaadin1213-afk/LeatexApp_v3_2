
using System;
using System.Drawing;
using System.IO;
using Svg;

namespace LeatexApp.Services
{
    public static class LogoService
    {
        private static Image? _cache;
        public static Image? GetLogoImage()
        {
            if (_cache != null) return _cache;
            var s = SettingsService.Load();
            if (string.IsNullOrWhiteSpace(s.LogoPath) || !File.Exists(s.LogoPath)) return null;
            var ext = Path.GetExtension(s.LogoPath).ToLowerInvariant();
            try
            {
                if (ext == ".svg")
                {
                    var svg = SvgDocument.Open(s.LogoPath);
                    int h = (int)Math.Max(24, s.LogoHeightPx);
                    int w = (int)(h * 4);
                    var bmp = new Bitmap(w, h);
                    svg.Draw(bmp);
                    _cache = bmp;
                }
                else
                {
                    _cache = Image.FromFile(s.LogoPath);
                }
            }
            catch { _cache = null; }
            return _cache;
        }
    }
}

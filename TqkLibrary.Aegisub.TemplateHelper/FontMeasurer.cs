#if SkiaSharp
using SkiaSharp;
#endif
using System.Drawing;
using System.IO;

#if GDI
using System.Drawing.Text;
using System.Runtime.Versioning;
#endif
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.TemplateHelper
{
#if GDI
    [SupportedOSPlatform("windows")]
#endif
    public class FontMeasurer : IDisposable
    {
#if GDI
        readonly Bitmap _bitmap;
        readonly Graphics _graphics;
        readonly PrivateFontCollection? _pfc;
        readonly FontFamily _family;
        readonly Font _font;
#endif
#if SkiaSharp
        readonly MemoryStream? _memoryStream;
        readonly SKTypeface _typeface;
        readonly SKFont _skFont;
        readonly SKFontMetrics _fontMetrics;
        readonly float _height;
#endif
        public FontMeasurer(AssStyleData style)
        {
#if GDI
            this._bitmap = new(1, 1);
            this._graphics = Graphics.FromImage(_bitmap);

            if (File.Exists(style.FontFilePath))
            {
                _typeface = SKTypeface.FromFile(style.FontFilePath);

                _pfc = new PrivateFontCollection();
                _pfc.AddFontFile(style.FontFilePath);
                _family = _pfc.Families[0];
            }
            else
            {
                _typeface = SKTypeface.FromFamilyName(style.Fontname);

                _family = new FontFamily(style.Fontname);
            }
            _skFont = new SKFont { Typeface = _typeface, Size = style.Fontsize };
            _skFont.GetFontMetrics(out _fontMetrics);
            _height = _fontMetrics.Descent - _fontMetrics.Ascent + _fontMetrics.Leading;

            _font = new Font(_family, style.Fontsize);
#endif
#if SkiaSharp
            if (File.Exists(style.FontFilePath))
            {
                _memoryStream = new MemoryStream(File.ReadAllBytes(style.FontFilePath));
                _memoryStream.Seek(0, SeekOrigin.Begin);
                _typeface = SKTypeface.FromStream(_memoryStream);
            }
            else
            {
                _typeface = SKTypeface.FromFamilyName(style.Fontname);
            }
            _skFont = new SKFont { Typeface = _typeface, Size = style.Fontsize };
            _skFont.GetFontMetrics(out _fontMetrics);
            _height = _fontMetrics.Descent - _fontMetrics.Ascent + _fontMetrics.Leading;
#endif
        }

        public void Dispose()
        {
#if GDI
            _font.Dispose();
            _family.Dispose();
            _graphics.Dispose();
            _bitmap.Dispose();
            _pfc?.Dispose();
#endif
#if SkiaSharp
            _skFont.Dispose();
            _typeface.Dispose();
            _memoryStream?.Dispose();
#endif
        }

        public SizeF MeasureString(string text)
        {
#if GDI
            return _graphics.MeasureString(text, _font);
#endif
#if SkiaSharp
            float width = _skFont.MeasureText(text);
            return new SizeF(width, _height);
#endif
        }
    }
}

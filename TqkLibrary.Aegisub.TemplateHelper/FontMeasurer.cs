using System.Drawing;
using System.Drawing.Text;
using System.Runtime.Versioning;
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.TemplateHelper
{
    [SupportedOSPlatform("windows")]
    public class FontMeasurer : IDisposable
    {
        readonly Bitmap _bitmap;
        readonly Graphics _graphics;
        readonly PrivateFontCollection? _pfc;
        readonly FontFamily _family;
        readonly Font _font;
        public FontMeasurer(AssStyleData style)
        {
            this._bitmap = new(1, 1);
            this._graphics = Graphics.FromImage(_bitmap);

            if (File.Exists(style.FontFilePath))
            {
                _pfc = new PrivateFontCollection();
                _pfc.AddFontFile(style.FontFilePath);
                _family = _pfc.Families[0];
            }
            else
            {
                _family = new FontFamily(style.Fontname);
            }
            _font = new Font(_family, style.Fontsize);
        }

        public void Dispose()
        {
            _font.Dispose();
            _family.Dispose();
            _graphics.Dispose();
            _bitmap.Dispose();
            _pfc?.Dispose();
        }

        public SizeF MeasureString(string text)
        {
            return _graphics.MeasureString(text, _font);
        }
    }
}

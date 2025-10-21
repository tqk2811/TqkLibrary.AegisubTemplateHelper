using System.Drawing;
using TqkLibrary.Aegisub.TemplateHelper;
using TqkLibrary.Aegisub.TemplateHelper.Enums;

namespace TqkLibrary.Aegisub.TemplateHelper.DataClasses
{
    public class AssStyleData
    {
        public string Name { get; set; } = "Default";
        public string Fontname { get; set; } = "Arial";
        public int Fontsize { get; init; } = 48;
        public Color PrimaryColour { get; set; }
        public Color SecondaryColour { get; set; } = Color.Transparent;
        public Color OutlineColour { get; set; } = Color.Transparent;
        public Color BackColour { get; set; } = Color.Transparent;

        public bool Bold { get; set; }//0 and -1
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool StrikeOut { get; set; }
        public int ScaleX { get; set; } = 100;
        public int ScaleY { get; set; } = 100;
        public float Spacing { get; set; } = 0.0f;
        public int Angle { get; set; } = 0;
        public BorderStyle BorderStyle { get; set; } = BorderStyle.OutlineAndDropShadow; // 0: No border, 1: Normal border, 2: Shadowed border
        public float Outline { get; set; } = 0.0f;
        public float Shadow { get; set; } = 0.0f;
        public Alignment Alignment { get; set; } = Alignment.CenteredSub;//2
        public int MarginL { get; set; } = 30;
        public int MarginR { get; set; } = 30;
        public int MarginV { get; set; } = 30;
        public int Encoding { get; set; } = 1;

        public static string Format
        {
            get
            {
                string[] formatParts =
                {
                    nameof(Name),
                    nameof(Fontname),
                    nameof(Fontsize),
                    nameof(PrimaryColour),
                    nameof(SecondaryColour),
                    nameof(OutlineColour),
                    nameof(BackColour),
                    nameof(Bold),
                    nameof(Italic),
                    nameof(Underline),
                    nameof(StrikeOut),
                    nameof(ScaleX),
                    nameof(ScaleY),
                    nameof(Spacing),
                    nameof(Angle),
                    nameof(BorderStyle),
                    nameof(Outline),
                    nameof(Shadow),
                    nameof(Alignment),
                    nameof(MarginL),
                    nameof(MarginR),
                    nameof(MarginV),
                    nameof(Encoding),
                };
                return $"Format: {string.Join(", ", formatParts)}";
            }
        }

        public string Style
        {
            get
            {
                string[] parts =
                {
                    Name,
                    Fontname,
                    Fontsize.ToString(),
                    $"&H{PrimaryColour.ToAssColor()}",
                    $"&H{SecondaryColour.ToAssColor()}",
                    $"&H{OutlineColour.ToAssColor()}",
                    $"&H{BackColour.ToAssColor()}",
                    Bold ? "-1" : "0",
                    Italic ? "-1" : "0",
                    Underline ? "-1" : "0",
                    StrikeOut ? "-1" : "0",
                    ScaleX.ToString(),
                    ScaleY.ToString(),
                    Spacing.ToString("F1"),
                    Angle.ToString(),
                    ((int)BorderStyle).ToString(),
                    Outline.ToString("F1"),
                    Shadow.ToString("F1"),
                    ((int)Alignment).ToString(),
                    MarginL.ToString(),
                    MarginR.ToString(),
                    MarginV.ToString(),
                    Encoding.ToString()
                };
                return $"Style: {string.Join(",", parts)}";

            }
        }
        public override string ToString()
        {
            return $@"[V4+ Styles]
{Format}
{Style}";
        }
    }
}

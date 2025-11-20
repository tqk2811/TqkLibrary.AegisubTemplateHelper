using System;
using System.Collections.Generic;
using System.Drawing;
using TqkLibrary.Aegisub.Enums;

namespace TqkLibrary.Aegisub.Models
{
    public class Dialogue
    {
        public int Layer { get; set; } = 0;
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public required string Style { get; set; }
        public string? Name { get; set; }
        public int MarginL { get; set; } = 0;
        public int MarginR { get; set; } = 0;
        public int MarginV { get; set; } = 0;
        public string? Effect { get; set; }
        public string Text
        {
            get
            {
                string result = string.Join(string.Empty, DialogueSyllableEffects);
                List<string> tags = new();
                if (Pos.HasValue)
                {
                    tags.Add($"\\pos({Pos.Value.X},{Pos.Value.Y})");
                }
                if (Alignment.HasValue)
                {
                    tags.Add($"\\an{(int)Alignment}");
                }
                if (tags.Any())
                {
                    result = $"{{{string.Join(string.Empty, tags)}}} {result}";
                }
                return result;
            }
        }

        public Point? Pos { get; set; }
        public Alignment? Alignment { get; set; }

        public List<DialogueSyllableEffect> DialogueSyllableEffects { get; } = new();

        public static string Format
        {
            get
            {
                string[] formatParts =
                {
                    nameof(Layer),
                    nameof(Start),
                    nameof(End),
                    nameof(Style),
                    nameof(Name),
                    nameof(MarginL),
                    nameof(MarginR),
                    nameof(MarginV),
                    nameof(Effect),
                    nameof(Text)
                };
                return $"Format: {string.Join(", ", formatParts)}";
            }
        }

        public override string ToString()
        {
            string[] parts =
            {
                Layer.ToString(),
                Start.ToTimeString(),
                End.ToTimeString(),
                Style,
                Name ?? string.Empty,
                MarginL.ToString(),
                MarginR.ToString(),
                MarginV.ToString(),
                Effect ?? string.Empty,
                Text
            };
            return $"Dialogue: {string.Join(',', parts)}";
        }

    }
}

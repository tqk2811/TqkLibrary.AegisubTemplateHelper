using System;
using System.Collections.Generic;

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
        public string Text => string.Join(string.Empty, DialogueSyllableEffects);

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

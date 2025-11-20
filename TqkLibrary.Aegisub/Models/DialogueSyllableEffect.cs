using System;
using TqkLibrary.Aegisub.Enums;

namespace TqkLibrary.Aegisub.Models
{
    public class DialogueSyllableEffect
    {
        public required string Syllable { get; set; }
        public TimeSpan WordTime { get; set; }
        public SyllableEffect Effect { get; set; }
        public override string ToString()
        {
            if (Effect == SyllableEffect.None)
            {
                return Syllable;
            }
            else
            {
                double ds = Math.Round(WordTime.TotalMilliseconds / 10, 0);
                if (!string.IsNullOrWhiteSpace(Syllable) && ds < 1.0) ds = 1;
                return $"{{\\{Effect}{(int)ds}}}{Syllable}";
            }
        }
    }
}

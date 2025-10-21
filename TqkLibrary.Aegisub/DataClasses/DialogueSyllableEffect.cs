using System;
using TqkLibrary.Aegisub.Enums;

namespace TqkLibrary.Aegisub.DataClasses
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
                return $"{{\\{Effect}{(int)Math.Round(WordTime.TotalMilliseconds / 10, 0)}}}{Syllable}";
            }
        }
    }
}

using System;
using TqkLibrary.AegisubTemplateHelper.Enums;

namespace TqkLibrary.AegisubTemplateHelper.DataClasses
{
    public class DialogueWordEffect
    {
        public required string Text { get; set; }
        public TimeSpan WordTime { get; set; }
        public SyllableEffect Effect { get; set; }
        public override string ToString()
        {
            if (Effect == SyllableEffect.None)
            {
                return Text;
            }
            else
            {
                return $"{{\\{Effect}{(int)Math.Round(WordTime.TotalMilliseconds / 10, 0)}}}{Text}";
            }
        }
    }
}

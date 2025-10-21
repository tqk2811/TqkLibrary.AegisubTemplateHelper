using System.Drawing;
using TqkLibrary.Aegisub.Models;
using TqkLibrary.Aegisub.Interfaces;

namespace TqkLibrary.Aegisub
{
    public static class Extensions
    {
        public static string ToTimeString(this TimeSpan timeSpan)
        {
            return $"{timeSpan.Days * 24 + timeSpan.Hours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}";
        }
        public static string ToAssColor(this Color color)
        {
            var r = $"{color.B.ToString("X2")}{color.G.ToString("X2")}{color.R.ToString("X2")}";
            return r;
        }
    }
}

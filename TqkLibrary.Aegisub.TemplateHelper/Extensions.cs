using System.Drawing;
using TqkLibrary.Aegisub.TemplateHelper.DataClasses;
using TqkLibrary.Aegisub.TemplateHelper.Interfaces;

namespace TqkLibrary.Aegisub.TemplateHelper
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
        public static IEnumerable<IEnumerable<IWord>> SplitWords(this ISentence sentence, AssStyleData style, bool IsOneWordPerLine, int maxWidth)
        {
            if (IsOneWordPerLine)
            {
                foreach (var word in sentence.Words)
                {
                    yield return Enumerable.Repeat(word, 1);
                }
            }
            else
            {
                using Bitmap bitmap = new(1, 1);
                using Graphics graphics = Graphics.FromImage(bitmap);
                using Font font = new Font(style.Fontname, style.Fontsize);
                var size = graphics.MeasureString(sentence.Text, font);
                if (size.Width > maxWidth)
                {
                    int line = (int)Math.Ceiling(size.Width * 1.0 / maxWidth);
                    foreach (var item in sentence.Words.SplitWords(line))
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return sentence.Words;
                }
            }
        }
        public static IEnumerable<IEnumerable<T>> SplitWords<T>(this IReadOnlyList<T> words, int line)
        {
            int wordCountPerLine = (int)Math.Ceiling(words.Count * 1.0 / line);
            for (int i = 0; i < line - 1; i++)
            {
                yield return words.Skip(i * wordCountPerLine).Take(wordCountPerLine).ToList();
            }
            yield return words.Skip((line - 1) * wordCountPerLine).ToList();
        }
    }
}

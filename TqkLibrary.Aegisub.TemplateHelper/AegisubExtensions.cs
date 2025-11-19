using System.Drawing;
using System.Runtime.Versioning;
using TqkLibrary.Aegisub.Interfaces;
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.TemplateHelper
{
    public static class AegisubExtensions
    {
        public static AegisubWord Clone(this IAegisubWord aegisubWord)
            => new AegisubWord()
            {
                Start = aegisubWord.Start,
                End = aegisubWord.End,
                Word = aegisubWord.Word,
            };
        public static IEnumerable<AegisubWord> Clone(this IEnumerable<IAegisubWord> aegisubWords)
            => aegisubWords.Select(x => x.Clone());
        public static AegisubSentence Clone(this IAegisubSentence aegisubSentence)
            => new AegisubSentence()
            {
                Start = aegisubSentence.Start,
                End = aegisubSentence.End,
                Text = aegisubSentence.Text,
                Words = aegisubSentence.Words.Clone().ToList(),
            };



        [SupportedOSPlatform("windows")]
        public static IEnumerable<IAegisubSentence> SplitWords(this IAegisubSentence sentence, FontMeasurer fontMeasurer, int maxWidth)
        {
            {
                var size = fontMeasurer.MeasureString(sentence.Text);
                if (size.Width <= maxWidth)
                {
                    yield return sentence;
                }
            }

            int wasTakeCount = 0;
            for (int i = 0; i < sentence.Words.Count && wasTakeCount < sentence.Words.Count; i++)
            {
                if (i < wasTakeCount)
                    continue;
                var takewords = sentence.Words.Skip(wasTakeCount).Take(i - wasTakeCount + 1).ToList();
                string text = string.Join(" ", takewords);
                var size = fontMeasurer.MeasureString(text);
                if (size.Width > maxWidth || i == sentence.Words.Count - 1)//`tràn` hoặc `cuối` hoặc `tràn với 1 word`
                {
                    var words = sentence.Words.Skip(wasTakeCount);
                    bool isFullyWithOneWord = (i == wasTakeCount);//tràn với 1 word
                    if (size.Width > maxWidth)//tràn
                        words = words.Take(Math.Max(i - wasTakeCount, 1));//`tràn với 1 word` -> i - wasTakeCount = 0

                    var wordsList = words.ToList();
                    yield return new AegisubSentence()
                    {
                        Start = sentence.Words.First().Start,
                        End = sentence.Words.Last().End,
                        Words = wordsList.Clone().ToList(),
                        Text = string.Join(" ", wordsList.Select(x => x.Word))
                    };
                    if (isFullyWithOneWord) wasTakeCount = i + 1;
                    else wasTakeCount = i;
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

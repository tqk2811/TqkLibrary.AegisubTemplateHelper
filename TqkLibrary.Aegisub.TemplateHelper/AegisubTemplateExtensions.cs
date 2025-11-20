using System.Drawing;
using System.Runtime.Versioning;
using TqkLibrary.Aegisub.Interfaces;
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.TemplateHelper
{
    public static class AegisubTemplateExtensions
    {
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
            if (sentence.Words.Any())
            {
                int wasTakeCount = 0;
                for (int i = 0; i < sentence.Words.Count && wasTakeCount < sentence.Words.Count; i++)
                {
                    if (i < wasTakeCount)
                        continue;
                    var testTakewords = sentence.Words.Skip(wasTakeCount).Take(i - wasTakeCount + 1).ToList();
                    string text = string.Join(" ", testTakewords.Select(x => x.Word));
                    var size = fontMeasurer.MeasureString(text);
                    if (size.Width > maxWidth || i == sentence.Words.Count - 1)//`tràn` hoặc `cuối` hoặc `tràn với 1 word`
                    {
                        var realTakeWords = sentence.Words.Skip(wasTakeCount);
                        bool isFullyWithOneWord = (i == wasTakeCount);//tràn với 1 word
                        if (size.Width > maxWidth)//tràn
                            realTakeWords = realTakeWords.Take(Math.Max(i - wasTakeCount, 1));//`tràn với 1 word` -> i - wasTakeCount = 0

                        var wordsList = realTakeWords.ToList();
                        yield return new AegisubSentence()
                        {
                            Start = sentence.Start,
                            End = sentence.End,
                            Words = wordsList.Clone().ToList(),
                            Text = string.Join(" ", wordsList.Select(x => x.Word))
                        };
                        if (isFullyWithOneWord) wasTakeCount = i + 1;
                        else wasTakeCount = i;
                    }
                }
            }
            else
            {
                var words = sentence.Text.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
                int wasTakeCount = 0;
                for (int i = 0; i < words.Length && wasTakeCount < words.Length; i++)
                {
                    if (i < wasTakeCount)
                        continue;
                    var testTakewords = words.Skip(wasTakeCount).Take(i - wasTakeCount + 1).ToList();
                    string text = string.Join(" ", testTakewords);
                    var size = fontMeasurer.MeasureString(text);
                    if (size.Width > maxWidth || i == words.Length - 1)//`tràn` hoặc `cuối` hoặc `tràn với 1 word`
                    {
                        var realTakeWords = words.Skip(wasTakeCount);
                        bool isFullyWithOneWord = (i == wasTakeCount);//tràn với 1 word
                        if (size.Width > maxWidth)//tràn
                            realTakeWords = realTakeWords.Take(Math.Max(i - wasTakeCount, 1));//`tràn với 1 word` -> i - wasTakeCount = 0

                        var wordsList = realTakeWords.ToList();
                        yield return new AegisubSentence()
                        {
                            Start = sentence.Start,
                            End = sentence.End,
                            Words = [],
                            Text = string.Join(" ", wordsList)
                        };
                        if (isFullyWithOneWord) wasTakeCount = i + 1;
                        else wasTakeCount = i;
                    }
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

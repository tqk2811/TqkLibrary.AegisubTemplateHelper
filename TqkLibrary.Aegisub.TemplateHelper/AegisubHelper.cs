using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using TqkLibrary.Aegisub.Enums;
using TqkLibrary.Aegisub.Exceptions;
using TqkLibrary.Aegisub.Interfaces;
using TqkLibrary.Aegisub.Models;

namespace TqkLibrary.Aegisub.TemplateHelper
{
    public class AegisubHelper
    {
        public required string AegisubDir { get; set; }
        public required IEnumerable<IAegisubSentence> Sentences { get; set; }
        public required string TempSubFilePath { get; set; }
        public required string GenerateOutputSubFilePath { get; set; }
        public required AssScriptInfoData ScriptInfo { get; set; }//video size
        public required AssStyleData Style { get; set; }//mỗi style một line
        public required AegisubTemplateConfigureData Template { get; set; }
        public bool IsAllowSplitLine { get; set; } = false;
        public Point? AnchorPoint { get; set; }

        [SupportedOSPlatform("windows")]
        public virtual async Task GenerateAssFileAsync(CancellationToken cancellationToken = default)
        {
            AdvancedConfigure advancedConfigure = await Template.GetAdvancedConfigureAsync(cancellationToken) ?? new();

            IEnumerable<IAegisubSentence> sentences = Sentences;
            if (advancedConfigure.IsOneWordPerLine)
                sentences = sentences
                    .SelectMany(x => x.Words)
                    .Select(x => new AegisubSentence()
                    {
                        Start = x.Start,
                        End = x.End,
                        Text = x.Word,
                        Words = [x.Clone()],
                    });

            using FontMeasurer fontMeasurer = new FontMeasurer(Style);

            List<Dialogue> dialogues = new List<Dialogue>();
            foreach (IAegisubSentence sentence in sentences)
                dialogues.AddRange(ResolveTextOverflow(sentence, advancedConfigure, fontMeasurer));

            await RunGenerateAssFileAsync(dialogues, cancellationToken);
        }
#if GDI
        [SupportedOSPlatform("windows")]
#endif
        protected virtual IEnumerable<Dialogue> ResolveTextOverflow(IAegisubSentence sentence, AdvancedConfigure advancedConfigure, FontMeasurer fontMeasurer)
        {
            if (IsAllowSplitLine)
            {
                if (sentence.Text.Contains("\n")) throw new InvalidOperationException($"{nameof(IAegisubSentence.Text)} must not contains line break");
                if (!AnchorPoint.HasValue) throw new InvalidOperationException($"{nameof(AnchorPoint)} must have value for split text");

                int maxWidth = Style.GetMaxWidth(ScriptInfo.VideoSize.Width);
                List<IAegisubSentence> splitSentences = sentence.SplitWords(fontMeasurer, maxWidth).ToList();
                int lineCount = splitSentences.Count;
                float lineHeight = fontMeasurer.MeasureString(sentence.Text).Height;
                List<Point> AnchorPoints = new(lineCount);

                int vertical = ((int)Style.Alignment - 1) / 3;//0 bottom, 1 mid, 2 top
                //int horizontal = ((int)Style.Alignment - 1) % 3;//0 left, 1 center, 2 right

                for (int i = 0; i < lineCount; i++)
                {
                    float yOffset = 0;
                    if (lineCount > 1)
                    {
                        switch (vertical)
                        {
                            case 0://bottom
                                yOffset = -i * lineHeight;
                                break;
                            case 1://mid 
                                   //lineCount =2, i= 0 -> -0.5 ; i=1 -> +0.5
                                   //lineCount =3, i= 0 -> -1 ; i=1 -> 0 ; i=2 -> +1
                                yOffset = (i - (lineCount - 1) / 2.0f) * lineHeight;
                                break;
                            case 2://top
                                yOffset = i * lineHeight;
                                break;
                        }
                    }
                    AnchorPoints.Add(new Point(AnchorPoint.Value.X, AnchorPoint.Value.Y + (int)yOffset));
                }
                for (int i = 0; i < splitSentences.Count; i++)
                {
                    yield return GenDialogue(splitSentences[i], advancedConfigure.IsUseSyl, AnchorPoints[i]);
                }
            }
            else
            {
                yield return GenDialogue(sentence, advancedConfigure.IsUseSyl, AnchorPoint);
            }
        }

        [SupportedOSPlatform("windows")]
        protected virtual async Task RunGenerateAssFileAsync(IEnumerable<Dialogue> dialogues, CancellationToken cancellationToken = default)
        {
            using (StreamWriter streamWriter = new StreamWriter(TempSubFilePath, false, Encoding.UTF8))
            {
                streamWriter.WriteLine(ScriptInfo);//[Script Info]
                streamWriter.WriteLine();
                streamWriter.WriteLine(AssStyleData.Format);//[V4+ Styles]
                streamWriter.WriteLine(Style.Style);
                streamWriter.WriteLine();
                streamWriter.WriteLine(AssEventData.Event);//[Events]
                foreach (var line in await Template.GetSubCommentsAsync())
                {
                    streamWriter.WriteLine(line);
                }
                foreach (Dialogue dialogue in dialogues)
                {
                    streamWriter.WriteLine(dialogue);
                }
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AegisubDir, "aegisub-cli.exe"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AegisubDir,
            };
            processStartInfo.ArgumentList.Add("--automation");
            processStartInfo.ArgumentList.Add("kara-templater.lua");
            processStartInfo.ArgumentList.Add(TempSubFilePath);
            processStartInfo.ArgumentList.Add(GenerateOutputSubFilePath);
            processStartInfo.ArgumentList.Add("Apply karaoke template");
            using Process? process = Process.Start(processStartInfo);
            if (process is null)
                throw new InvalidOperationException($"Can't start process aegisub-cli");
            Task<string> t_stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> t_stderr = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                string stdout = await t_stdout;
                string stderr = await t_stderr;

                throw new AegisubProcessException()
                {
                    ExitCode = process.ExitCode,
                    StdOut = stdout,
                    StdErr = stderr
                };
            }
        }


        protected virtual Dialogue GenDialogue(IAegisubSentence sentence, bool isUseSyl, Point? pos = null)
        {
            Dialogue dialogue = new Dialogue()
            {
                Layer = 0,
                Start = sentence.Start,
                End = sentence.End,
                Style = Style.Name,
                Name = null,
                MarginL = 0,
                MarginR = 0,
                MarginV = 0,
                Effect = null,
                Pos = pos,
            };
            if (isUseSyl)
            {
                if (sentence.Words is null || !sentence.Words.Any())
                    throw new InvalidOperationException($"When use syl {nameof(IAegisubSentence)}.{nameof(sentence.Words)} must have values");

                var timeDelay = sentence.Words.First().Start - sentence.Start;
                if (timeDelay > TimeSpan.Zero)
                {
                    DialogueSyllableEffect delayEffect = new()
                    {
                        Syllable = "",
                        WordTime = timeDelay,
                        Effect = SyllableEffect.k,
                    };
                    dialogue.DialogueSyllableEffects.Add(delayEffect);
                }

                for (int j = 0; j < sentence.Words.Count; j++)
                {
                    var current = sentence.Words[j];
                    var next = sentence.Words.Skip(j + 1).FirstOrDefault();

                    DialogueSyllableEffect wordEffect = new()
                    {
                        Syllable = current.Word,
                        WordTime = current.End - current.Start,
                        Effect = SyllableEffect.k,
                    };
                    dialogue.DialogueSyllableEffects.Add(wordEffect);
                    if (next is not null)
                    {
                        //insert space
                        DialogueSyllableEffect spaceEffect = new()
                        {
                            Syllable = " ",
                            WordTime = next.Start - current.End,
                            Effect = SyllableEffect.k,
                        };
                        dialogue.DialogueSyllableEffects.Add(spaceEffect);
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(sentence.Text))
                    throw new InvalidOperationException($"When not use syl, {nameof(IAegisubSentence)}.{nameof(sentence.Text)} must have value");
                dialogue.DialogueSyllableEffects.Add(new DialogueSyllableEffect()
                {
                    Syllable = sentence.Text,
                    Effect = SyllableEffect.None,
                });
            }
            return dialogue;
        }
    }
}

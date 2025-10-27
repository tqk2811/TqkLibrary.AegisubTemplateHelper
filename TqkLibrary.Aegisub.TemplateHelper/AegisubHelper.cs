using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        public required IEnumerable<ISentence> Sentences { get; set; }
        public required string WorkingDir { get; set; }
        public required string GenerateOutputSubFilePath { get; set; }
        public required AssScriptInfoData ScriptInfo { get; set; }//video size
        public required AssStyleData Style { get; set; }
        public required AegisubTemplateConfigureData Template { get; set; }

        public virtual int MaxWidth => ScriptInfo.VideoSize.Width - Style.MarginL - Style.MarginR;
        public double Speed { get; set; } = 1.0;

        public virtual async Task GenerateAssFileAsync(CancellationToken cancellationToken = default)
        {
            AdvancedConfigure advancedConfigure = await Template.GetAdvancedConfigureAsync(cancellationToken) ?? new();
            SyllableEffect effect = advancedConfigure.IsUseSyl ? SyllableEffect.k : SyllableEffect.None;

            List<Dialogue> dialogues = new List<Dialogue>();
            foreach (ISentence sentence in Sentences)
            {
                if (sentence.Words.Any())
                {
                    var lines = sentence.SplitWords(Style, advancedConfigure.IsOneWordPerLine, MaxWidth)
                        .Select(x => x.ToList())
                        .Where(x => x.Any())
                        .ToList();
                    var allWords = lines.SelectMany(x => x).ToList();
                    int wordIndex = 0;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        var words = lines[i];

                        TimeSpan start = words.First().Start;
                        TimeSpan end = words.Last().End;
                        if (Speed != 1.0)
                        {
                            start = start / Speed;
                            end = end / Speed;
                        }
                        Dialogue dialogue = new Dialogue()
                        {
                            Layer = 0,
                            Start = start,
                            End = end,
                            Style = Style.Name,
                            Name = null,
                            MarginL = 0,
                            MarginR = 0,
                            MarginV = 0,
                            Effect = null,
                        };

                        for (int j = 0; j < words.Count; j++)
                        {
                            var current = words[j];
                            DialogueSyllableEffect wordEffect = new()
                            {
                                Syllable = current.Word,
                                WordTime = (current.End - current.Start) / Speed,
                                Effect = effect
                            };
                            dialogue.DialogueSyllableEffects.Add(wordEffect);

                            var next = allWords.Skip(wordIndex + 1).FirstOrDefault();
                            if (next is not null)
                            {
                                //insert space
                                DialogueSyllableEffect spaceEffect = new()
                                {
                                    Syllable = " ",
                                    WordTime = (next.Start - current.End) / Speed,
                                    Effect = effect
                                };
                                dialogue.DialogueSyllableEffects.Add(spaceEffect);
                            }
                            wordIndex++;
                        }

                        dialogues.Add(dialogue);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(sentence.Text))
                {
                    if (effect != SyllableEffect.None)
                        throw new InvalidOperationException();
                    string text = sentence.Text;

                    using Bitmap bitmap = new(1, 1);
                    using Graphics graphics = Graphics.FromImage(bitmap);
                    using Font font = new Font(Style.Fontname, Style.Fontsize);
                    var size = graphics.MeasureString(text, font);
                    if (size.Width > MaxWidth)
                    {
                        int line = (int)Math.Ceiling(size.Width * 1.0 / MaxWidth);
                        var str_words = sentence.Text.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        var lines = str_words.SplitWords(line);

                        text = string.Join("\\N", lines.Select(x => string.Join(" ", x)));
                    }

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
                    };
                    dialogue.DialogueSyllableEffects.Add(new DialogueSyllableEffect()
                    {
                        Syllable = text,
                        Effect = SyllableEffect.None,
                    });
                    dialogues.Add(dialogue);
                }
            }

            string tempSubPath = Path.Combine(WorkingDir, $"{Guid.NewGuid()}.ass");
            using (StreamWriter streamWriter = new StreamWriter(tempSubPath, false, Encoding.UTF8))
            {
                streamWriter.WriteLine(ScriptInfo);//[Script Info]
                streamWriter.WriteLine();
                streamWriter.WriteLine(Style);//[V4+ Styles]
                streamWriter.WriteLine();
                streamWriter.WriteLine(AssEventData.Event);//[Events]
                foreach (var line in await Template.GetSubCommentsAsync())
                {
                    streamWriter.WriteLine(line);
                }
                dialogues.ForEach(streamWriter.WriteLine);
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AegisubDir, "aegisub-cli.exe"),
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AegisubDir,
            };
            processStartInfo.ArgumentList.Add("--automation");
            processStartInfo.ArgumentList.Add("kara-templater.lua");
            processStartInfo.ArgumentList.Add(tempSubPath);
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

    }
}

using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TqkLibrary.Aegisub.JsonConverters;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTemplateConfigureData
    {
        public required string TemplateFilePath { get; set; }

        [JsonConverter(typeof(AegisubTemplateDictionaryConverter))]
        public Dictionary<string, object> FieldValues { get; set; } = new();

        [JsonConverter(typeof(AegisubTemplateDictionaryConverter))]
        public Dictionary<string, object> HiddenFieldValues { get; set; } = new();




        static readonly Regex regex_FieldValues = new Regex("\\[([A-z0-9]+);([A-z0-9]+)\\]");
        static readonly Regex regex_HiddenFieldValues = new Regex("\\[([A-z0-9]+);;([A-z0-9]+)\\]");
        public virtual async Task LoadFieldAsync()
        {
            var lines = await File.ReadAllLinesAsync(TemplateFilePath);
            lines = lines.Where(x => x.StartsWith("Comment")).ToArray();
            List<string> fields = new List<string>();

            List<string> currentFields = new();
            List<string> currentHiddenFields = new();
            foreach (var line in lines)
            {
                MatchCollection matchCollection = regex_FieldValues.Matches(line);
                foreach (Match match in matchCollection)
                {
                    string name = match.Groups[1].Value;
                    string defaultValue = match.Groups[2].Value;
                    Type type = AegisubTemplateDictionaryConverter.GetTypeHelper(name);

                    currentFields.Add(name);
                    if (!FieldValues.ContainsKey(name))
                    {
                        FieldValues[name] = AegisubTemplateDictionaryConverter.CreateDefaultTypeHelper(type, defaultValue);
                    }
                }

                matchCollection = regex_HiddenFieldValues.Matches(line);
                foreach (Match match in matchCollection)
                {
                    string name = match.Groups[1].Value;
                    string defaultValue = match.Groups[2].Value;
                    Type type = AegisubTemplateDictionaryConverter.GetTypeHelper(name);

                    currentHiddenFields.Add(name);
                    if (!HiddenFieldValues.ContainsKey(name))
                    {
                        HiddenFieldValues[name] = AegisubTemplateDictionaryConverter.CreateDefaultTypeHelper(type, defaultValue);
                    }
                }
            }

            List<string> exceptFields = FieldValues.Keys.Except(currentFields).ToList();
            foreach (var item in exceptFields)
            {
                FieldValues.Remove(item);
            }
            List<string> exceptHiddenFields = HiddenFieldValues.Keys.Except(currentHiddenFields).ToList();
            foreach (var item in exceptHiddenFields)
            {
                HiddenFieldValues.Remove(item);
            }
        }

        public virtual async Task<IEnumerable<string>> GetSubCommentsAsync()
        {
            if (!FieldValues.Any())
                await LoadFieldAsync();

            var lines = await File.ReadAllLinesAsync(TemplateFilePath);
            lines = lines
                .Where(x => x.StartsWith("Comment"))//WARNING NEVER TRIM STRING
                .Select(x =>
                {
                    string line = x;
                    foreach (var item in FieldValues)
                    {
#if DEBUG
                        var type = item.Value.GetType();
#endif
                        string replaced;
                        if (item.Value is System.Drawing.Color color)
                        {
                            replaced = color.ToAssColor();
                        }
                        else if (item.Value is float f)
                        {
                            replaced = f.ToString("F1");
                        }
                        else
                        {
                            replaced = item.Value.ToString()!;
                        }

                        line = Regex.Replace(line, $"\\[{Regex.Escape(item.Key)};([A-z0-9]+)\\]", replaced);
                    }
                    foreach (var item in HiddenFieldValues)
                    {
#if DEBUG
                        var type = item.Value.GetType();
#endif
                        string replaced;
                        if (item.Value is System.Drawing.Color color)
                        {
                            replaced = color.ToAssColor();
                        }
                        else if (item.Value is float f)
                        {
                            replaced = f.ToString("F1");
                        }
                        else
                        {
                            replaced = item.Value.ToString()!;
                        }

                        line = Regex.Replace(line, $"\\[{Regex.Escape(item.Key)};;([A-z0-9]+)\\]", replaced);
                    }
                    return line;
                })
                .ToArray();
            return lines;
        }

        public virtual async Task<AdvancedConfigure?> GetAdvancedConfigureAsync(CancellationToken cancellationToken = default)
        {
            var lines = await File.ReadAllLinesAsync(TemplateFilePath, cancellationToken);
            var line = lines.FirstOrDefault(x => x.StartsWith($"{nameof(AdvancedConfigure)}:"));
            if (!string.IsNullOrWhiteSpace(line))
            {
                var sub = line.Substring($"{nameof(AdvancedConfigure)}:".Length).Trim();
                return JsonConvert.DeserializeObject<AdvancedConfigure>(sub);
            }
            return null;
        }

    }
}

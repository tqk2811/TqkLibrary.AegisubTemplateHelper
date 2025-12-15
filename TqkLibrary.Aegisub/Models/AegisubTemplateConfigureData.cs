using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TqkLibrary.Aegisub.JsonConverters;

namespace TqkLibrary.Aegisub.Models
{
    public class AegisubTemplateConfigureData
    {
        public required string TemplateFilePath { get; set; }

        [JsonConverter(typeof(AegisubTemplateDictionaryFieldValueConverter))]
        public Dictionary<string, AegisubTemplateConfigureFieldValue> FieldValues { get; set; } = new();



        static readonly Regex regex_FieldValues = new Regex("\\[([A-z0-9]+);([A-z0-9]+)\\]");
        static readonly Regex regex_FieldValues_MinMax = new Regex("\\[([A-z0-9]+);([A-z0-9]+);([A-z0-9]+);([A-z0-9]+)\\]");
        public virtual async Task LoadFieldAsync()
        {
            var lines = await File.ReadAllLinesAsync(TemplateFilePath);
            lines = lines.Where(x => x.StartsWith("Comment")).ToArray();
            List<string> fields = new List<string>();

            List<string> currentFields = new();
            foreach (var line in lines)
            {
                MatchCollection matchCollection = regex_FieldValues.Matches(line);
                foreach (Match match in matchCollection)
                {
                    string name = match.Groups[1].Value;
                    string defaultValue = match.Groups[2].Value;
                    Type type = AegisubTemplateDictionaryFieldValueConverter.GetTypeHelper(name);

                    currentFields.Add(name);
                    if (!FieldValues.ContainsKey(name))
                    {
                        var value = new AegisubTemplateConfigureFieldValue()
                        {
                            Value = AegisubTemplateDictionaryFieldValueConverter.CreateDefaultTypeHelper(type, defaultValue)
                        };
                        FieldValues[name] = value;
                    }
                }
                matchCollection = regex_FieldValues_MinMax.Matches(line);
                foreach (Match match in matchCollection)
                {
                    string name = match.Groups[1].Value;
                    string defaultValue = match.Groups[2].Value;
                    string minValue = match.Groups[3].Value;
                    string maxValue = match.Groups[4].Value;
                    Type type = AegisubTemplateDictionaryFieldValueConverter.GetTypeHelper(name);

                    currentFields.Add(name);
                    if (!FieldValues.ContainsKey(name))
                    {
                        var value = new AegisubTemplateConfigureFieldValue()
                        {
                            Value = AegisubTemplateDictionaryFieldValueConverter.CreateDefaultTypeHelper(type, defaultValue),
                            MinValue = AegisubTemplateDictionaryFieldValueConverter.CreateDefaultTypeHelper(type, minValue),
                            MaxValue = AegisubTemplateDictionaryFieldValueConverter.CreateDefaultTypeHelper(type, maxValue),
                        };
                        FieldValues[name] = value;
                    }

                }
            }

            List<string> exceptFields = FieldValues.Keys.Except(currentFields).ToList();
            foreach (var item in exceptFields)
            {
                FieldValues.Remove(item);
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
                        if (item.Value.Value is System.Drawing.Color color)
                        {
                            replaced = color.ToAssColor();
                        }
                        else if (item.Value.Value is float f)
                        {
                            replaced = f.ToString("F1");
                        }
                        else
                        {
                            replaced = item.Value.ToString()!;
                        }

                        line = Regex.Replace(line, $"\\[{Regex.Escape(item.Key)};([A-z0-9]+)\\]", replaced);
                    }
                    return line;
                })
                .ToArray();
            return lines;
        }


        public virtual async Task<IReadOnlyList<string>> GetDataLinesAsync(string headerName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(headerName)) throw new ArgumentNullException(nameof(headerName));
            var lines = await File.ReadAllLinesAsync(TemplateFilePath, cancellationToken);
            return lines.Where(x => x.StartsWith(headerName)).ToList();
        }
        public virtual async Task<T?> GetDataLineAsync<T>(CancellationToken cancellationToken = default)
        {
            string headerName = $"{typeof(T).Name}:";
            var lines = await GetDataLinesAsync(headerName, cancellationToken);
            string? line = lines.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(line))
            {
                var sub = line.Substring(headerName.Length).Trim();
                return JsonConvert.DeserializeObject<T>(sub);
            }
            return default(T);
        }

        public virtual Task<AdvancedConfigure?> GetAdvancedConfigureAsync(CancellationToken cancellationToken = default)
            => GetDataLineAsync<AdvancedConfigure>(cancellationToken);

    }
}

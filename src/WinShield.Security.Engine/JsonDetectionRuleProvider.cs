using System.Text.Json;
using System.Text.Json.Serialization;
using WinShield.Domain;

namespace WinShield.Security.Engine;

public sealed class JsonDetectionRuleProvider : IDetectionRuleProvider
{
    private readonly string _rulesDirectory;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public JsonDetectionRuleProvider(string rulesDirectory)
    {
        _rulesDirectory = rulesDirectory;
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<IReadOnlyList<DetectionRule>> LoadRulesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_rulesDirectory))
        {
            return Array.Empty<DetectionRule>();
        }

        var rules = new List<DetectionRule>();
        foreach (var file in Directory.EnumerateFiles(_rulesDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            await using var stream = File.OpenRead(file);
            var rule = await JsonSerializer.DeserializeAsync<DetectionRule>(stream, _options, cancellationToken);
            if (rule is not null)
            {
                rules.Add(rule);
            }
        }

        return rules;
    }
}

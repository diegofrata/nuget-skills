namespace NuGetSkills.Services;

public record ParsedFrontmatter(Dictionary<string, string> Fields, string Body)
{
    public string? GetField(string key) =>
        Fields.TryGetValue(key, out var value) ? value : null;
}

public static class FrontmatterParser
{
    public static ParsedFrontmatter Parse(string content)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = content.Split('\n');
        var inFrontmatter = false;
        var bodyStart = 0;

        var frontmatterClosed = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].Trim();

            if (!inFrontmatter)
            {
                if (trimmed == "---")
                    inFrontmatter = true;
                continue;
            }

            if (trimmed == "---")
            {
                bodyStart = i + 1;
                frontmatterClosed = true;
                break;
            }

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex <= 0)
                continue;

            var key = trimmed[..colonIndex].Trim();
            var value = trimmed[(colonIndex + 1)..].Trim();
            fields[key] = value;
        }

        // If frontmatter was opened but never closed, treat entire content as body
        if (inFrontmatter && !frontmatterClosed)
        {
            fields.Clear();
            bodyStart = 0;
        }

        var body = string.Join('\n', lines[bodyStart..]).TrimStart('\n');
        return new ParsedFrontmatter(fields, body);
    }
}

using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class FrontmatterParserTests
{
    [Fact]
    public void Parse_ExtractsFieldsAndBody()
    {
        var content = """
            ---
            name: test-skill
            description: A test skill
            ---

            # Content
            Some body text.
            """;

        var result = FrontmatterParser.Parse(content);

        Assert.Equal("test-skill", result.GetField("name"));
        Assert.Equal("A test skill", result.GetField("description"));
        Assert.Contains("# Content", result.Body);
        Assert.Contains("Some body text.", result.Body);
    }

    [Fact]
    public void Parse_NoFrontmatter_ReturnsEmptyFieldsAndFullBody()
    {
        var content = "# Just content\nNo frontmatter.";

        var result = FrontmatterParser.Parse(content);

        Assert.Empty(result.Fields);
        Assert.Contains("# Just content", result.Body);
    }

    [Fact]
    public void Parse_EmptyFrontmatter_ReturnsEmptyFields()
    {
        var content = "---\n---\n\n# Body";

        var result = FrontmatterParser.Parse(content);

        Assert.Empty(result.Fields);
        Assert.Contains("# Body", result.Body);
    }

    [Fact]
    public void Parse_CaseInsensitiveFieldLookup()
    {
        var content = "---\nName: upper\nDESCRIPTION: also upper\n---\n";

        var result = FrontmatterParser.Parse(content);

        Assert.Equal("upper", result.GetField("name"));
        Assert.Equal("also upper", result.GetField("description"));
        Assert.Equal("upper", result.GetField("NAME"));
    }

    [Fact]
    public void Parse_ExtraFields_AllPreserved()
    {
        var content = "---\nname: test\nauthor: someone\nversion: 1.0\n---\n";

        var result = FrontmatterParser.Parse(content);

        Assert.Equal("test", result.GetField("name"));
        Assert.Equal("someone", result.GetField("author"));
        Assert.Equal("1.0", result.GetField("version"));
    }

    [Fact]
    public void Parse_ColonInValue_PreservesFullValue()
    {
        var content = "---\ndescription: Use this: it works\n---\n";

        var result = FrontmatterParser.Parse(content);

        Assert.Equal("Use this: it works", result.GetField("description"));
    }

    [Fact]
    public void GetField_Missing_ReturnsNull()
    {
        var content = "---\nname: test\n---\n";

        var result = FrontmatterParser.Parse(content);

        Assert.Null(result.GetField("nonexistent"));
    }

    [Fact]
    public void Parse_BodyStripsLeadingNewlines()
    {
        var content = "---\nname: test\n---\n\n\n# Body";

        var result = FrontmatterParser.Parse(content);

        Assert.StartsWith("# Body", result.Body);
    }
}

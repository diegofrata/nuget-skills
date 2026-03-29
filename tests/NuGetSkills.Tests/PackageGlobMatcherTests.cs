using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class PackageGlobMatcherTests
{
    [Fact]
    public void IsMatch_NullField_ReturnsTrue()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", null));
    }

    [Fact]
    public void IsMatch_EmptyField_ReturnsTrue()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", ""));
    }

    [Fact]
    public void IsMatch_WhitespaceField_ReturnsTrue()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "   "));
    }

    [Fact]
    public void IsMatch_ExactMatch()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "Contoso.Core"));
    }

    [Fact]
    public void IsMatch_ExactMatch_CaseInsensitive()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "contoso.core"));
    }

    [Fact]
    public void IsMatch_WildcardSuffix()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Http.Client", "Contoso.Http*"));
    }

    [Fact]
    public void IsMatch_WildcardSuffix_MatchesExact()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Http", "Contoso.Http*"));
    }

    [Fact]
    public void IsMatch_WildcardPrefix()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "*.Core"));
    }

    [Fact]
    public void IsMatch_WildcardMiddle()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Http.Client", "Contoso.*.Client"));
    }

    [Fact]
    public void IsMatch_WildcardOnly_MatchesEverything()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Anything.At.All", "*"));
    }

    [Fact]
    public void IsMatch_NoMatch()
    {
        Assert.False(PackageGlobMatcher.IsMatch("Contoso.Core", "Contoso.Http*"));
    }

    [Fact]
    public void IsMatch_NoMatch_PartialId()
    {
        Assert.False(PackageGlobMatcher.IsMatch("Contoso.CoreExtras", "Contoso.Core"));
    }

    [Fact]
    public void IsMatch_MultiplePatterns_FirstMatches()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Http", "Contoso.Http*, Contoso.Core"));
    }

    [Fact]
    public void IsMatch_MultiplePatterns_SecondMatches()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "Contoso.Http*, Contoso.Core"));
    }

    [Fact]
    public void IsMatch_MultiplePatterns_NoneMatch()
    {
        Assert.False(PackageGlobMatcher.IsMatch("Contoso.Logging", "Contoso.Http*, Contoso.Core"));
    }

    [Fact]
    public void IsMatch_WhitespaceInPatterns_Trimmed()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", " Contoso.Http* , Contoso.Core "));
    }

    [Fact]
    public void IsMatch_EmptyPatternsBetweenCommas_Ignored()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Core", "Contoso.Http*,,Contoso.Core"));
    }

    [Fact]
    public void IsMatch_MultipleWildcards()
    {
        Assert.True(PackageGlobMatcher.IsMatch("Contoso.Http.Client", "*Http*"));
    }

    [Fact]
    public void IsMatch_DotsAreNotRegexWildcards()
    {
        // "Contoso.Core" should not match "ContosoXCore" — dots are literal
        Assert.False(PackageGlobMatcher.IsMatch("ContosoXCore", "Contoso.Core"));
    }
}

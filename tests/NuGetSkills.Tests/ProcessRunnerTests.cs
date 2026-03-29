using NuGetSkills.Services;

namespace NuGetSkills.Tests;

public class ProcessRunnerTests
{
    [Fact]
    public async Task RunAsync_SuccessfulCommand_ReturnsSuccess()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "--version", cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Stdout));
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task RunAsync_FailingCommand_ReturnsFailure()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "nonexistent-command-xyz", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task RunAsync_NonExistentBinary_ReturnsFailure()
    {
        var result = await ProcessRunner.RunAsync("this-binary-does-not-exist-xyz", "", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal(-1, result.ExitCode);
    }

    [Fact]
    public async Task RunAsync_CapturesStdout()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "--version", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Matches(@"\d+\.\d+\.\d+", result.Stdout.Trim());
    }

    [Fact]
    public async Task RunAsync_CapturesStderr()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "nonexistent-command-xyz", cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrEmpty(result.Stderr));
    }

    [Fact]
    public async Task RunAsync_WithTimeout_CompletesBeforeTimeout()
    {
        var result = await ProcessRunner.RunAsync("dotnet", "--version", TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task RunAsync_Cancellation_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ProcessRunner.RunAsync("dotnet", "--version", cancellationToken: cts.Token));
    }
}

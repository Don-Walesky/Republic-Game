namespace Republic.App;

/// <summary>
/// Application entry point for the Wave 0 foundation host.
/// </summary>
public static class Program
{
    /// <summary>
    /// Starts the repository's deterministic bootstrap host.
    /// </summary>
    public static async Task<int> Main(string[] args)
    {
        _ = args;
        var bootstrapper = new ApplicationBootstrapper();
        var application = bootstrapper.Bootstrap();
        await application.RunAsync().ConfigureAwait(false);
        return 0;
    }
}

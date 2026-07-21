namespace Republic.App;

using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Engine;

/// <summary>
/// Top-level application facade used by Program.cs.
/// </summary>
public sealed class RepublicApplication
{
    private readonly RepublicConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly RepublicEngine _engine;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicApplication"/> class.
    /// </summary>
    public RepublicApplication(RepublicConfiguration configuration, ILogger logger, RepublicEngine engine)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <summary>
    /// Executes the repository's Wave 0 bootstrap path.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Republic bootstrap starting.");
        await _engine.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await _engine.RunAsync(
            _configuration.Engine.StartupFrameCount,
            TimeSpan.FromMilliseconds(_configuration.Engine.FrameDeltaMilliseconds),
            cancellationToken).ConfigureAwait(false);
        _logger.LogInfo("Republic bootstrap completed.");
    }
}

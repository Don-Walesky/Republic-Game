namespace Republic.Core.Engine;

using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.Time;
using Republic.Core.World;

/// <summary>
/// Composes the Wave 0 runtime loop.
/// </summary>
public sealed class RepublicEngine
{
    private readonly EngineConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly ITimeSystem _timeSystem;
    private readonly IEventBus _eventBus;
    private readonly IWorldManager _worldManager;
    private bool _initialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepublicEngine"/> class.
    /// </summary>
    public RepublicEngine(
        EngineConfiguration configuration,
        ILogger logger,
        ITimeSystem timeSystem,
        IEventBus eventBus,
        IWorldManager worldManager)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeSystem = timeSystem ?? throw new ArgumentNullException(nameof(timeSystem));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _worldManager = worldManager ?? throw new ArgumentNullException(nameof(worldManager));
    }

    /// <summary>
    /// Initializes the runtime world shell.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _worldManager.CreateAsync(_configuration.WorldName, cancellationToken).ConfigureAwait(false);
        _initialized = true;
        _logger.LogInfo("Republic engine initialized.");
    }

    /// <summary>
    /// Runs a finite number of deterministic frames.
    /// </summary>
    public async Task RunAsync(int frames, TimeSpan realFrameDelta, CancellationToken cancellationToken = default)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Engine must be initialized before running.");
        }

        for (var frame = 0; frame < frames; frame++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _timeSystem.TickAsync(realFrameDelta.TotalSeconds, cancellationToken).ConfigureAwait(false);
            _worldManager.AdvanceTo(_timeSystem.CurrentTick);
            await _eventBus.ProcessQueuedEventsAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug($"Processed frame {frame + 1} at tick {_timeSystem.CurrentTick}.");
        }
    }
}

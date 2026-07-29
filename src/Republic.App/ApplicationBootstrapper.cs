namespace Republic.App;

using Microsoft.Extensions.DependencyInjection;
using Republic.Core.Configuration;
using Republic.Core.Diagnostics;
using Republic.Core.Engine;
using Republic.Core.Events;
using Republic.Core.Persistence;
using Republic.Core.Time;
using Republic.Core.World;

/// <summary>
/// Builds the Wave 0 service container.
/// </summary>
public sealed class ApplicationBootstrapper
{
    /// <summary>
    /// Creates a fully configured application instance.
    /// </summary>
    public RepublicApplication Bootstrap()
    {
        var configurationPath = Path.Combine(AppContext.BaseDirectory, "Config", "defaults.json");
        var configurationManager = new JsonConfigurationManager();
        var configuration = configurationManager.Load(configurationPath);
        var services = new ServiceCollection();

        services.AddSingleton<IConfigurationManager>(configurationManager);
        services.AddSingleton(configuration);
        services.AddSingleton(configuration.Engine);
        services.AddSingleton(configuration.Time);
        services.AddSingleton(configuration.Persistence);
        services.AddSingleton(configuration.Logging);
        services.AddSingleton<ILogger>(serviceProvider =>
        {
            var loggingConfiguration = serviceProvider.GetRequiredService<LoggingConfiguration>();
            var sinks = new List<ILogSink>();
            if (loggingConfiguration.ConsoleEnabled)
            {
                sinks.Add(new ConsoleLogSink());
            }

            if (loggingConfiguration.FileEnabled)
            {
                sinks.Add(new FileLogSink(loggingConfiguration.FilePath));
            }

            return new Logger(loggingConfiguration, sinks);
        });
        services.AddSingleton(new EventBusOptions());
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<ITimeSystem, TimeSystem>();
        services.AddSingleton<IStateSerializer, JsonStateSerializer>();
        services.AddSingleton<FileSaveStore>();
        services.AddSingleton<IWorldManager, WorldManager>();
        services.AddSingleton<RepublicEngine>();
        services.AddSingleton<RepublicApplication>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<RepublicApplication>();
    }
}

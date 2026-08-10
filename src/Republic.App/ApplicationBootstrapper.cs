namespace Republic.App;

using Microsoft.Extensions.DependencyInjection;
using Republic.Core.Configuration;
using Republic.Core.Cabinet.Services;
using Republic.Core.Crises.Services;
using Republic.Core.Decisions.Services;
using Republic.Core.Demographics.Classes.Services;
using Republic.Core.Diagnostics;
using Republic.Core.Diplomacy.Services;
using Republic.Core.Economy.Budget.Services;
using Republic.Core.Economy.Trade.Services;
using Republic.Core.Elections.Services;
using Republic.Core.Engine;
using Republic.Core.Events;
using Republic.Core.Government.Services;
using Republic.Core.Intelligence.Services;
using Republic.Core.Legislature.Services;
using Republic.Core.Media.Services;
using Republic.Core.Persistence;
using Republic.Core.Persistence.Services;
using Republic.Core.Scenarios.Services;
using Republic.Core.Tasks.Services;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Services;
using Republic.Core.Workspace.Services;

/// <summary>
/// Builds the service container.
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
        services.AddSingleton<ITaskQueueManager, TaskQueueManager>();
        services.AddSingleton<IStateSerializer, JsonStateSerializer>();
        services.AddSingleton<FileSaveStore>();
        services.AddSingleton<ISaveGameManager, SaveGameManager>();
        services.AddSingleton<ICountryService, CountryService>();
        services.AddSingleton<IGeographyService, GeographyService>();
        services.AddSingleton<IResourceService, ResourceService>();
        services.AddSingleton<IDemographicService, DemographicService>();
        services.AddSingleton<IEconomicService, EconomicService>();
        services.AddSingleton<IPoliticalCultureService, PoliticalCultureService>();
        services.AddSingleton<IWorldManager, WorldManager>();
        services.AddSingleton<IDecisionEngine, DecisionEngine>();
        services.AddSingleton<ICrisisTriggerEngine, CrisisTriggerEngine>();
        services.AddSingleton<IInterPlayerWarfareService, InterPlayerWarfareService>();
        services.AddSingleton<IDiplomacyService, DiplomacyService>();
        services.AddSingleton<ICabinetService, CabinetService>();
        services.AddSingleton<IIntelligenceService, IntelligenceService>();
        services.AddSingleton<ILegislatureService, LegislatureService>();
        services.AddSingleton<IScenarioBootstrapper, ScenarioBootstrapper>();
        services.AddSingleton<IBudgetService, BudgetService>();
        services.AddSingleton<IElectionService, ElectionService>();
        services.AddSingleton<IRivalAIService, RivalAIService>();
        services.AddSingleton<ITradeMarketService, TradeMarketService>();
        services.AddSingleton<IGovernmentReformService, GovernmentReformService>();
        services.AddSingleton<IPressConferenceService, PressConferenceService>();
        services.AddSingleton<IDemographicClassService, DemographicClassService>();
        services.AddSingleton<IVisitorService, VisitorService>();
        services.AddSingleton<IPhoneService, PhoneService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<INewsService, NewsService>();
        services.AddSingleton<ICalendarService, CalendarService>();
        services.AddSingleton<IWorkspaceManager, WorkspaceManager>();
        services.AddSingleton<RepublicEngine>();
        services.AddSingleton<RepublicApplication>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<RepublicApplication>();
    }
}

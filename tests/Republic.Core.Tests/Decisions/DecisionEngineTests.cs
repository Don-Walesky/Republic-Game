namespace Republic.Core.Tests.Decisions;

using Republic.Core.Decisions.Models;
using Republic.Core.Decisions.Services;
using Republic.Core.Events;
using Republic.Core.World;
using Republic.Core.World.Models;

public sealed class DecisionEngineTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());
    private readonly TestLogger _logger = new();
    private readonly WorldManager _world;

    public DecisionEngineTests()
    {
        _world = new WorldManager(_eventBus, _logger);
        _world.CreateAsync("Test World").GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ExecuteDecision_AppliesPolicyEffectsToWorld()
    {
        var engine = new DecisionEngine(_world, _eventBus, _logger);
        var country = _world.Countries.RegisterCountry(new Country { Id = "c1", Name = "Aethel", BaselineStability = 50.0 });
        var faction = _world.PoliticalCulture.RegisterFaction(new Faction { Id = "f1", Name = "Patriots", ApprovalRating = 40.0 });

        var context = new DecisionContext
        {
            Id = "d1",
            Title = "Emergency Infrastructure Plan",
            Options = new List<DecisionOption>
            {
                new DecisionOption
                {
                    Id = "opt-approve",
                    Label = "Approve Infrastructure Package",
                    TreasuryCost = 1_000_000,
                    Effects = new List<PolicyEffect>
                    {
                        new PolicyEffect { TargetMetric = "Stability", TargetId = "c1", DeltaValue = 15.0 },
                        new PolicyEffect { TargetMetric = "Approval", TargetId = "f1", DeltaValue = 10.0 },
                        new PolicyEffect { TargetMetric = "Happiness", DeltaValue = 5.0 },
                    }
                }
            }
        };

        engine.RegisterDecision(context);
        Assert.Single(engine.GetPendingDecisions());

        var success = await engine.ExecuteDecisionAsync("d1", "opt-approve");

        Assert.True(success);
        Assert.Empty(engine.GetPendingDecisions());
        Assert.Equal(65.0, country.BaselineStability);
        Assert.Equal(50.0, faction.ApprovalRating);
    }

    [Fact]
    public async Task DirectEnactPolicy_AppliesDecreeImmediately()
    {
        var engine = new DecisionEngine(_world, _eventBus, _logger);
        var initialTreasury = _world.Economic.GetIndicators().TreasuryBalance;

        await engine.DirectEnactPolicyAsync("Emergency Economic Relief", new List<PolicyEffect>
        {
            new PolicyEffect { TargetMetric = "Treasury", DeltaValue = 500_000 }
        });

        Assert.Equal(initialTreasury + 500_000, _world.Economic.GetIndicators().TreasuryBalance);
    }
}

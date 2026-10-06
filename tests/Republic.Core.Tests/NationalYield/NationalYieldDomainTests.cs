namespace Republic.Core.Tests.NationalYield;

using System.Reflection;
using System.Text.Json;
using Republic.Core.Diagnostics;
using Republic.Core.Events;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World;
using Republic.Core.World.Models;
using Republic.Core.World.Services;
using Xunit;

/// <summary>
/// Unit tests for the National Yield domain model (Step 4A).
/// Validates domain representation, independent instances per country, complete category coverage,
/// food exclusion, serialization round-trip, and capability preservation.
/// </summary>
public sealed class NationalYieldDomainTests
{
    private readonly EventBus _eventBus = new(new EventBusOptions(), new TestLogger());
    private readonly TestLogger _logger = new();

    [Fact]
    public void Test1_NewlyCreatedCountry_HasValidNationalYieldState()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Republic of Valerius", clock.CurrentTime);

        Assert.NotNull(country.Yield);
        Assert.NotNull(country.Yield.Economic);
        Assert.NotNull(country.Yield.HumanKnowledge);
        Assert.NotNull(country.Yield.State);
        Assert.NotNull(country.Yield.ResourceOutput);

        // Sensible neutral defaults for all 14 categories at creation
        Assert.Equal(0.0, country.Yield.RepuTreasuryRevenue);
        Assert.Equal(0.0, country.Yield.IndustrialCapacity);
        Assert.Equal(0.0, country.Yield.EnergyCapacity);
        Assert.Equal(0.0, country.Yield.InfrastructureCapacity);
        Assert.Equal(0.0, country.Yield.FinancialCapacity);

        Assert.Equal(0.0, country.Yield.HumanCapital);
        Assert.Equal(0.0, country.Yield.ScienceCapacity);
        Assert.Equal(0.0, country.Yield.InnovationCapacity);

        Assert.Equal(0.0, country.Yield.AdministrativeCapacity);
        Assert.Equal(0.0, country.Yield.SecurityCapacity);
        Assert.Equal(0.0, country.Yield.IntelligenceCapacity);
        Assert.Equal(0.0, country.Yield.MilitaryReadiness);
        Assert.Equal(0.0, country.Yield.DiplomaticCapacity);

        Assert.Equal(0.0, country.Yield.NaturalResourceOutput);
    }

    [Fact]
    public void Test2_EveryCountry_ReceivesOwnIndependentNationalYieldInstance()
    {
        var clock = RepublicClock.CreateControlled();
        var countryService = new CountryService(_eventBus, _logger, clock);

        var countryA = countryService.FoundCountry("Solaris");
        var countryB = countryService.FoundCountry("Lunaris");

        // Independent references
        Assert.NotSame(countryA.Yield, countryB.Yield);
        Assert.NotSame(countryA.Yield.Economic, countryB.Yield.Economic);
        Assert.NotSame(countryA.Yield.HumanKnowledge, countryB.Yield.HumanKnowledge);
        Assert.NotSame(countryA.Yield.State, countryB.Yield.State);
        Assert.NotSame(countryA.Yield.ResourceOutput, countryB.Yield.ResourceOutput);

        // Even when passing an existing instance to Found(), it is cloned for safety
        var customYield = new NationalYield { IndustrialCapacity = 50.0 };
        var countryC = Country.Found("Terranis", clock.CurrentTime, yield: customYield);

        Assert.NotSame(customYield, countryC.Yield);
        Assert.Equal(50.0, countryC.Yield.IndustrialCapacity);
    }

    [Fact]
    public void Test3_UpdatingCountryAYield_DoesNotModifyCountryBYield()
    {
        var clock = RepublicClock.CreateControlled();
        var countryA = Country.Found("Republic Alpha", clock.CurrentTime);
        var countryB = Country.Found("Republic Beta", clock.CurrentTime);

        // Mutate Country A across all dimensions
        countryA.Yield.RepuTreasuryRevenue = 150_000.0;
        countryA.Yield.IndustrialCapacity = 75.5;
        countryA.Yield.EnergyCapacity = 90.0;
        countryA.Yield.InfrastructureCapacity = 82.0;
        countryA.Yield.FinancialCapacity = 65.0;

        countryA.Yield.HumanCapital = 70.0;
        countryA.Yield.ScienceCapacity = 85.0;
        countryA.Yield.InnovationCapacity = 60.0;

        countryA.Yield.AdministrativeCapacity = 95.0;
        countryA.Yield.SecurityCapacity = 88.0;
        countryA.Yield.IntelligenceCapacity = 72.0;
        countryA.Yield.MilitaryReadiness = 80.0;
        countryA.Yield.DiplomaticCapacity = 68.0;

        countryA.Yield.NaturalResourceOutput = 45.0;

        // Verify Country B remains untouched
        Assert.Equal(0.0, countryB.Yield.RepuTreasuryRevenue);
        Assert.Equal(0.0, countryB.Yield.IndustrialCapacity);
        Assert.Equal(0.0, countryB.Yield.EnergyCapacity);
        Assert.Equal(0.0, countryB.Yield.InfrastructureCapacity);
        Assert.Equal(0.0, countryB.Yield.FinancialCapacity);

        Assert.Equal(0.0, countryB.Yield.HumanCapital);
        Assert.Equal(0.0, countryB.Yield.ScienceCapacity);
        Assert.Equal(0.0, countryB.Yield.InnovationCapacity);

        Assert.Equal(0.0, countryB.Yield.AdministrativeCapacity);
        Assert.Equal(0.0, countryB.Yield.SecurityCapacity);
        Assert.Equal(0.0, countryB.Yield.IntelligenceCapacity);
        Assert.Equal(0.0, countryB.Yield.MilitaryReadiness);
        Assert.Equal(0.0, countryB.Yield.DiplomaticCapacity);

        Assert.Equal(0.0, countryB.Yield.NaturalResourceOutput);

        // Verify Country A holds the new values
        Assert.Equal(150_000.0, countryA.Yield.RepuTreasuryRevenue);
        Assert.Equal(75.5, countryA.Yield.IndustrialCapacity);
        Assert.Equal(85.0, countryA.Yield.ScienceCapacity);
        Assert.Equal(80.0, countryA.Yield.MilitaryReadiness);
        Assert.Equal(45.0, countryA.Yield.NaturalResourceOutput);
    }

    [Fact]
    public void Test4_AllFourteenNationalYieldCategories_AreRepresentedAndAccessible()
    {
        var categories = Enum.GetValues<NationalYieldCategory>();
        Assert.Equal(14, categories.Length);

        var yield = new NationalYield();

        // 1. Economic / Productive Capacity (5 categories)
        yield.SetCapacity(NationalYieldCategory.RepuTreasuryRevenue, 1000.0);
        yield.SetCapacity(NationalYieldCategory.IndustrialCapacity, 50.0);
        yield.SetCapacity(NationalYieldCategory.EnergyCapacity, 60.0);
        yield.SetCapacity(NationalYieldCategory.InfrastructureCapacity, 70.0);
        yield.SetCapacity(NationalYieldCategory.FinancialCapacity, 80.0);

        // 2. Human / Knowledge Capacity (3 categories)
        yield.SetCapacity(NationalYieldCategory.HumanCapital, 55.0);
        yield.SetCapacity(NationalYieldCategory.ScienceCapacity, 65.0);
        yield.SetCapacity(NationalYieldCategory.InnovationCapacity, 75.0);

        // 3. State Capacity (5 categories)
        yield.SetCapacity(NationalYieldCategory.AdministrativeCapacity, 85.0);
        yield.SetCapacity(NationalYieldCategory.SecurityCapacity, 90.0);
        yield.SetCapacity(NationalYieldCategory.IntelligenceCapacity, 40.0);
        yield.SetCapacity(NationalYieldCategory.MilitaryReadiness, 45.0);
        yield.SetCapacity(NationalYieldCategory.DiplomaticCapacity, 95.0);

        // 4. Resource Output (1 category)
        yield.SetCapacity(NationalYieldCategory.NaturalResourceOutput, 30.0);

        // Verify category accessors return exact values
        Assert.Equal(1000.0, yield.GetCapacity(NationalYieldCategory.RepuTreasuryRevenue));
        Assert.Equal(50.0, yield.GetCapacity(NationalYieldCategory.IndustrialCapacity));
        Assert.Equal(60.0, yield.GetCapacity(NationalYieldCategory.EnergyCapacity));
        Assert.Equal(70.0, yield.GetCapacity(NationalYieldCategory.InfrastructureCapacity));
        Assert.Equal(80.0, yield.GetCapacity(NationalYieldCategory.FinancialCapacity));

        Assert.Equal(55.0, yield.GetCapacity(NationalYieldCategory.HumanCapital));
        Assert.Equal(65.0, yield.GetCapacity(NationalYieldCategory.ScienceCapacity));
        Assert.Equal(75.0, yield.GetCapacity(NationalYieldCategory.InnovationCapacity));

        Assert.Equal(85.0, yield.GetCapacity(NationalYieldCategory.AdministrativeCapacity));
        Assert.Equal(90.0, yield.GetCapacity(NationalYieldCategory.SecurityCapacity));
        Assert.Equal(40.0, yield.GetCapacity(NationalYieldCategory.IntelligenceCapacity));
        Assert.Equal(45.0, yield.GetCapacity(NationalYieldCategory.MilitaryReadiness));
        Assert.Equal(95.0, yield.GetCapacity(NationalYieldCategory.DiplomaticCapacity));

        Assert.Equal(30.0, yield.GetCapacity(NationalYieldCategory.NaturalResourceOutput));

        // Verify direct convenience properties match
        Assert.Equal(1000.0, yield.RepuTreasuryRevenue);
        Assert.Equal(50.0, yield.IndustrialCapacity);
        Assert.Equal(60.0, yield.EnergyCapacity);
        Assert.Equal(70.0, yield.InfrastructureCapacity);
        Assert.Equal(80.0, yield.FinancialCapacity);

        Assert.Equal(55.0, yield.HumanCapital);
        Assert.Equal(65.0, yield.ScienceCapacity);
        Assert.Equal(75.0, yield.InnovationCapacity);

        Assert.Equal(85.0, yield.AdministrativeCapacity);
        Assert.Equal(90.0, yield.SecurityCapacity);
        Assert.Equal(40.0, yield.IntelligenceCapacity);
        Assert.Equal(45.0, yield.MilitaryReadiness);
        Assert.Equal(95.0, yield.DiplomaticCapacity);

        Assert.Equal(30.0, yield.NaturalResourceOutput);

        // Verify nested dimension properties match
        Assert.Equal(1000.0, yield.Economic.RepuTreasuryRevenue);
        Assert.Equal(50.0, yield.Economic.IndustrialCapacity);
        Assert.Equal(60.0, yield.Economic.EnergyCapacity);
        Assert.Equal(70.0, yield.Economic.InfrastructureCapacity);
        Assert.Equal(80.0, yield.Economic.FinancialCapacity);

        Assert.Equal(55.0, yield.HumanKnowledge.HumanCapital);
        Assert.Equal(65.0, yield.HumanKnowledge.ScienceCapacity);
        Assert.Equal(75.0, yield.HumanKnowledge.InnovationCapacity);

        Assert.Equal(85.0, yield.State.AdministrativeCapacity);
        Assert.Equal(90.0, yield.State.SecurityCapacity);
        Assert.Equal(40.0, yield.State.IntelligenceCapacity);
        Assert.Equal(45.0, yield.State.MilitaryReadiness);
        Assert.Equal(95.0, yield.State.DiplomaticCapacity);

        Assert.Equal(30.0, yield.ResourceOutput.NaturalResourceOutput);
    }

    [Fact]
    public void Test5_Food_IsNotRepresentedAsNationalYieldCategory()
    {
        // Enforce design decision: Food is part of the emergent civilian economy, NOT a National Yield category.
        var enumNames = Enum.GetNames<NationalYieldCategory>();
        foreach (var name in enumNames)
        {
            Assert.DoesNotContain("Food", name, StringComparison.OrdinalIgnoreCase);
        }

        // Reflection check across all NationalYield types
        var yieldTypes = new[]
        {
            typeof(NationalYield),
            typeof(EconomicCapacities),
            typeof(HumanKnowledgeCapacities),
            typeof(StateCapacities),
            typeof(ResourceOutputCapacities),
            typeof(NationalYieldCategory)
        };

        foreach (var type in yieldTypes)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var prop in properties)
            {
                Assert.DoesNotContain("Food", prop.Name, StringComparison.OrdinalIgnoreCase);
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in fields)
            {
                Assert.DoesNotContain("Food", field.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Test6_NationalYield_SerializesAndDeserializesCorrectly()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found("Federation of Zenith", clock.CurrentTime);

        country.Yield.RepuTreasuryRevenue = 250_000.0;
        country.Yield.IndustrialCapacity = 85.0;
        country.Yield.EnergyCapacity = 92.0;
        country.Yield.InfrastructureCapacity = 78.0;
        country.Yield.FinancialCapacity = 88.0;

        country.Yield.HumanCapital = 74.0;
        country.Yield.ScienceCapacity = 91.0;
        country.Yield.InnovationCapacity = 83.0;

        country.Yield.AdministrativeCapacity = 89.0;
        country.Yield.SecurityCapacity = 94.0;
        country.Yield.IntelligenceCapacity = 76.0;
        country.Yield.MilitaryReadiness = 82.0;
        country.Yield.DiplomaticCapacity = 87.0;

        country.Yield.NaturalResourceOutput = 65.0;

        var json = JsonSerializer.Serialize(country, new JsonSerializerOptions { WriteIndented = true });
        var restored = JsonSerializer.Deserialize<Country>(json);

        Assert.NotNull(restored);
        Assert.NotNull(restored.Yield);

        // Verify all 14 categories survived round-trip serialization
        Assert.Equal(250_000.0, restored.Yield.RepuTreasuryRevenue);
        Assert.Equal(85.0, restored.Yield.IndustrialCapacity);
        Assert.Equal(92.0, restored.Yield.EnergyCapacity);
        Assert.Equal(78.0, restored.Yield.InfrastructureCapacity);
        Assert.Equal(88.0, restored.Yield.FinancialCapacity);

        Assert.Equal(74.0, restored.Yield.HumanCapital);
        Assert.Equal(91.0, restored.Yield.ScienceCapacity);
        Assert.Equal(83.0, restored.Yield.InnovationCapacity);

        Assert.Equal(89.0, restored.Yield.AdministrativeCapacity);
        Assert.Equal(94.0, restored.Yield.SecurityCapacity);
        Assert.Equal(76.0, restored.Yield.IntelligenceCapacity);
        Assert.Equal(82.0, restored.Yield.MilitaryReadiness);
        Assert.Equal(87.0, restored.Yield.DiplomaticCapacity);

        Assert.Equal(65.0, restored.Yield.NaturalResourceOutput);
    }

    [Fact]
    public async Task Test7_WorldState_SnapshotAndRestore_PreservesCountryNationalYield()
    {
        var clock = RepublicClock.CreateControlled();
        var worldManager = new WorldManager(_eventBus, _logger, clock);
        await worldManager.CreateAsync("Terra Nova");

        var countryA = worldManager.Countries.FoundCountry("Aurelia");
        countryA.Yield.IndustrialCapacity = 70.0;
        countryA.Yield.ScienceCapacity = 90.0;
        countryA.Yield.RepuTreasuryRevenue = 120_000.0;

        var countryB = worldManager.Countries.FoundCountry("Borealis");
        countryB.Yield.IndustrialCapacity = 40.0;
        countryB.Yield.MilitaryReadiness = 85.0;
        countryB.Yield.DiplomaticCapacity = 92.0;

        // Snapshot world state
        var snapshot = worldManager.Snapshot();
        Assert.Equal(2, snapshot.Countries.Count);

        // Serialize and deserialize snapshot (as persistence would)
        var json = JsonSerializer.Serialize(snapshot);
        var restoredSnapshot = JsonSerializer.Deserialize<WorldState>(json);
        Assert.NotNull(restoredSnapshot);

        // Restore to another world manager instance
        var restoredManager = new WorldManager(_eventBus, _logger, clock);
        restoredManager.Restore(restoredSnapshot);

        var restoredA = restoredManager.Countries.GetCountry(countryA.Id);
        var restoredB = restoredManager.Countries.GetCountry(countryB.Id);

        Assert.NotNull(restoredA);
        Assert.NotNull(restoredB);

        Assert.Equal(70.0, restoredA.Yield.IndustrialCapacity);
        Assert.Equal(90.0, restoredA.Yield.ScienceCapacity);
        Assert.Equal(120_000.0, restoredA.Yield.RepuTreasuryRevenue);

        Assert.Equal(40.0, restoredB.Yield.IndustrialCapacity);
        Assert.Equal(85.0, restoredB.Yield.MilitaryReadiness);
        Assert.Equal(92.0, restoredB.Yield.DiplomaticCapacity);

        // Check independence after restore
        restoredA.Yield.IndustrialCapacity = 99.0;
        Assert.Equal(40.0, restoredB.Yield.IndustrialCapacity);
    }

    [Fact]
    public void Test8_NoArbitraryConversionBetweenCapacitiesAndRepu_Exists()
    {
        // Enforce architectural boundary: Capacities must NOT have ad-hoc conversion methods or hardcoded ratios to REPU.
        // E.g., no "ConvertToRepu", "RepuPerScience", "RepuEquivalent" methods or properties.
        var nationalYieldMethods = typeof(NationalYield).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var invalidMethodNames = new[] { "ConvertToRepu", "ToRepu", "CalculateRevenue", "YieldToCurrency" };

        foreach (var method in nationalYieldMethods)
        {
            foreach (var invalid in invalidMethodNames)
            {
                Assert.False(
                    string.Equals(method.Name, invalid, StringComparison.OrdinalIgnoreCase),
                    $"Method '{method.Name}' violates the non-conversion boundary for Step 4A.");
            }
        }
    }

    [Fact]
    public void Test9_NationalYield_Clone_CreatesDeepIndependentCopy()
    {
        var original = new NationalYield
        {
            RepuTreasuryRevenue = 10_000.0,
            IndustrialCapacity = 50.0,
            ScienceCapacity = 60.0,
            AdministrativeCapacity = 70.0,
            NaturalResourceOutput = 35.0
        };

        var clone = original.Clone();

        Assert.NotSame(original, clone);
        Assert.NotSame(original.Economic, clone.Economic);
        Assert.NotSame(original.HumanKnowledge, clone.HumanKnowledge);
        Assert.NotSame(original.State, clone.State);
        Assert.NotSame(original.ResourceOutput, clone.ResourceOutput);

        Assert.Equal(original.RepuTreasuryRevenue, clone.RepuTreasuryRevenue);
        Assert.Equal(original.IndustrialCapacity, clone.IndustrialCapacity);
        Assert.Equal(original.ScienceCapacity, clone.ScienceCapacity);
        Assert.Equal(original.AdministrativeCapacity, clone.AdministrativeCapacity);
        Assert.Equal(original.NaturalResourceOutput, clone.NaturalResourceOutput);

        // Mutate clone and assert original is unaffected
        clone.RepuTreasuryRevenue = 99_999.0;
        clone.ScienceCapacity = 99.0;

        Assert.Equal(10_000.0, original.RepuTreasuryRevenue);
        Assert.Equal(60.0, original.ScienceCapacity);
    }
}

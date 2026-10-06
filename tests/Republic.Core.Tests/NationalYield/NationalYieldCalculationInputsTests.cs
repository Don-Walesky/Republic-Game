namespace Republic.Core.Tests.NationalYield;

using System.Reflection;
using System.Text.Json;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

/// <summary>
/// Unit tests for the National Yield calculation-input model (Step 4B).
/// Validates input representation, composition with Country state, serialization, category integrity,
/// food exclusion, and non-calculation boundaries.
/// </summary>
public sealed class NationalYieldCalculationInputsTests
{
    [Fact]
    public void Test1_CalculationInputModel_CanBeCreated_WithDefaultsAndExplicitValues()
    {
        // 1. Defaults
        var defaultInputs = new NationalYieldCalculationInputs();
        Assert.NotNull(defaultInputs.Modifiers);
        Assert.Empty(defaultInputs.Modifiers);
        Assert.Equal(string.Empty, defaultInputs.CountryId);
        Assert.Equal(0.0, defaultInputs.EconomicCapacity);
        Assert.Equal(0.0, defaultInputs.GovernmentEffectiveness);
        Assert.Equal(0.0, defaultInputs.InfrastructureCondition);
        Assert.Equal(0.0, defaultInputs.HumanCapital);
        Assert.Equal(0.0, defaultInputs.ScienceTechnology);
        Assert.Equal(0.0, defaultInputs.PoliticalStability);
        Assert.Equal(0.0, defaultInputs.SecurityLevel);
        Assert.Equal(0.0, defaultInputs.CabinetEffectiveness);

        // 2. Explicit values
        var customInputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-123",
            EconomicCapacity = 75.0,
            GovernmentEffectiveness = 80.0,
            InfrastructureCondition = 68.0,
            HumanCapital = 72.0,
            ScienceTechnology = 85.0,
            PoliticalStability = 90.0,
            SecurityLevel = 78.0,
            CabinetEffectiveness = 82.0
        };

        Assert.Equal("country-123", customInputs.CountryId);
        Assert.Equal(75.0, customInputs.EconomicCapacity);
        Assert.Equal(80.0, customInputs.GovernmentEffectiveness);
        Assert.Equal(68.0, customInputs.InfrastructureCondition);
        Assert.Equal(72.0, customInputs.HumanCapital);
        Assert.Equal(85.0, customInputs.ScienceTechnology);
        Assert.Equal(90.0, customInputs.PoliticalStability);
        Assert.Equal(78.0, customInputs.SecurityLevel);
        Assert.Equal(82.0, customInputs.CabinetEffectiveness);
    }

    [Fact]
    public void Test2_FromCountryFactory_ComposesWithExistingCountryDomainState()
    {
        var clock = RepublicClock.CreateControlled();
        var country = Country.Found(
            name: "Federal Republic of Zephyrus",
            foundingTime: clock.CurrentTime,
            baselineStability: 85.5);

        country.NationalTraits.Add("Maritime Trade");
        country.NationalTraits.Add("Scientific Heritage");

        country.Yield.IndustrialCapacity = 70.0;
        country.Yield.AdministrativeCapacity = 80.0;
        country.Yield.InfrastructureCapacity = 75.0;
        country.Yield.HumanCapital = 78.0;
        country.Yield.ScienceCapacity = 88.0;
        country.Yield.SecurityCapacity = 82.0;

        var inputs = NationalYieldCalculationInputs.FromCountry(country, cabinetEffectiveness: 77.0);

        // Asserts composition with authoritative Country properties (no duplicate competing truth)
        Assert.Equal(country.Id, inputs.CountryId);
        Assert.Equal(country.BaselineStability, inputs.PoliticalStability);
        Assert.Equal(70.0, inputs.EconomicCapacity);
        Assert.Equal(80.0, inputs.GovernmentEffectiveness);
        Assert.Equal(75.0, inputs.InfrastructureCondition);
        Assert.Equal(78.0, inputs.HumanCapital);
        Assert.Equal(88.0, inputs.ScienceTechnology);
        Assert.Equal(82.0, inputs.SecurityLevel);
        Assert.Equal(77.0, inputs.CabinetEffectiveness);

        // Asserts national traits mapped to modifiers
        Assert.Equal(2, inputs.Modifiers.Count);
        Assert.Contains(inputs.Modifiers, m => m.Name == "Maritime Trade" && m.Source == "NationalTrait");
        Assert.Contains(inputs.Modifiers, m => m.Name == "Scientific Heritage" && m.Source == "NationalTrait");
    }

    [Fact]
    public void Test3_DefinedInputs_CanRepresentCategorySpecificAndGeneralModifiers()
    {
        var inputs = new NationalYieldCalculationInputs { CountryId = "c-1" };

        var generalModifier = new NationalYieldModifier
        {
            Id = "mod-general",
            Name = "National Morale",
            TargetCategory = null,
            Source = "CivicMovement",
            Description = "General societal morale boost",
            Value = 10.0
        };

        var industrialModifier = new NationalYieldModifier
        {
            Id = "mod-industrial",
            Name = "Heavy Machinery Subsidy",
            TargetCategory = NationalYieldCategory.IndustrialCapacity,
            Source = "ExecutiveDecree",
            Description = "Targeted industrial development initiative",
            Value = 15.0
        };

        var scienceModifier = new NationalYieldModifier
        {
            Id = "mod-science",
            Name = "Quantum Lab Grant",
            TargetCategory = NationalYieldCategory.ScienceCapacity,
            Source = "ResearchCouncil",
            Description = "Advanced research acceleration",
            Value = 25.0
        };

        inputs.AddModifier(generalModifier);
        inputs.AddModifier(industrialModifier);
        inputs.AddModifier(scienceModifier);

        var industrialMatches = inputs.GetModifiersForCategory(NationalYieldCategory.IndustrialCapacity).ToList();
        Assert.Equal(2, industrialMatches.Count);
        Assert.Contains(industrialMatches, m => m.Id == "mod-general");
        Assert.Contains(industrialMatches, m => m.Id == "mod-industrial");

        var scienceMatches = inputs.GetModifiersForCategory(NationalYieldCategory.ScienceCapacity).ToList();
        Assert.Equal(2, scienceMatches.Count);
        Assert.Contains(scienceMatches, m => m.Id == "mod-general");
        Assert.Contains(scienceMatches, m => m.Id == "mod-science");

        var energyMatches = inputs.GetModifiersForCategory(NationalYieldCategory.EnergyCapacity).ToList();
        Assert.Single(energyMatches);
        Assert.Contains(energyMatches, m => m.Id == "mod-general");
    }

    [Fact]
    public void Test4_SerializationAndDeserialization_RoundTripsAccurately()
    {
        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-serialized-1",
            EconomicCapacity = 65.5,
            GovernmentEffectiveness = 72.0,
            InfrastructureCondition = 80.0,
            HumanCapital = 74.0,
            ScienceTechnology = 88.0,
            PoliticalStability = 79.5,
            SecurityLevel = 84.0,
            CabinetEffectiveness = 71.0
        };

        inputs.AddModifier(new NationalYieldModifier
        {
            Id = "mod-1",
            Name = "Port Logistics Overhaul",
            TargetCategory = NationalYieldCategory.InfrastructureCapacity,
            Source = "MinistryOfTransport",
            Description = "Port modernization",
            Value = 12.0
        });

        var json = JsonSerializer.Serialize(inputs, new JsonSerializerOptions { WriteIndented = true });
        var restored = JsonSerializer.Deserialize<NationalYieldCalculationInputs>(json);

        Assert.NotNull(restored);
        Assert.Equal("country-serialized-1", restored.CountryId);
        Assert.Equal(65.5, restored.EconomicCapacity);
        Assert.Equal(72.0, restored.GovernmentEffectiveness);
        Assert.Equal(80.0, restored.InfrastructureCondition);
        Assert.Equal(74.0, restored.HumanCapital);
        Assert.Equal(88.0, restored.ScienceTechnology);
        Assert.Equal(79.5, restored.PoliticalStability);
        Assert.Equal(84.0, restored.SecurityLevel);
        Assert.Equal(71.0, restored.CabinetEffectiveness);

        Assert.Single(restored.Modifiers);
        var restoredMod = restored.Modifiers[0];
        Assert.Equal("mod-1", restoredMod.Id);
        Assert.Equal("Port Logistics Overhaul", restoredMod.Name);
        Assert.Equal(NationalYieldCategory.InfrastructureCapacity, restoredMod.TargetCategory);
        Assert.Equal("MinistryOfTransport", restoredMod.Source);
        Assert.Equal(12.0, restoredMod.Value);
    }

    [Fact]
    public void Test5_DoesNotAlterExistingNationalYieldCategories()
    {
        var categories = Enum.GetValues<NationalYieldCategory>();
        Assert.Equal(14, categories.Length);

        // Verify all 14 canonical categories from Step 4A remain intact
        Assert.True(Enum.IsDefined(NationalYieldCategory.RepuTreasuryRevenue));
        Assert.True(Enum.IsDefined(NationalYieldCategory.IndustrialCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.EnergyCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.InfrastructureCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.FinancialCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.HumanCapital));
        Assert.True(Enum.IsDefined(NationalYieldCategory.ScienceCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.InnovationCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.AdministrativeCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.SecurityCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.IntelligenceCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.MilitaryReadiness));
        Assert.True(Enum.IsDefined(NationalYieldCategory.DiplomaticCapacity));
        Assert.True(Enum.IsDefined(NationalYieldCategory.NaturalResourceOutput));
    }

    [Fact]
    public void Test6_DoesNotIntroduceFoodIntoNationalYieldOrCalculationInputs()
    {
        var typesToCheck = new[]
        {
            typeof(NationalYieldCalculationInputs),
            typeof(NationalYieldModifier),
            typeof(NationalYieldCategory),
            typeof(NationalYield),
            typeof(EconomicCapacities),
            typeof(HumanKnowledgeCapacities),
            typeof(StateCapacities),
            typeof(ResourceOutputCapacities)
        };

        foreach (var type in typesToCheck)
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
    public void Test7_DoesNotCalculateYield()
    {
        var inputMethods = typeof(NationalYieldCalculationInputs).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var modifierMethods = typeof(NationalYieldModifier).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var forbiddenMethodSubstrings = new[]
        {
            "Calculate",
            "Compute",
            "EvaluateYield",
            "Tick",
            "Advance",
            "ExecuteYield"
        };

        foreach (var method in inputMethods.Concat(modifierMethods))
        {
            foreach (var forbidden in forbiddenMethodSubstrings)
            {
                Assert.DoesNotContain(forbidden, method.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Test8_DoesNotGenerateRepuRevenue()
    {
        var inputMethods = typeof(NationalYieldCalculationInputs).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var modifierMethods = typeof(NationalYieldModifier).GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var forbiddenRevenueSubstrings = new[]
        {
            "GenerateRepu",
            "DepositRevenue",
            "CollectRevenue",
            "TransferRepu",
            "CalculateRevenue",
            "ProduceRepu"
        };

        foreach (var method in inputMethods.Concat(modifierMethods))
        {
            foreach (var forbidden in forbiddenRevenueSubstrings)
            {
                Assert.DoesNotContain(forbidden, method.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Test9_DoesNotIntroduceArbitraryMultipliersOrHardcodedFormulas()
    {
        var inputProperties = typeof(NationalYieldCalculationInputs).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var modifierProperties = typeof(NationalYieldModifier).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var forbiddenNames = new[]
        {
            "Multiplier",
            "YieldMultiplier",
            "ProductionRatio",
            "Weighting",
            "ConversionRate"
        };

        foreach (var prop in inputProperties.Concat(modifierProperties))
        {
            foreach (var forbidden in forbiddenNames)
            {
                Assert.DoesNotContain(forbidden, prop.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Test10_Clone_CreatesDeepIndependentCopy()
    {
        var original = new NationalYieldCalculationInputs
        {
            CountryId = "orig-1",
            EconomicCapacity = 50.0,
            PoliticalStability = 80.0
        };

        original.AddModifier(new NationalYieldModifier
        {
            Id = "mod-1",
            Name = "Original Modifier",
            Value = 10.0
        });

        var clone = original.Clone();

        Assert.NotSame(original, clone);
        Assert.NotSame(original.Modifiers, clone.Modifiers);
        Assert.NotSame(original.Modifiers[0], clone.Modifiers[0]);

        Assert.Equal("orig-1", clone.CountryId);
        Assert.Equal(50.0, clone.EconomicCapacity);
        Assert.Equal(80.0, clone.PoliticalStability);
        Assert.Equal(10.0, clone.Modifiers[0].Value);

        // Mutating clone does not affect original
        clone.EconomicCapacity = 99.0;
        clone.Modifiers.Add(new NationalYieldModifier { Id = "mod-2", Name = "Second Mod" });

        Assert.Equal(50.0, original.EconomicCapacity);
        Assert.Single(original.Modifiers);
        Assert.Equal(2, clone.Modifiers.Count);
    }
}

namespace Republic.Core.Tests.NationalYield;

using System.Reflection;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

/// <summary>
/// Unit tests for the isolated, deterministic National Yield calculation engine (Step 4D).
/// Validates determinism, country isolation, input immutability, complete 14-category coverage,
/// food exclusion, direct input influence, modifier determinism, and zero randomness.
/// </summary>
public sealed class NationalYieldCalculatorTests
{
    private readonly NationalYieldCalculator _calculator = new();

    [Fact]
    public void TestA_Determinism_GivenIdenticalInputs_ProducesIdenticalResults()
    {
        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-det-1",
            EconomicCapacity = 80.0,
            GovernmentEffectiveness = 75.0,
            InfrastructureCondition = 70.0,
            HumanCapital = 65.0,
            ScienceTechnology = 85.0,
            PoliticalStability = 90.0,
            SecurityLevel = 78.0,
            CabinetEffectiveness = 82.0
        };

        inputs.AddModifier(new NationalYieldModifier
        {
            Id = "mod-ind",
            Name = "Industrial Subsidy",
            TargetCategory = NationalYieldCategory.IndustrialCapacity,
            Value = 10.0
        });

        var result1 = _calculator.Calculate(inputs);
        var result2 = _calculator.Calculate(inputs);

        // Assert all 14 categories evaluate identically
        Assert.Equal(result1.RepuTreasuryRevenue, result2.RepuTreasuryRevenue);
        Assert.Equal(result1.IndustrialCapacity, result2.IndustrialCapacity);
        Assert.Equal(result1.EnergyCapacity, result2.EnergyCapacity);
        Assert.Equal(result1.InfrastructureCapacity, result2.InfrastructureCapacity);
        Assert.Equal(result1.FinancialCapacity, result2.FinancialCapacity);

        Assert.Equal(result1.HumanCapital, result2.HumanCapital);
        Assert.Equal(result1.ScienceCapacity, result2.ScienceCapacity);
        Assert.Equal(result1.InnovationCapacity, result2.InnovationCapacity);

        Assert.Equal(result1.AdministrativeCapacity, result2.AdministrativeCapacity);
        Assert.Equal(result1.SecurityCapacity, result2.SecurityCapacity);
        Assert.Equal(result1.IntelligenceCapacity, result2.IntelligenceCapacity);
        Assert.Equal(result1.MilitaryReadiness, result2.MilitaryReadiness);
        Assert.Equal(result1.DiplomaticCapacity, result2.DiplomaticCapacity);

        Assert.Equal(result1.NaturalResourceOutput, result2.NaturalResourceOutput);
    }

    [Fact]
    public void TestB_CountryIsolation_CalculatingCountryA_DoesNotMutateCountryB()
    {
        var clock = RepublicClock.CreateControlled();
        var countryA = Country.Found("Nation Alpha", clock.CurrentTime);
        var countryB = Country.Found("Nation Beta", clock.CurrentTime);

        countryA.Yield.IndustrialCapacity = 50.0;
        countryB.Yield.IndustrialCapacity = 20.0;

        var inputsA = NationalYieldCalculationInputs.FromCountry(countryA, cabinetEffectiveness: 70.0);
        var inputsB = NationalYieldCalculationInputs.FromCountry(countryB, cabinetEffectiveness: 40.0);

        var originalBInd = inputsB.EconomicCapacity;

        var resultA = _calculator.Calculate(inputsA);

        // Assert Country B's input and yield remain unmodified
        Assert.Equal(originalBInd, inputsB.EconomicCapacity);
        Assert.Equal(20.0, countryB.Yield.IndustrialCapacity);
        Assert.NotEqual(resultA.IndustrialCapacity, countryB.Yield.IndustrialCapacity);
    }

    [Fact]
    public void TestC_InputImmutability_CalculationDoesNotModifyInputObject()
    {
        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-immut-1",
            EconomicCapacity = 100.0,
            GovernmentEffectiveness = 80.0,
            InfrastructureCondition = 75.0,
            HumanCapital = 70.0,
            ScienceTechnology = 90.0,
            PoliticalStability = 85.0,
            SecurityLevel = 80.0,
            CabinetEffectiveness = 75.0
        };

        var mod = new NationalYieldModifier
        {
            Id = "mod-1",
            Name = "Research Grant",
            TargetCategory = NationalYieldCategory.ScienceCapacity,
            Value = 15.0
        };
        inputs.AddModifier(mod);

        // Snapshot input property values prior to calculation
        var origEco = inputs.EconomicCapacity;
        var origGov = inputs.GovernmentEffectiveness;
        var origModCount = inputs.Modifiers.Count;
        var origModVal = mod.Value;

        var result = _calculator.Calculate(inputs);

        Assert.NotNull(result);
        Assert.Equal(origEco, inputs.EconomicCapacity);
        Assert.Equal(origGov, inputs.GovernmentEffectiveness);
        Assert.Equal(origModCount, inputs.Modifiers.Count);
        Assert.Equal(origModVal, inputs.Modifiers[0].Value);
    }

    [Fact]
    public void TestD_AllFourteenCategories_ArePresentInCalculationResult()
    {
        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-14",
            EconomicCapacity = 50.0,
            InfrastructureCondition = 50.0,
            HumanCapital = 50.0,
            ScienceTechnology = 50.0,
            GovernmentEffectiveness = 50.0,
            SecurityLevel = 50.0
        };

        var result = _calculator.Calculate(inputs);

        // Verify all 14 canonical categories are present and accessible
        foreach (var category in Enum.GetValues<NationalYieldCategory>())
        {
            var capacity = result.GetCapacity(category);
            Assert.False(double.IsNaN(capacity));
            Assert.True(capacity >= 0.0);
        }

        // Direct input mappings reflect straight baseline values without balancing multipliers
        Assert.Equal(50.0, result.IndustrialCapacity);
        Assert.Equal(50.0, result.InfrastructureCapacity);
        Assert.Equal(50.0, result.HumanCapital);
        Assert.Equal(50.0, result.ScienceCapacity);
        Assert.Equal(50.0, result.AdministrativeCapacity);
        Assert.Equal(50.0, result.SecurityCapacity);

        // Uncalibrated categories safely report their neutral baseline (0.0) pending future domain models
        Assert.Equal(0.0, result.EnergyCapacity);
        Assert.Equal(0.0, result.FinancialCapacity);
        Assert.Equal(0.0, result.InnovationCapacity);
        Assert.Equal(0.0, result.IntelligenceCapacity);
        Assert.Equal(0.0, result.MilitaryReadiness);
        Assert.Equal(0.0, result.DiplomaticCapacity);
        Assert.Equal(0.0, result.NaturalResourceOutput);
        Assert.Equal(0.0, result.RepuTreasuryRevenue);
    }

    [Fact]
    public void TestE_FoodExclusion_FoodIsNotRepresentedInCalculatorOrOutput()
    {
        var calculatorType = typeof(NationalYieldCalculator);
        var methods = calculatorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var method in methods)
        {
            Assert.DoesNotContain("Food", method.Name, StringComparison.OrdinalIgnoreCase);
        }

        var result = _calculator.Calculate(new NationalYieldCalculationInputs());
        var resultProperties = typeof(NationalYield).GetProperties();
        foreach (var prop in resultProperties)
        {
            Assert.DoesNotContain("Food", prop.Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void TestF_BasicInputInfluence_ChangingInputsAltersCorrespondingOutputs()
    {
        var lowInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 20.0,
            ScienceTechnology = 20.0,
            GovernmentEffectiveness = 20.0,
            InfrastructureCondition = 20.0,
            HumanCapital = 20.0,
            SecurityLevel = 20.0
        };

        var highInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 100.0,
            ScienceTechnology = 100.0,
            GovernmentEffectiveness = 100.0,
            InfrastructureCondition = 100.0,
            HumanCapital = 100.0,
            SecurityLevel = 100.0
        };

        var lowResult = _calculator.Calculate(lowInputs);
        var highResult = _calculator.Calculate(highInputs);

        Assert.True(highResult.IndustrialCapacity > lowResult.IndustrialCapacity);
        Assert.True(highResult.ScienceCapacity > lowResult.ScienceCapacity);
        Assert.True(highResult.AdministrativeCapacity > lowResult.AdministrativeCapacity);
        Assert.True(highResult.InfrastructureCapacity > lowResult.InfrastructureCapacity);
        Assert.True(highResult.HumanCapital > lowResult.HumanCapital);
        Assert.True(highResult.SecurityCapacity > lowResult.SecurityCapacity);
    }

    [Fact]
    public void TestG_ModifierDeterminism_TargetedAndGeneralModifiersApplyDeterministically()
    {
        var baseInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 50.0,
            ScienceTechnology = 50.0
        };

        var inputsWithMod = baseInputs.Clone();
        inputsWithMod.AddModifier(new NationalYieldModifier
        {
            Id = "mod-sci",
            TargetCategory = NationalYieldCategory.ScienceCapacity,
            Value = 25.0
        });

        var baseResult = _calculator.Calculate(baseInputs);
        var modResult = _calculator.Calculate(inputsWithMod);

        // Targeted category receives modifier value
        Assert.Equal(baseResult.ScienceCapacity + 25.0, modResult.ScienceCapacity);

        // Unrelated category is unaffected by targeted modifier
        Assert.Equal(baseResult.InfrastructureCapacity, modResult.InfrastructureCapacity);
    }

    [Fact]
    public void TestH_NoRandomness_CalculatorHasNoRandomOrSystemClockDependencies()
    {
        var calculatorType = typeof(NationalYieldCalculator);
        var fields = calculatorType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var field in fields)
        {
            Assert.False(
                field.FieldType == typeof(Random),
                $"Field '{field.Name}' references System.Random, violating deterministic isolation.");
        }

        // Test running 50 iterations back-to-back produces identical results
        var inputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 45.67,
            GovernmentEffectiveness = 81.23,
            PoliticalStability = 67.89
        };

        var first = _calculator.Calculate(inputs);
        for (var i = 0; i < 50; i++)
        {
            var next = _calculator.Calculate(inputs);
            Assert.Equal(first.IndustrialCapacity, next.IndustrialCapacity);
            Assert.Equal(first.AdministrativeCapacity, next.AdministrativeCapacity);
        }
    }
}

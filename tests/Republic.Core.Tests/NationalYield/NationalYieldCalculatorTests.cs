namespace Republic.Core.Tests.NationalYield;

using System.Reflection;
using Republic.Core.NationalYield;
using Republic.Core.Time;
using Republic.Core.World.Models;
using Xunit;

/// <summary>
/// Unit tests for the simplified, deterministic National Yield calculation engine.
/// Validates determinism, country isolation, input immutability, complete 14-category coverage,
/// food exclusion, enabling constraints, archetypal country behaviors, and non-negativity.
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
    public void TestD_AllFourteenCategories_ProducePositiveMeaningfulResults()
    {
        var inputs = new NationalYieldCalculationInputs
        {
            CountryId = "country-14",
            EconomicCapacity = 50.0,
            InfrastructureCondition = 50.0,
            HumanCapital = 50.0,
            ScienceTechnology = 50.0,
            GovernmentEffectiveness = 50.0,
            SecurityLevel = 50.0,
            PoliticalStability = 50.0
        };

        var result = _calculator.Calculate(inputs);

        // Verify all 14 canonical categories are present, non-negative, and produce meaningful values
        foreach (var category in Enum.GetValues<NationalYieldCategory>())
        {
            var capacity = result.GetCapacity(category);
            Assert.False(double.IsNaN(capacity));
            Assert.True(capacity > 0.0, $"Category '{category}' should produce a meaningful positive value.");
        }

        // Verify sensible outputs on a matched scale without explosive multipliers
        Assert.Equal(50.0, result.IndustrialCapacity);
        Assert.Equal(50.0, result.EnergyCapacity);
        Assert.Equal(50.0, result.InfrastructureCapacity);
        Assert.Equal(50.0, result.FinancialCapacity);
        Assert.Equal(50.0, result.HumanCapital);
        Assert.Equal(50.0, result.ScienceCapacity);
        Assert.Equal(50.0, result.InnovationCapacity);
        Assert.Equal(50.0, result.AdministrativeCapacity);
        Assert.Equal(50.0, result.SecurityCapacity);
        Assert.Equal(50.0, result.IntelligenceCapacity);
        Assert.Equal(50.0, result.MilitaryReadiness);
        Assert.Equal(50.0, result.DiplomaticCapacity);
        Assert.Equal(50.0, result.NaturalResourceOutput);
        Assert.Equal(50.0, result.RepuTreasuryRevenue);
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
            SecurityLevel = 20.0,
            PoliticalStability = 20.0
        };

        var highInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 100.0,
            ScienceTechnology = 100.0,
            GovernmentEffectiveness = 100.0,
            InfrastructureCondition = 100.0,
            HumanCapital = 100.0,
            SecurityLevel = 100.0,
            PoliticalStability = 100.0
        };

        var lowResult = _calculator.Calculate(lowInputs);
        var highResult = _calculator.Calculate(highInputs);

        Assert.True(highResult.IndustrialCapacity > lowResult.IndustrialCapacity);
        Assert.True(highResult.ScienceCapacity > lowResult.ScienceCapacity);
        Assert.True(highResult.AdministrativeCapacity > lowResult.AdministrativeCapacity);
        Assert.True(highResult.InfrastructureCapacity > lowResult.InfrastructureCapacity);
        Assert.True(highResult.HumanCapital > lowResult.HumanCapital);
        Assert.True(highResult.SecurityCapacity > lowResult.SecurityCapacity);
        Assert.True(highResult.RepuTreasuryRevenue > lowResult.RepuTreasuryRevenue);
    }

    [Fact]
    public void TestG_ModifierDeterminism_TargetedAndGeneralModifiersApplyDeterministically()
    {
        var baseInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 50.0,
            InfrastructureCondition = 50.0,
            ScienceTechnology = 50.0,
            HumanCapital = 50.0
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
            InfrastructureCondition = 60.0,
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

    [Fact]
    public void TestI_EnablingConstraintsAndDirectRelationships_WorkAsExpected()
    {
        // 1. Weak Infrastructure constrains Industrial Capacity
        var weakInfra = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 90.0,
            InfrastructureCondition = 20.0,
            GovernmentEffectiveness = 50.0,
            PoliticalStability = 50.0
        };
        var weakInfraResult = _calculator.Calculate(weakInfra);
        Assert.Equal(20.0, weakInfraResult.IndustrialCapacity);

        // Upgrading Infrastructure unbottlenecks Industrial Capacity
        var strongInfra = weakInfra.Clone();
        strongInfra.InfrastructureCondition = 90.0;
        var strongInfraResult = _calculator.Calculate(strongInfra);
        Assert.Equal(90.0, strongInfraResult.IndustrialCapacity);

        // 2. Strong Science directly helps Innovation
        var scienceInputs = new NationalYieldCalculationInputs
        {
            ScienceTechnology = 80.0,
            HumanCapital = 80.0
        };
        var scienceResult = _calculator.Calculate(scienceInputs);
        Assert.Equal(80.0, scienceResult.ScienceCapacity);
        Assert.Equal(80.0, scienceResult.InnovationCapacity);

        // 3. Weak Human Capital constrains Science
        var lowHumanInputs = new NationalYieldCalculationInputs
        {
            ScienceTechnology = 80.0,
            HumanCapital = 30.0
        };
        var lowHumanResult = _calculator.Calculate(lowHumanInputs);
        Assert.Equal(30.0, lowHumanResult.ScienceCapacity);
        Assert.Equal(30.0, lowHumanResult.InnovationCapacity);

        // 4. Strong Administration unbottlenecks REPU Treasury Revenue
        var corruptState = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 90.0,
            InfrastructureCondition = 90.0,
            GovernmentEffectiveness = 15.0,
            PoliticalStability = 50.0
        };
        var corruptResult = _calculator.Calculate(corruptState);
        Assert.Equal(90.0, corruptResult.IndustrialCapacity);
        Assert.Equal(15.0, corruptResult.RepuTreasuryRevenue);

        // Cleaning up administration unbottlenecks revenue
        var cleanState = corruptState.Clone();
        cleanState.GovernmentEffectiveness = 85.0;
        var cleanResult = _calculator.Calculate(cleanState);
        Assert.Equal(85.0, cleanResult.RepuTreasuryRevenue);
    }

    [Fact]
    public void TestJ_ArchetypalCountries_ProduceSensibleIntuitiveOutcomes()
    {
        // 1. Weak Country: severely limited across the board
        var weakInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 15.0,
            InfrastructureCondition = 15.0,
            HumanCapital = 15.0,
            ScienceTechnology = 15.0,
            GovernmentEffectiveness = 15.0,
            SecurityLevel = 15.0,
            PoliticalStability = 15.0
        };
        var weakResult = _calculator.Calculate(weakInputs);

        // Modest, functioning values; does not magically become a superpower
        Assert.Equal(15.0, weakResult.IndustrialCapacity);
        Assert.Equal(15.0, weakResult.ScienceCapacity);
        Assert.Equal(15.0, weakResult.RepuTreasuryRevenue);
        Assert.True(weakResult.IndustrialCapacity < 30.0);

        // 2. Developing Country: moderate capabilities with room to grow
        var devInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 50.0,
            InfrastructureCondition = 50.0,
            HumanCapital = 50.0,
            ScienceTechnology = 50.0,
            GovernmentEffectiveness = 50.0,
            SecurityLevel = 50.0,
            PoliticalStability = 50.0
        };
        var devResult = _calculator.Calculate(devInputs);

        Assert.Equal(50.0, devResult.IndustrialCapacity);
        Assert.Equal(50.0, devResult.RepuTreasuryRevenue);
        Assert.True(devResult.IndustrialCapacity > weakResult.IndustrialCapacity);

        // 3. Strong Country: high capabilities across all sectors
        var strongInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 90.0,
            InfrastructureCondition = 90.0,
            HumanCapital = 90.0,
            ScienceTechnology = 90.0,
            GovernmentEffectiveness = 90.0,
            SecurityLevel = 90.0,
            PoliticalStability = 90.0
        };
        var strongResult = _calculator.Calculate(strongInputs);

        // Strong output, but bounded (not absurdly huge numbers)
        Assert.Equal(90.0, strongResult.IndustrialCapacity);
        Assert.Equal(90.0, strongResult.RepuTreasuryRevenue);
        Assert.True(strongResult.IndustrialCapacity > devResult.IndustrialCapacity);
        Assert.True(strongResult.RepuTreasuryRevenue < 200.0, "Revenue should not arbitrarily explode.");

        // 4. Specialized Country: Knowledge/Tech Hub (High Science & Human Capital, modest Industry & Military)
        var techHubInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 25.0,
            InfrastructureCondition = 25.0,
            HumanCapital = 95.0,
            ScienceTechnology = 95.0,
            GovernmentEffectiveness = 80.0,
            SecurityLevel = 20.0,
            PoliticalStability = 80.0
        };
        var techHubResult = _calculator.Calculate(techHubInputs);

        // High Science and Innovation reflect its national specialty
        Assert.Equal(95.0, techHubResult.ScienceCapacity);
        Assert.Equal(95.0, techHubResult.InnovationCapacity);
        Assert.Equal(95.0, techHubResult.HumanCapital);

        // Modest Industrial and Military reflect its trade-offs
        Assert.Equal(25.0, techHubResult.IndustrialCapacity);
        Assert.Equal(20.0, techHubResult.MilitaryReadiness);
        Assert.True(techHubResult.ScienceCapacity > techHubResult.IndustrialCapacity);
    }

    [Fact]
    public void TestK_NonNegativity_ZeroAndNegativeInputsNeverProduceNegativeOutputs()
    {
        var zeroInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = 0.0,
            InfrastructureCondition = 0.0,
            HumanCapital = 0.0,
            ScienceTechnology = 0.0,
            GovernmentEffectiveness = 0.0,
            SecurityLevel = 0.0,
            PoliticalStability = 0.0
        };
        var zeroResult = _calculator.Calculate(zeroInputs);

        foreach (var category in Enum.GetValues<NationalYieldCategory>())
        {
            var capacity = zeroResult.GetCapacity(category);
            Assert.Equal(0.0, capacity);
        }

        var negativeInputs = new NationalYieldCalculationInputs
        {
            EconomicCapacity = -50.0,
            InfrastructureCondition = -20.0,
            HumanCapital = -10.0,
            ScienceTechnology = -5.0,
            GovernmentEffectiveness = -30.0,
            SecurityLevel = -15.0,
            PoliticalStability = -100.0
        };
        var negResult = _calculator.Calculate(negativeInputs);

        foreach (var category in Enum.GetValues<NationalYieldCategory>())
        {
            var capacity = negResult.GetCapacity(category);
            Assert.Equal(0.0, capacity);
        }
    }
}

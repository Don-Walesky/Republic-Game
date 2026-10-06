# ADR-0005: National Yield Calculation Input Model

## Status
Accepted

## Context
In Step 4A ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)), Republic established the foundational domain representation for National Yield across 14 canonical categories.
National Yield represents what a sovereign nation is capable of producing, generating, or sustaining.

Republic's canonical design dictates that national output depends on a country's ability to convert resources, institutions, human talent, and leadership decisions into productive outcomes. Before designing any mathematical calculation engine, a clean input contract is needed to represent the factors influencing yield evaluation.

### Design Principles & Boundaries
1. **Clean Input Contract**: Define the parameters that a future calculation engine requires without baking in arbitrary formulas or conversion rates.
2. **No Arbitrary Multipliers or Hardcoded Formulas**: Final mathematical weights, production formulas, and conversion ratios are intentionally deferred to subsequent simulation engine steps.
3. **No Duplicate Competing Truth**: Factors that already exist on the sovereign country (such as baseline political stability and established capacities) compose directly with existing domain state rather than creating competing sources of truth.
4. **Food Exclusion Maintained**: Food remains strictly an output of the emergent civilian economy and is not an input or category of National Yield.
5. **No Revenue Generation or Ticks**: This model is pure domain state; it does not trigger yield ticks, produce REPU, or deposit funds into the treasury.

## Decision

1. **Domain Models Created**:
   - `NationalYieldCalculationInputs`: Encapsulates the complete set of rated factors informing yield calculations:
     - `CountryId`: Identifier of the evaluated sovereign nation.
     - `EconomicCapacity`: Baseline productive and industrial base.
     - `GovernmentEffectiveness`: Administrative competence and civil service execution throughput.
     - `InfrastructureCondition`: Logistics networks, port access, transport quality, and grid reliability.
     - `HumanCapital`: Workforce skills, educational attainment, technical literacy, and public health.
     - `ScienceTechnology`: Academic research capability and commercial tech adoption.
     - `PoliticalStability`: Societal cohesion, constitutional order, and public stability (sourced from `Country.BaselineStability`).
     - `SecurityLevel`: Law enforcement effectiveness, border integrity, and territorial defense.
     - `CabinetEffectiveness`: Ministerial competence, executive leadership, and portfolio alignment.
     - `Modifiers`: Strongly typed list of `NationalYieldModifier` items.
   - `NationalYieldModifier`: Represents explicitly defined national modifiers originating from national traits, executive decrees, active bilateral treaties, or cabinet initiatives, capturing `Id`, `Name`, `TargetCategory`, `Source`, `Description`, and `Value`.

2. **Composition with Existing Country Domain**:
   - Provided factory method `NationalYieldCalculationInputs.FromCountry(Country country, double cabinetEffectiveness = 0.0)`.
   - Bridges authoritative `Country` properties (e.g. `BaselineStability`, `Yield` baseline capacities, and `NationalTraits`) into calculation inputs without mutating or duplicating underlying country state.

3. **Deterministic & Serializable**:
   - Implemented as pure POCO objects serializable via `System.Text.Json`.
   - Independent of Unity, UI, networking, or database systems.
   - Deep cloning (`Clone()`) ensures input snapshots are isolated from caller mutations.

4. **Category & Food Boundaries Preserved**:
   - The 14 canonical categories established in Step 4A remain unaltered.
   - Food is strictly excluded from all input and modifier models.

## Consequences
- The forthcoming National Yield calculation engine (Step 4C) will consume `NationalYieldCalculationInputs` via a clean, predictable parameter contract.
- Future systems (cabinet portfolios, national traits, international treaties, infrastructure degradation/investment) can contribute to calculation inputs through the standardized modifier and factor interfaces.

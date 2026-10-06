# ADR-0004: National Yield Domain Model

## Status
Accepted

## Context
Republic models the sovereign country as the central entity of the simulation. As established in the canonical design (Step 1):
- **National Yield represents what a functioning country is capable of producing, generating, or sustaining.**
- It is **not** a free resource pool or an arbitrary resource drip where the government receives free magic resources every cycle.
- National Yield represents measurable national productive output and accumulating state capacity.
- Different categories represent distinct **capabilities** rather than arbitrary convertible currencies (e.g., Industrial Capacity is productive capability; Military Readiness is tactical preparedness; Diplomatic Capacity is treaty/soft power bandwidth; REPU Treasury Revenue is monetary output).

### Food Exclusion Principle
**Food is explicitly excluded from National Yield.**
Food belongs to the emergent productive civilian economy. In Republic, food production is driven by citizens, private businesses, agriculture, infrastructure, energy availability, and market dynamics. Government policy, infrastructure, and investments may improve the conditions under which the nation produces food, but the state does not magically generate a "food yield".

### Scope Limitation for Step 4A
This step establishes strictly the **foundational domain representation**.
The National Yield calculation engine, yield cycles, revenue generation ticks, founding $\times 10$ buffs, cabinet modifiers, and economic formulas belong to subsequent steps.

## Decision

1. **Categorization & Strongly Typed Model**:
   - Defined `NationalYieldCategory` enum in `Republic.Core.NationalYield` identifying the 14 canonical categories:
     - **Economic / Productive Capacity**:
       1. `RepuTreasuryRevenue` (monetary output)
       2. `IndustrialCapacity` (manufacturing and fabrication throughput)
       3. `EnergyCapacity` (power grid and electrical reserves)
       4. `InfrastructureCapacity` (transport, logistics, and port throughput)
       5. `FinancialCapacity` (banking capitalization and credit depth)
     - **Human / Knowledge Capacity**:
       6. `HumanCapital` (workforce skills and public vitality)
       7. `ScienceCapacity` (academic and scientific research bandwidth)
       8. `InnovationCapacity` (technology adoption and commercial modernism)
     - **State Capacity**:
       9. `AdministrativeCapacity` (civil service and bureaucratic bandwidth)
       10. `SecurityCapacity` (law enforcement and rule of law strength)
       11. `IntelligenceCapacity` (analysis bandwidth and counter-espionage)
       12. `MilitaryReadiness` (armed forces combat readiness and mobilization)
       13. `DiplomaticCapacity` (treaty leverage and international soft power)
     - **Resource Output**:
       14. `NaturalResourceOutput` (raw strategic material extraction)

2. **Domain Architecture**:
   - `NationalYield`: Root aggregate object containing grouped sub-models:
     - `EconomicCapacities` (`Economic`)
     - `HumanKnowledgeCapacities` (`HumanKnowledge`)
     - `StateCapacities` (`State`)
     - `ResourceOutputCapacities` (`ResourceOutput`)
   - Direct typed properties with `[JsonIgnore]` are exposed on `NationalYield` for ergonomic dot-notation (e.g., `country.Yield.IndustrialCapacity`) alongside category indexers (`GetCapacity(category)` and `SetCapacity(category, value)`).

3. **Country Attachment & Isolation**:
   - Attached to the `Country` entity via `public NationalYield Yield { get; init; } = new();`.
   - Every country owns its own independent `NationalYield` instance.
   - `Country.Found(...)` accepts an optional `yield` parameter and deep-clones it (`yield?.Clone() ?? new NationalYield()`), guaranteeing that no two country instances can ever share mutable yield state.

4. **Serialization and Persistence**:
   - The model is pure POCO, deterministically serializable with `System.Text.Json`.
   - Zero dependencies on Unity, database systems, networking, or UI.
   - Fully compatible with `WorldState` snapshot and restore workflows in `WorldManager`.

5. **No Arbitrary Conversions**:
   - No ad-hoc conversion methods or hardcoded ratios (e.g., "1 Science = 100 REPU") exist at this stage. Capacities are treated as independent capabilities.

## Consequences
- The country simulation now has a typed, persistent foundation to track all productive and institutional capabilities.
- Step 4B (Yield Calculation Engine) and subsequent economic systems can safely compute, update, and inspect yields without modifying the underlying domain identity.

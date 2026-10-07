# ADR-0009: National Yield Numerical Calibration Framework

## Status
Accepted (Framework Only)

## Context
Across Steps 4A through 4E, Republic established:
- The foundational National Yield domain model across 14 canonical categories ([ADR-0004](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0004-national-yield-domain-model.md)).
- The calculation input contract and modifier representation ([ADR-0005](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0005-national-yield-calculation-inputs.md)).
- The six-stage calculation rules and architectural responsibilities ([ADR-0006](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0006-national-yield-calculation-rules.md)).
- The isolated, deterministic calculation engine with a minimal neutral baseline ([ADR-0007](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0007-isolated-national-yield-calculation-engine.md)).
- The sparse, semantically typed conceptual relationship map ([ADR-0008](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0008-national-yield-category-relationships.md)).

Step 4E defined **"what can influence what, and why"** while strictly deferring **"by how much."**

This document establishes the governing numerical calibration framework that bridges conceptual relationships into sensible mathematical calculations.

The objective of this framework is to answer:
> **"What kind of mathematical treatment should each relationship receive?"**
> Rather than:
> **"What exact numbers, fixed formulas, or arbitrary coefficients should we lock in?"**

> [!NOTE]
> **Flexible Framework Scope Boundary**:
> This document guides implementation without pretending that final game-balancing numbers are already known.
> It deliberately does **not** lock Republic into a universal 0–100 scale, fixed numerical ranges, arbitrary coefficients, specific equations, fixed mathematical curves, or hard-coded floors and ceilings.
> Actual balancing will be iteratively tested and calibrated based on gameplay feel and systemic behavior.

---

## Decision

Republic adopts a **bounded, deterministic, and flexible calibration framework** to govern numerical calculations across the six-stage calculation pipeline.

---

## 1. Core Calibration Principles

Any numerical calculation in the National Yield engine must adhere to these foundational principles:

### A. Determinism
Given identical input values on [`NationalYieldCalculationInputs`](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/src/Republic.Core/NationalYield/NationalYieldCalculationInputs.cs) and identical modifiers:
- The calculation engine must evaluate the exact same numerical result every time.
- **Zero in-engine entropy**: `System.Random` is strictly prohibited.
- **Zero clock reliance**: The calculation engine must never query `DateTime.UtcNow` or system time.
- **Zero hidden state**: Calculations must be pure functions with zero static caching, global state, or cross-country memory.

### B. Boundedness & Stability
Simulation mathematics must prevent numerical instability, runaway compounding, and unmanageable divergence:
- **No Infinite Exponential Runaway**: Upstream improvements must never create unbounded compounding loops.
- **Controlled Growth**: A strong or developing country must not produce absurdly huge numbers that break simulation balance or UI presentation.
- **Non-Negativity Invariant**: Every National Yield capacity, output, and flow must remain $\ge 0.0$ at all times.

### C. Monotonicity
Systemic relationships must behave predictably and intuitively:
- **Directional Consistency**: Where an approved relationship is beneficial, increasing an upstream capability should not unexpectedly reduce the downstream capability.
- **Explicit Trade-offs**: If a negative consequence or friction is intentionally designed (e.g. bureaucratic overhead or military maintenance drag), it must be an explicit, understandable mechanism rather than an accidental byproduct of opaque polynomial formulas.

### D. Sensible Diminishing Returns
Real-world national capabilities experience diminishing marginal productivity:
- The 100th unit of infrastructure or research investment should not necessarily deliver the same transformative marginal gain as the 1st.
- As a country approaches advanced development, investments should yield diminishing marginal gains to prevent runaway gaps between nations.
- Diminishing returns should be introduced where conceptually appropriate (e.g. mature infrastructure networks, specialized frontier research), without forcing every category into the same rigid mathematical curve.

### E. Sensible Thresholds Where Needed
Certain national capabilities require foundational baselines before dependent systems can operate effectively:
- Industrial manufacturing requires a working baseline of energy and transport infrastructure to function at scale.
- Natural resource extraction requires logistics corridors to convey raw materials.
- Where thresholds are needed, they should be designed to avoid brittle, gamey cutoffs or erratic numerical oscillations.

### F. Capacity vs. Flow Rigor
The engine strictly preserves the distinction between persistent capability and temporal generation:
- **Capacities** are persistent operational ratings representing what the country can sustain at any moment. They do **not** accumulate into bank accounts.
- **Flows** (specifically REPU Treasury Revenue) represent dynamic generation rates evaluated over elapsed time.
- **Distinct Semantics**: A country's capacities must not be arbitrarily converted into money, nor should all categories be forced into the same unit.

---

## 2. Mathematical Treatment by Relationship Type

Building directly upon the controlled vocabulary established in [ADR-0008](file:///c:/Users/WALE/Republic%20-%20Game/Republic-Game/Docs/Architecture/Decisions/ADR-0008-national-yield-category-relationships.md), relationships receive mathematical treatments tailored to their conceptual role:

### 1. Direct Relationships
- **Role**: Forms the immediate substrate or core driver of the downstream capability.
- **Mathematical Treatment**: The downstream category scales directly with the upstream capability. Direct relationships should remain transparent and understandable, incorporating diminishing returns where appropriate rather than runaway compounding.

### 2. Enabling Relationships
- **Role**: Provides the physical throughput, logistics, or operational foundation that allows another capability to function.
- **Mathematical Treatment**: Acts as an operational support or bottleneck constraint. If an enabling capability (such as energy or transport) is severely deficient, the dependent capability is constrained. When enabling capabilities are strong, dependent capabilities operate smoothly.

### 3. Governance Relationships
- **Role**: Modulates institutional conversion efficiency, execution integrity, and reduction of administrative leakage.
- **Mathematical Treatment**: Acts as an institutional realization factor. Strong administration and stability ensure that national potential is effectively converted into realized output, whereas weak administration results in administrative friction, poor service delivery, and revenue leakage.

### 4. Conditional Relationships
- **Role**: Unlocks or scales latent potential only when prerequisite conditions, natural endowments, or facilities exist.
- **Mathematical Treatment**: Activated when relevant prerequisites (such as territorial resource endowments or matching processing facilities) are present.

### 5. External Relationships
- **Role**: Sovereign capabilities oriented outward toward defense, deterrence, or international relations.
- **Mathematical Treatment**: Evaluated as sovereign output metrics consumed by external subsystems (diplomacy, defense), without creating intra-tick feedback loops inside National Yield.

---

## 3. Scale Flexibility & Input/Output Handling

Republic deliberately avoids artificial, rigid numerical constraints:

### No Universal 0–100 Scale Mandate
- Republic does **not** force every input or output into a rigid 0–100 range.
- While benchmark ratings often fall in intuitive ranges for developing and developed nations, the architecture supports natural growth, specialized nations, and evolving gameplay needs without artificial ceilings or arbitrary clamping.

### Different Categories Behave Differently
Categories represent fundamentally different dimensions of a nation:
- **Operational Capacities** (e.g. Industrial Capacity, Military Readiness, Administrative Capacity) represent structural capabilities.
- **Physical / Utility Capacities** (e.g. Energy Capacity, Infrastructure Capacity) represent physical network throughput and availability.
- **Extractive Volumes** (e.g. Natural Resource Output) represent physical material extraction from territorial deposits.
- **Monetary Flows** (e.g. REPU Treasury Revenue) represent sovereign monetary income generated over time.

Because these categories represent different realities, they should not be treated as interchangeable or forced into identical numerical scales.

---

## 4. Baseline & Zero-State Qualitative Behavior

The model must remain robust and well-behaved across all possible national conditions:
- **Zero or Very Low Inputs**: A country with collapsed infrastructure, zero energy, or failed administration should face severe bottlenecks and heavy leakage, but the engine must never produce negative numbers, divide by zero, or crash.
- **Moderate / Developing State**: Standard capabilities produce sensible, balanced output where developmental investments yield noticeable, intuitive improvements.
- **High / Advanced State**: Advanced nations achieve strong output across multiple categories, with sensible diminishing returns preventing runaway exponential dominance.

---

## 5. Category-Specific Calibration Guidelines

The following guide outlines the expected conceptual behavior for each of the 14 canonical categories:

| Category | Role | Primary Upstream Drivers | Expected Behavior & Considerations |
|----------|------|--------------------------|-----------------------------------|
| **REPU Treasury Revenue** | Monetary Flow | Industry, Resources, Finance, Admin | Generated from formal economic activity and resources, modulated by administrative tax collection and anti-leakage integrity. |
| **Industrial Capacity** | Capacity | Economic Base, Energy, Infrastructure, Innovation | Physical manufacturing throughput; supported and unbottlenecked by adequate power and transport logistics. |
| **Energy Capacity** | Capacity (Utility) | Infrastructure, Natural Resources, Innovation | Sovereign power generation and grid distribution; requires physical infrastructure to deliver power. |
| **Infrastructure Capacity** | Capacity | Infrastructure Condition, Industry, Admin | Physical logistics networks (ports, rail, roads); sustained and maintained through public administration and industrial materials. |
| **Financial Capacity** | Capacity | Economic Base, Stability, Security | Commercial banking depth and credit liquidity; requires economic volume and confidence backed by rule of law. |
| **Human Capital** | Capacity | Population Skill, Education, Health, Admin | Workforce capability and resilience; nurtured through public administration and social services. |
| **Science Capacity** | Capacity | Science / Tech Base, Human Capital, Admin | Foundational research and discovery; fueled by educated human talent and institutional support. |
| **Innovation Capacity** | Capacity | Science, Human Capital, Finance | Commercial technology adoption and modernization; translates scientific knowledge into applied productivity. |
| **Administrative Capacity** | Governance | Government Effectiveness, Human Capital | Institutional competence and civil service execution; converts national potential into realized public outcomes. |
| **Security Capacity** | Governance / Strategic | Security Level, Administration, Intelligence | Law enforcement and civil order; provides a safe operating environment for society and commerce. |
| **Intelligence Capacity** | Strategic | Security Base, Administration, Innovation | Early warning and strategic analysis; supports domestic security and military situational awareness. |
| **Military Readiness** | Strategic | Security Level, Industry, Human Capital | Armed forces defense posture; depends on trained personnel and industrial equipment rather than generating revenue. |
| **Diplomatic Capacity** | Strategic | Administration, Stability, Military | Sovereign international leverage and negotiating bandwidth; reflects institutional competence and strategic strength. |
| **Natural Resource Output** | Output | Territorial Endowments, Infrastructure, Energy | Extractive volume of minerals, timber, and fuels; matters where deposits exist and transport corridors can move them. |

---

## 6. What Must Never Happen (Mathematical Anti-Patterns)

To protect the simulation from arbitrary and fragile balancing hacks, the following practices are strictly forbidden:
1. **Arbitrary Magic Numbers**: Avoid arbitrary multipliers (such as `×1000` or `×0.5`) or flat percentage bonuses (such as `+10%`) detached from causal mechanics.
2. **All-to-All Dense Networks**: Avoid having every category influence every other category. Causal relationships must remain sparse, intuitive, and defensible.
3. **Intra-Tick Circular Calculation**: No category may depend on another category that simultaneously depends on it within the same calculation pass.
4. **Direct Science $\rightarrow$ REPU Conversion**: Science does not directly mint money; it creates knowledge that commercial enterprise applies.
5. **Direct Military $\rightarrow$ REPU Conversion**: Military readiness does not generate government revenue; it provides security and deterrence.
6. **Food Inside National Yield**: Food is produced by the civilian economy and is strictly excluded from National Yield.
7. **National Yield as GDP**: National Yield represents sovereign capabilities, not a single composite GDP number.
8. **Randomness & Clock Dependencies**: Zero random number generation and zero system clock dependencies inside the calculation engine.

---

## 7. Multiplayer Fairness & Balance Principles

Republic is fundamentally a persistent multiplayer nation-building simulation. The numerical calibration framework ensures long-term competitive health:
- **No Early-Game Snowballing**: An early developmental lead must not automatically create an insurmountable, exponential advantage.
- **Multiple Viable Development Paths**: A focused knowledge economy, an industrial power, a resource exporter, and a balanced republic should each represent viable, interesting sovereign strategies.
- **Meaningful Trade-offs**: Investing heavily in one domain (e.g. military readiness) carries opportunity costs in other areas, preventing countries from maximizing all capabilities simultaneously without trade-offs.

---

## Consequences

- **Clear, Unlocked Guidance**: Developers and designers have an understandable framework that outlines relationship behaviors without locking Republic into arbitrary mathematical equations or premature numerical ranges.
- **Empirical Calibration**: The National Yield calculation engine can implement simple, sensible working calculations and evolve iteratively based on testing and gameplay feel.
- **Preserved Architectural Integrity**: The six-stage calculation pipeline, country isolation, and determinism guarantees remain strictly maintained.

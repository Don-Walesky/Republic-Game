# ADR-0016: Cabinet Appointments Modulating Sovereign State Capacity

## Status
Accepted

## Context
Following the implementation of State Capacity as the sovereign revenue conversion rate ([ADR-0015](ADR-0015-state-capacity-conversion-rate.md)), Phase 4 item 4O introduces executive cabinet appointments directly affecting State Capacity inputs.

In the Republic simulation, state capacity is not a static property or an arbitrary setting; it is steered by the executive leadership appointed to key ministerial portfolios. A competent and honest minister elevates public administration and curbs malfeasance, while vacancy or incompetent leadership degrades state capacity down to institutional bottlenecks.

## Decision

1. **Portfolio Scope**:
   - In this slice (Phase 4O), three ministerial portfolios directly govern specific institutional inputs:
     1. `CabinetPortfolio.Finance` governs `CivilServiceQuality`.
     2. `CabinetPortfolio.Interior` governs `CorruptionControl`.
     3. `CabinetPortfolio.Infrastructure` governs `InfrastructureCondition`.
   - Other portfolios (e.g., Defense, Foreign Affairs) remain unassigned to capacity inputs in this slice.

2. **Normalized Minister Attributes (0.0 to 1.0)**:
   - Each minister ([`Minister`](../../../src/Republic.Core/Cabinet/Models/Minister.cs)) possesses five scores, each clamped strictly between 0.0 and 1.0:
     - `Competence`
     - `Loyalty`
     - `Integrity`
     - `Experience`
     - `PoliticalConnections`
   - Clamping is strictly enforced on assignment. Legacy properties (`CompetenceRating`, `LoyaltyRating`) remain synchronized for backward compatibility.

3. **Domain Calculation Rules for Capacity Inputs**:
   - **Finance**: Minister `Competence` and `Integrity` set `CivilServiceQuality` to their mathematical minimum:
     $$\text{CivilServiceQuality} = \min(\text{Competence}, \text{Integrity})$$
   - **Interior**: Minister `Competence` and `Integrity` set `CorruptionControl` to their mathematical minimum:
     $$\text{CorruptionControl} = \min(\text{Competence}, \text{Integrity})$$
   - **Infrastructure**: Minister `Competence` and `Experience` set `InfrastructureCondition` to their mathematical minimum:
     $$\text{InfrastructureCondition} = \min(\text{Competence}, \text{Experience})$$

4. **Vacant Portfolio Rule**:
   - A vacant portfolio sets its affected institutional input strictly to `0.3`, never `1.0` or `0.0`.
   - When a minister is dismissed or a portfolio is vacated, the corresponding input immediately reverts to `0.3` (`Country.VacantPortfolioFactor`).

5. **Role of Loyalty and Political Connections**:
   - `Loyalty` and `PoliticalConnections` do not raise or alter State Capacity inputs.
   - They are recorded on the minister model and preserved for future political intrigues and faction mechanics slices.

6. **Zero Economic Cost in this Slice**:
   - Appointing a minister spends zero funds from the sovereign treasury (`RepuTreasury`) in this slice.

7. **Sovereign Country Isolation**:
   - Each sovereign country maintains its own ministerial cabinet mapping (`Country.Cabinet`).
   - Appointing or dismissing a minister in Country A has zero side effects on Country B.

8. **Reuse of Existing Cabinet Architecture**:
   - Reuses existing types ([`Minister`](../../../src/Republic.Core/Cabinet/Models/Minister.cs), [`CabinetPortfolio`](../../../src/Republic.Core/Cabinet/Models/CabinetPortfolio.cs), [`ICabinetService`](../../../src/Republic.Core/Cabinet/Services/ICabinetService.cs), and [`CabinetService`](../../../src/Republic.Core/Cabinet/Services/CabinetService.cs)).
   - Does not construct duplicate minister hierarchies or secondary portfolio enums.
   - The authoritative appointment method [`Country.AppointMinister`](../../../src/Republic.Core/World/Models/Country.cs) stores the minister and invokes [`Country.RecalculateCabinetStateCapacity`](../../../src/Republic.Core/World/Models/Country.cs).

9. **Preservation of Core Rules**:
   - The State Capacity score remains the minimum of all seven institutional inputs as established in [ADR-0015](ADR-0015-state-capacity-conversion-rate.md).
   - Founding-day ×10 yield multiplier ([ADR-0012](ADR-0012-founding-day-yield-multiplier-rule.md)) and temporary tapering founding buffs ([ADR-0014](ADR-0014-temporary-tapering-founding-buffs.md)) are preserved unchanged.

10. **CLI Integration**:
    - The existing CLI Cabinet option `[3]` in [`Program.cs`](../../../src/Republic.Cli/Program.cs) is wired to allow the executive player to appoint Finance, Interior, or Infrastructure ministers.
    - Upon appointment, the new State Capacity percentage is immediately recalculated and printed to the console.
    - No new top-level menu numbers are added.

11. **Strict Scope Boundaries**:
    - Does not implement Phase 4P development projects, elections, diplomacy, or networking.

## Consequences
- Executive cabinet personnel choices immediately steer sovereign administrative efficiency and bottleneck resolution.
- Unfilled ministerial vacancies severely constrain sovereign revenue conversion through the 0.3 vacancy penalty.
- Pure country isolation and state capacity calculation rules remain intact.

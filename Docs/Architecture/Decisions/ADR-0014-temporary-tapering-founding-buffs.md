# ADR-0014: Temporary Tapering Founding Buffs Beyond the Founding-Day Multiplier

## Status
Accepted

## Context
Following the establishment of the National Yield cycle execution ([ADR-0010](ADR-0010-national-yield-cycle-execution.md)), the founding-day ×10 yield multiplier rule ([ADR-0012](ADR-0012-founding-day-yield-multiplier-rule.md)), the 6-hour production schedule ([ADR-0012](ADR-0012-six-hour-national-yield-cycle-scheduling.md)), and offline catch-up briefing ([ADR-0013](ADR-0013-offline-yield-catch-up-and-presidential-briefing.md)), Step 4M introduces temporary, post-independence national surge buffs.

In statecraft, the declaration of independence and establishment of sovereign institutions creates intense patriotic, institutional, and international momentum that persists beyond the initial founding day, but gradually tapers as political normality sets in.

## Decision

1. **Seven Temporary Founding Buffs**:
   - The simulation models seven distinct founding buffs:
     1. `Administrative efficiency`
     2. `Innovation drive`
     3. `Development initiative`
     4. `Investor confidence`
     5. `Diplomatic recognition momentum`
     6. `Institution-building`
     7. `Temporary national unity`

2. **Decay Mechanics on the Republic Clock**:
   - Buffs start on the sovereign country's Independence Day on the global Republic clock ([`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs)). Direct calls to real-time APIs (`DateTime.Now`) are strictly prohibited.
   - **Days 0 to 7**: Full strength (1.0 / 100%).
   - **Days 7 to 28**: Linear tapering from 1.0 down to 0.0 over a 21-day window:
     $$\text{factor} = \frac{28.0 - \text{elapsedDays}}{21.0}$$
   - **Day 28 and Beyond**: Exactly zero (0.0 / 0%).

3. **Separation from Founding-Day ×10 Multiplier**:
   - The founding-day ×10 rule ([`IFoundingDayYieldRule`](../../../src/Republic.Core/NationalYield/IFoundingDayYieldRule.cs)) remains strictly active on the country's founding day only.
   - The founding buffs are separate named country modifiers; they do not multiply REPU revenue by 10 again and do not alter the core revenue formula.

4. **Sovereign Country Isolation**:
   - Each sovereign country ([`Country`](../../../src/Republic.Core/World/Models/Country.cs)) maintains its own independent founding moment and state.
   - Buff strength is evaluated strictly per-country; Country B never inherits, shares, or influences Country A's founding buff strength.

5. **Pure Domain Evaluator & Snapshot**:
   - An immutable snapshot model ([`FoundingBuffSnapshot`](../../../src/Republic.Core/NationalYield/FoundingBuffSnapshot.cs)) captures the seven values (0.0 to 1.0) and the evaluation timestamp `AsOfTime`.
   - A pure domain evaluator ([`IFoundingBuffEvaluator`](../../../src/Republic.Core/NationalYield/IFoundingBuffEvaluator.cs) and [`FoundingBuffEvaluator`](../../../src/Republic.Core/NationalYield/FoundingBuffEvaluator.cs)) evaluates buff factors deterministically from founding time and current simulation time.
   - The snapshot is accessible directly on [`Country.FoundingBuffs`](../../../src/Republic.Core/World/Models/Country.cs) via `EvaluateFoundingBuffs`.

6. **CLI Dashboard Presentation**:
   - The executive desk dashboard in [`Program.cs`](../../../src/Republic.Cli/Program.cs) displays the seven founding buff factors formatted as percentages beside the treasury and directly beneath the treasury balance line.
   - No interactive menu options are added.

7. **Offline Catch-Up Briefing Integration**:
   - The presidential briefing ([`OfflineYieldBriefing`](../../../src/Republic.Core/NationalYield/OfflineYieldBriefing.cs)) captures active founding buffs on return, but does not credit additional REPU revenue from them in this slice.

8. **Strict Scope Boundaries**:
   - Does not implement 4N State Capacity, cabinet effects, development projects, diplomacy systems, or networking.

## Consequences
- Sovereign nations possess an authentic, tapering post-independence institutional surge.
- Yield calculations and treasury mechanics remain deterministic and protected against duplicate revenue multipliers.
- Country state isolation and clock purity are preserved.

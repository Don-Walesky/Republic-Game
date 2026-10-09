# ADR-0025: Ruling and Opposition Political Parties

## Status
Accepted

## Context
Following the implementation of the initial single-party founding slice in Phase 6 item 6.1 ([ADR-0024](ADR-0024-player-founded-political-party.md)), Phase 6 item 6.2 expands the sovereign country's political model to accommodate institutional political contestation.

In democratic and representative governance, legitimacy requires structured opposition. A singular political party without opposition risks unchecked autocracy; introducing a recognized opposition party creates the institutional balance required for parliamentary deliberation, oversight, and future electoral competition.

## Decision

1. **Two-Party Country Model**:
   - Extended [`Country`](../../../src/Republic.Core/World/Models/Country.cs) to hold up to two political parties:
     - `RulingParty`: the first party founded in the country.
     - `OppositionParty`: the second party founded in the country.
     - `Party` and `PoliticalParty` aliases remain mapped to `RulingParty` for full backward compatibility with Phase 6.1.
     - `Parties`: read-only list exposing active parties.
   - Introduced [`PoliticalPartyRole`](../../../src/Republic.Core/Politics/PoliticalParty.cs) enum (`Ruling`, `Opposition`) on [`PoliticalParty`](../../../src/Republic.Core/Politics/PoliticalParty.cs), with `IsRuling` and `IsOpposition` helper properties.

2. **Sequential Founding & Role Assignment**:
   - The first party founded via `Country.FoundParty` is designated as the **Ruling Party** with `Role = PoliticalPartyRole.Ruling`.
   - The second party founded via `Country.FoundParty` is designated as the **Opposition Party** with `Role = PoliticalPartyRole.Opposition`.
   - A third founding attempt is strictly rejected (returns `null`), preserving the two existing parties.

3. **Name Validation and Duplicate Rejection**:
   - Party names must be trimmed and non-empty. An empty or whitespace-only name is rejected (returns `null`).
   - The two party names within a country cannot match, ignoring case (`string.Equals(..., StringComparison.OrdinalIgnoreCase)`). Attempting to found an opposition party with the same name as the ruling party is rejected (returns `null`).

4. **Public Support Rules**:
   - The opposition party starts with baseline public support of `10.0`.
   - The ruling party's public support remains exactly where it was prior to the opposition's founding.

5. **Country Isolation & Temporal Determinism**:
   - Sovereign country isolation is strictly maintained: Country B does not receive Country A's parties, and parties have distinct `CountryId` references.
   - The founding moment is anchored strictly to the Republic simulation clock (`RepublicTime` / `IRepublicClock`); never calls `DateTime.Now` or `DateTime.UtcNow`.

6. **CLI Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the systemic consequence line.
   - If no parties exist, prints `"Party: No party"`.
   - If parties exist, prints both parties marked `Ruling:` and `Opposition:` (e.g. `Ruling: Civic Union (Support: 10%)`, `Opposition: National Vanguard (Support: 10%)`).
   - No interactive menu number is added.

7. **Scope Boundaries**:
   - Strictly confined to Phase 6 item 6.2 (a country can hold two parties).
   - Does not implement elections, legislative seats, a parliament, interest groups, diplomacy, or military systems.
   - Does not rewrite National Yield, treasury, systemic consequences, or national strength.

## Consequences
- Establishes a competitive two-party governance framework (Ruling vs Opposition).
- Preserves full backward-compatibility with Phase 6.1 consumers.
- Enforces strict validation, country isolation, and simulation clock determinism.
- Prepares the simulation state for future electoral campaigning and vote calculations.

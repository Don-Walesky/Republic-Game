# ADR-0026: Country Interest Group

## Status
Accepted

## Context
Following the implementation of player-founded political parties in Phase 6 item 6.1 ([ADR-0024](ADR-0024-player-founded-political-party.md)) and the introduction of ruling and opposition parties in Phase 6 item 6.2 ([ADR-0025](ADR-0025-ruling-and-opposition-parties.md)), Phase 6 item 6.3 begins modeling civil society and sectoral factional power within a sovereign nation.

In democratic, oligarchic, and emerging republics alike, formal political parties do not operate in a vacuum. Organized socioeconomic factions—such as trade unions, business federations, agricultural syndicates, student associations, and the civil service apparatus—organize collective interests and wield institutional influence to shape national policy and stability.

## Decision

1. **Interest Group Record Specification**:
   - Implemented as an immutable domain record [`InterestGroup`](../../../src/Republic.Core/Politics/InterestGroup.cs) in `Republic.Core.Politics`.
   - Attributes:
     - `Name`: trimmed string, non-empty. An empty or whitespace-only name is rejected with an `ArgumentException` on direct construction.
     - `CountryId`: the unique identifier of the owning sovereign nation.
     - `Focus`: societal or economic focus categorized by [`InterestGroupFocus`](../../../src/Republic.Core/Politics/InterestGroup.cs) enum:
       - `Labor`: organized workforce and trade unions.
       - `Business`: commercial confederations and industrial syndicates.
       - `Farmers`: agrarian associations and rural cooperatives.
       - `Students`: academic and youth coalitions.
       - `CivilService`: state bureaucratic apparatus and administrative unions.
     - `Influence`: public influence rating from `0.0` to `100.0`, starting at baseline `15.0`.

2. **Single Interest Group Constraint & Rejection**:
   - In this slice, a sovereign nation can have at most one interest group.
   - Evaluated via `FoundInterestGroup` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - If an interest group has already been founded for the nation, any subsequent `FoundInterestGroup` invocation is rejected (returns `null`), preserving the initial group and its influence rating intact.
   - An empty or whitespace-only name is rejected and does not instantiate a group (returns `null`).

3. **Country Isolation**:
   - Country B does not receive Country A's interest group. Each sovereign country owns an independent, isolated `InterestGroup` property.
   - Founding an interest group in Country A induces zero mutations or side-effects in Country B.

4. **Temporal Determinism**:
   - Founding and managing interest groups is strictly deterministic and never calls `DateTime.Now` or `DateTime.UtcNow`.

5. **CLI Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the political party lines.
   - Displays `" Interest Group: No interest group"` if none exists, or `" Interest Group: {group}"` (formatted with name, focus, and influence percentage) when present.
   - Does not add an interactive directive menu number.

6. **Scope Boundaries**:
   - Strictly confined to Phase 6 item 6.3 (a country can have one interest group).
   - Does not implement elections, a legislature, diplomacy, or multiplayer.
   - Does not alter existing political party rules or national development projects.

## Consequences
- Establishes Republic's initial civil society interest group model with strong encapsulation and value equality.
- Enforces sovereign country isolation, validation rules, and simulation determinism.
- Prepares the state representation for future policy negotiation, lobbying, and factional pressure without premature architectural complexity.

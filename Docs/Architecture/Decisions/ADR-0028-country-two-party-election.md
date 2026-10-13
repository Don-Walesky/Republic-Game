# ADR-0028: Country Two-Party Democratic Election

## Status
Accepted

## Context
Following the establishment of political parties in Phase 6 item 6.1 ([ADR-0024](ADR-0024-player-founded-political-party.md)), the introduction of ruling and opposition parties in Phase 6 item 6.2 ([ADR-0025](ADR-0025-ruling-and-opposition-parties.md)), civil society interest groups in Phase 6 item 6.3 ([ADR-0026](ADR-0026-country-interest-group.md)), and legislative bill voting in Phase 6 item 6.4 ([ADR-0027](ADR-0027-country-bill-vote.md)), Phase 6 item 6.5 introduces the foundational democratic mechanism for transferring or renewing executive mandates: holding an election between a country's two political parties.

Democratic legitimacy requires a mechanism for political turnover grounded in popular support. When an election is held, public support ratings determine whether the incumbent ruling party retains executive authority or whether the opposition assumes government power.

## Decision

1. **CountryElection Record Specification**:
   - Implemented as an immutable domain record [`CountryElection`](../../../src/Republic.Core/Politics/CountryElection.cs) in `Republic.Core.Politics`.
   - Attributes:
     - `CountryId`: the unique identifier of the owning sovereign nation.
     - `Boundary`: the authoritative Republic simulation clock boundary (`RepublicTime`) when the election was held.
     - `WinnerPartyName`: the name of the victorious political party.
     - `LoserPartyName`: the name of the defeated political party.
     - `WinnerSupport`: the public support rating of the winner at election time.
     - `LoserSupport`: the public support rating of the loser at election time.

2. **Prerequisites & Rejection Rule**:
   - Executed via `HoldElection` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - An election strictly requires both an active ruling party (`RulingParty != null`) and an active opposition party (`OppositionParty != null`).
   - If either party is missing, the election is rejected (returns `null`), and no political roles or support values are altered.

3. **Plurality Support & Incumbency Tiebreaker**:
   - The party with higher `Support` wins the election.
   - On a tie (`RulingParty.Support == OppositionParty.Support`), the incumbent ruling party wins.

4. **Institutional Role Transition**:
   - The winning party assumes the ruling mandate (`Role = PoliticalPartyRole.Ruling`).
   - The losing party assumes the opposition role (`Role = PoliticalPartyRole.Opposition`).
   - If the opposition wins, the parties swap roles cleanly via immutable record updates.

5. **Boundary-Anchored Idempotence**:
   - A second election call at the same Republic simulation clock boundary does nothing: it returns the existing election record without swapping roles again.

6. **Country Isolation & Clock Determinism**:
   - Country B does not receive or participate in Country A's election.
   - Anchored strictly to Republic simulation clock boundaries (`RepublicTime` / `IRepublicClock`). Never calls `DateTime.Now` or `DateTime.UtcNow`.

7. **CLI Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the legislative bill line.
   - Displays `" Election: No election"` if none has been held, or `" Election: {election}"` (displaying winner, boundary, and vote percentages) when present.
   - Does not add an interactive directive menu number.

8. **Scope Boundaries**:
   - Strictly confined to Phase 6 item 6.5 (holding one election between two parties).
   - Does not implement campaign seasons, parliamentary seats, or multiplayer.
   - Does not alter bill, interest group, or party founding mechanics.

## Consequences
- Implements the foundational democratic transition mechanism in Republic's political core.
- Enforces institutional contestation, incumbency tiebreakers, and role inversion.
- Preserves full temporal determinism, idempotency, and sovereign country isolation.

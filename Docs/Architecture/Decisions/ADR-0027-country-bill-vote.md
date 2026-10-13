# ADR-0027: Country Legislative Bill Voting

## Status
Accepted

## Context
Following the establishment of political parties in Phase 6 item 6.1 ([ADR-0024](ADR-0024-player-founded-political-party.md)), the introduction of ruling and opposition parties in Phase 6 item 6.2 ([ADR-0025](ADR-0025-ruling-and-opposition-parties.md)), and civil society interest groups in Phase 6 item 6.3 ([ADR-0026](ADR-0026-country-interest-group.md)), Phase 6 item 6.4 introduces the initial legislative enactment capability for sovereign countries.

Governance requires legislative mechanisms through which executive proposals transition from proposed initiatives into enacted statute. In this initial slice, legislative enactment is modeled at the country level without introducing the full complexity of parliamentary seat allocations, roll-call voting, or electoral chambers.

## Decision

1. **CountryBill Record Specification**:
   - Implemented as an immutable domain record [`CountryBill`](../../../src/Republic.Core/Politics/CountryBill.cs) in `Republic.Core.Politics`.
   - Attributes:
     - `Title`: trimmed string, non-empty. An empty or whitespace-only title is rejected with an `ArgumentException` on direct construction.
     - `CountryId`: the unique identifier of the owning sovereign nation.
     - `Status`: legislative status categorized by [`CountryBillStatus`](../../../src/Republic.Core/Politics/CountryBill.cs) enum:
       - `Pending`: bill is proposed and awaiting a vote.
       - `Passed`: bill was voted on and successfully enacted into law.
       - `Failed`: bill was voted on and rejected.
     - `VotedBoundary`: the authoritative Republic simulation clock boundary (`RepublicTime?`) when voted, set only when a vote occurs (null while `Pending`).

2. **Single Pending Bill Constraint**:
   - A sovereign nation can have at most one bill in `Pending` status at any time.
   - Evaluated via `ProposeBill` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - If a pending bill already exists for the nation, any subsequent `ProposeBill` invocation is rejected (returns `null`), preserving the initial pending bill intact.
   - An empty or whitespace-only title is rejected and does not instantiate a bill (returns `null`).

3. **Ruling Party Vote Requirement & Passage Rule**:
   - Voting is executed via `VoteBill` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs).
   - Voting requires an active ruling party (`RulingParty != null`).
   - If no ruling party exists in the country, the vote is rejected and the bill remains `Pending`, leaving `VotedBoundary` as `null`.
   - If the ruling party exists, the bill passes (`Status = CountryBillStatus.Passed`). The opposition party does not block the bill in this slice.
   - The vote moment records the authoritative clock boundary (`VotedBoundary = votedBoundary`).

4. **Voting Idempotence & Boundary Preservation**:
   - Voting on the same bill twice is a no-op: it preserves the `Passed` status and leaves the original `VotedBoundary` unchanged.

5. **Country Isolation & Clock Determinism**:
   - Country B does not receive Country A's bill. Each sovereign country maintains its own bills isolated by `CountryId`.
   - The vote boundary is derived strictly from the Republic simulation clock (`RepublicTime` / `IRepublicClock`). Never calls `DateTime.Now` or `DateTime.UtcNow`.

6. **CLI Presentation**:
   - Rendered in [`Program.cs`](../../../src/Republic.Cli/Program.cs) in the Presidential Desk dashboard directly beneath the interest group line.
   - Displays `" Bill: No bill"` if none exists, or `" Bill: {bill}"` (displaying title, status, and vote boundary when voted).
   - Does not add an interactive directive menu number.

7. **Scope Boundaries**:
   - Confined strictly to Phase 6 item 6.4 (a country can vote on one bill).
   - Does not modify existing political party or interest group rules.
   - Does not edit `src/Republic.Core/Legislature`, `ElectionService`, `Diplomacy`, or `Networking`.
   - All new types reside under `Republic.Core.Politics`.

## Consequences
- Implements the foundational legislative workflow (proposal -> vote -> enactment) anchored to sovereign country state.
- Enforces parliamentary prerequisites (ruling party mandate required for passage).
- Guarantees determinism, immutability, and sovereign country isolation.

# ADR-0030: Bilateral Diplomatic Agreements, Recognition, and Alliances

## Status
Accepted

## Context
Following the establishment of foundational directed diplomatic relations in Phase 7.1 and interactive menu navigation in Phase 7.2 ([ADR-0029](ADR-0029-two-country-diplomatic-relation.md)), Phase 7 completes the diplomatic relationship lifecycle so that international relations become a fully usable and functional part of the presidential gameplay experience.

Rather than merely recording that a unilateral relation exists, presidential governance requires:
1. Formal diplomatic recognition upgrading informal ties.
2. Bilateral diplomatic agreements (trade agreements) governed by proposal, consideration, and acceptance/rejection lifecycles.
3. Sovereign bilateral alliances anchored to established, recognized diplomatic ties.
4. An interactive, transparent executive interface where foreign nations' decisions are resolved deterministically rather than assumed automatically.

## Decision

1. **Formal Diplomatic Recognition**:
   - Implemented via [`Country.RecognizeRelation`](../../../src/Republic.Core/World/Models/Country.cs) and coordinated through [`BilateralDiplomacyHandler.RecognizeRelation`](../../../src/Republic.Core/World/BilateralDiplomacyHandler.cs).
   - Requires an existing open relation; attempts to recognize an unopened relation are rejected (`DiplomaticActionResult.RelationNotFound`).
   - Updates the stored [`DiplomaticRelation`](../../../src/Republic.Core/World/DiplomaticRelation.cs) on the sovereign country to `DiplomaticRelationStatus.Recognized` and sets `RecognizedBoundary`.
   - Idempotent and safe: repeating recognition on an already recognized relation returns the existing record without duplicating entries or modifying timestamps.
   - Displayed in the Presidential Desk dashboard and [`NationalDashboard`](../../../src/Republic.Core/World/Models/NationalDashboard.cs).

2. **Bilateral Diplomatic Agreements Domain Model**:
   - Implemented as [`DiplomaticAgreement`](../../../src/Republic.Core/World/DiplomaticAgreement.cs) with enums [`DiplomaticAgreementType`](../../../src/Republic.Core/World/DiplomaticAgreement.cs) (`Trade`, `Alliance`) and [`DiplomaticAgreementStatus`](../../../src/Republic.Core/World/DiplomaticAgreement.cs) (`Pending`, `Accepted`, `Rejected`, `Terminated`).
   - Properties: `Id`, `SourceCountryId`, `TargetCountryId`, `AgreementType`, `Status`, `ProposedBoundary`, and `DecidedBoundary`.
   - Attached to participating sovereign countries via `_agreements` on [`Country`](../../../src/Republic.Core/World/Models/Country.cs) and queried via `Agreements`, `ActiveAgreements`, and `PendingAgreements`.

3. **Trade Agreement Lifecycle & Prerequisites**:
   - A country can propose a bilateral trade agreement to a foreign nation only if an open diplomatic relation exists.
   - Proposals start in `DiplomaticAgreementStatus.Pending` at the authoritative `ProposedBoundary`.
   - The target country resolves the proposal deterministically through `AcceptAgreement` or `RejectAgreement`, transitioning status to `Accepted` or `Rejected` and recording the authoritative `DecidedBoundary`.
   - Duplicate active or pending trade agreements between the same country pair are prohibited and rejected.

4. **Bilateral Alliances**:
   - Modeled through the shared agreement architecture using `DiplomaticAgreementType.Alliance`.
   - Prerequisite: requires an established diplomatic relationship (the open relation must be formally `Recognized`). Proposing an alliance on an unrecognized relation is rejected with `DiplomaticActionResult.PrerequisitesNotMet`.
   - Governed by the same proposal, acceptance, and rejection lifecycle.
   - Duplicate active or pending alliances are rejected.
   - Scope boundary: does not introduce military obligations, automated war participation, or global alliance blocs in this phase.

5. **Sovereign Country Isolation & Non-Interference**:
   - Bilateral relations and agreements are strictly confined to the participating sovereign nations.
   - External nations (such as Country C) remain completely untouched and isolated when Country A and Country B establish agreements or alliances.

6. **Deterministic Simulation Time**:
   - All lifecycle events (`OpenedBoundary`, `RecognizedBoundary`, `ProposedBoundary`, `DecidedBoundary`) are strictly anchored to the authoritative Republic simulation clock ([`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs) / [`IRepublicClock`](../../../src/Republic.Core/Time/IRepublicClock.cs)).
   - Strictly prohibits calls to `DateTime.Now` or `DateTime.UtcNow`.

7. **Interactive CLI Diplomacy Menu**:
   - Executive Directive menu option `[14] Diplomacy & International Relations` in [`Program.cs`](../../../src/Republic.Cli/Program.cs) allows the player to:
     1. View registered foreign countries with relation, trade, and alliance statuses.
     2. Inspect a country's diplomatic dossier (relation status, trade status, alliance status, national demographics).
     3. Open a diplomatic relation if none exists.
     4. Formally recognize an existing open relation.
     5. Propose a bilateral trade agreement or alliance where eligible.
     6. Review and deterministically resolve pending proposals (accept or reject).
     7. Safely cancel at every menu prompt and return to the main executive directive menu.

## Consequences
- Completes Phase 7 ("Foundational Diplomacy") with full lifecycle support for recognition, trade agreements, and alliances.
- Maintains strict simulation clock determinism, sovereign country isolation, and testability across all domain models and CLI interactions.
- Lays the clean structural foundation for future phases without introducing premature military, espionage, or complex multilateral treaty systems.

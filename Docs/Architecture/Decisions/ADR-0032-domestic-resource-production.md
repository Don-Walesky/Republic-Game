# ADR-0032: Domestic Resource Production from Associated Resource Nodes

## Status
Accepted

## Context
Following the completion of Phase 8 ([ADR-0031](ADR-0031-bilateral-trade-economic-transactions.md)), sovereign countries can transfer accumulated physical goods and REPU via bilateral trade. However, countries possessed no mechanism to replenish or extract commodities domestically, leaving economies reliant solely on static initial endowments.

To establish sustainable national economies without introducing premature extraction formulas, complex industrial supply chains, or global commodity exchanges, Phase 9 requires:
1. Hardening trade, inventory, and treasury models against non-finite floating point inputs (`NaN`, positive infinity, negative infinity).
2. Explicit domestic association between sovereign countries and natural or strategic resource nodes.
3. Domestic resource production that adds physical goods directly to a country's existing `ResourceInventory`.
4. Alignment with the authoritative Republic simulation clock and established six-hour cycle boundaries (00:00, 06:00, 12:00, 18:00 WAT).
5. Strict duplicate execution prevention ensuring a country cannot receive production twice for the same cycle boundary.
6. Safeguards preventing production from missing, unassociated, inactive, or depleted resource nodes.
7. Sovereign isolation ensuring non-participating nations experience zero inventory or balance alterations.
8. Visible presentation of domestic resource stockpiles before and after production cycles in the executive command interface.

## Decision

1. **Non-Finite Floating Point Rejection**:
   - Hardened [`BilateralTradeTerms`](../../../src/Republic.Core/World/BilateralTradeTerms.cs) to reject non-finite `quantity` and `price` via `double.IsFinite`.
   - Hardened [`ResourceInventory`](../../../src/Republic.Core/World/ResourceInventory.cs) (`Add`, `Remove`, `TryRemove`, `HasSufficient`, constructor) to reject non-finite quantities.
   - Hardened [`RepuTreasury`](../../../src/Republic.Core/Economy/Treasury/RepuTreasury.cs) (`Deposit`, `Withdraw`, `TryWithdraw`, constructor, `Balance` property) to reject non-finite monetary amounts.

2. **Resource Node Domain Model Enhancements**:
   - Extended [`ResourceNode`](../../../src/Republic.Core/World/Models/ResourceNode.cs) with:
     - `CountryId`: unique identifier of the sovereign nation owning/associating with the node.
     - `Name`: human-readable identifier (e.g., "Arcadia Iron Basin").
     - `IsActive`: boolean extraction state (defaults to `true`). Inactive nodes produce 0 goods.
     - `DefaultProductionRate`: explicit, documented baseline extraction rate (50.0 units per six-hour cycle).
     - Validated `Abundance` and `ExtractionRate` to reject non-finite and negative values.
     - `IsAssociatedWith(string countryId)` helper.

3. **Country Node Association**:
   - Extended [`Country`](../../../src/Republic.Core/World/Models/Country.cs) with:
     - `ResourceNodes`: list of domestic resource nodes associated with the sovereign country.
     - `AssociateResourceNode(ResourceNode node)`: registers association and guarantees matching `node.CountryId`.
     - `Country.Found` factory updated to support optional `resourceNodes` collection parameter.
     - Seeded factories ([`CountryProfileFactory`](../../../src/Republic.Core/World/Factories/CountryProfileFactory.cs)) and scenario bootstrappers ([`ScenarioBootstrapper`](../../../src/Republic.Core/Scenarios/Services/ScenarioBootstrapper.cs)) associate domestic nodes with created sovereign states.

4. **Resource Production Service & Engine**:
   - Implemented [`IResourceProductionService`](../../../src/Republic.Core/World/Services/IResourceProductionService.cs) and [`ResourceProductionService`](../../../src/Republic.Core/World/Services/ResourceProductionService.cs):
     - `Produce(Country country, ResourceNode node, RepublicTime boundary)` executes a single six-hour production cycle.
     - Validates:
       1. Node presence (null checks).
       2. Node active status (`node.IsActive`).
       3. Explicit association (`node.CountryId == country.Id` or `country.ResourceNodes.Contains(node)`).
       4. Authoritative boundary alignment (`NationalYieldSchedule.IsBoundary(boundary)`: 00:00, 06:00, 12:00, 18:00 WAT).
       5. Duplicate boundary idempotency via deterministic key `$"prod-{country.Id}-{node.Id}-{boundary.DayNumber}-{boundary.Hour}"`.
     - Extracts physical goods according to `node.ExtractionRate` (or `ResourceNode.DefaultProductionRate`), deducting from `node.Abundance` if non-renewable.
     - Deposits extracted units into `country.Inventory.Add(...)`.
     - Records credited boundary via `country.RecordProductionBoundaryApplied(...)` and `country.LastResourceProductionBoundary`.
     - Returns [`ResourceProductionResult`](../../../src/Republic.Core/World/ResourceProductionResult.cs).
     - Provides `ProduceDueCycles` for automatic cycle catch-up when advancing simulation time.

5. **Sovereign Country Isolation**:
   - Domestic production operates strictly upon the target country's inventory and associated nodes.
   - External nations' inventories, treasuries, relations, and agreements remain completely untouched.

6. **CLI Dashboard & Stockpile Transparency**:
   - Updated Presidential Desk in [`Program.cs`](../../../src/Republic.Cli/Program.cs) (`RenderDashboard`):
     - Displays current domestic stockpiles and active node status directly on the national overview.
   - Updated `AdvanceTimeAsync` in [`Program.cs`](../../../src/Republic.Cli/Program.cs):
     - Automatically executes due domestic production cycles when time advances.
     - Prints a clear before-and-after stockpile comparison and cycle extraction log.

7. **Phase 9.1 Reliability Enhancements (Multi-Node Tracking & Strict Ownership)**:
   - **Per-Node Cycle Tracking**:
     - Added `LastProductionBoundary` to [`ResourceNode`](../../../src/Republic.Core/World/Models/ResourceNode.cs). Each node independently tracks its most recently credited six-hour boundary, decoupling node extraction from the shared country-level boundary.
     - `ProduceDueCycles` evaluates due cycle boundaries independently per node starting from `node.LastProductionBoundary ?? (country.IsFounded ? country.FoundingTime : null)`.
     - Depleted or inactive nodes process without blocking or skipping other eligible domestic nodes.
     - Repeated catch-up calls safely yield zero duplicate extractions.
   - **Strict Resource Ownership Enforcement**:
     - Updated `Country.AssociateResourceNode(ResourceNode node)` to return `bool` and explicitly reject nodes already owned by another sovereign country (`node.CountryId != country.Id`), preserving legitimate unassigned-node association (`string.IsNullOrWhiteSpace(node.CountryId)`).
     - Hardened `ResourceProductionService.Produce` and `ProduceDueCycles` to explicitly reject mismatched node ownership regardless of list membership.
     - Failed ownership checks leave country stockpiles, foreign stockpiles, and node abundance 100% unaltered.

## Consequences
- Provides a sustainable, domestic economic loop allowing sovereign nations to replenish stockpiles and engage in sustained trade.
- Eliminates potential corruption from non-finite numerical inputs (`NaN`, `Infinity`) across economic systems.
- Ensures all eligible domestic resource nodes produce at every due six-hour boundary without cycle starvation or cross-node interference.
- Enforces inviolable sovereign resource node ownership, guaranteeing that foreign nations cannot associate or extract each other's resources.
- Reuses the authoritative Republic simulation clock and six-hour cycle schedule without creating redundant timers or separate production loops.
- Maintains strict testability and full backward-compatibility across all Phase 0-8 subsystems.

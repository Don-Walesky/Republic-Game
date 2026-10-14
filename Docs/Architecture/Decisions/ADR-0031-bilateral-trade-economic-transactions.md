# ADR-0031: Bilateral Trade Economic Transactions and Resource Inventory

## Status
Accepted

## Context
Following the completion of Phase 7 foundational diplomacy ([ADR-0030](ADR-0030-bilateral-agreements-and-alliances.md)), countries can open relations, formally recognize each other, and enter into bilateral agreements. However, trade agreements initially lacked an economic mechanism, functioning only as nominal treaty records.

To make bilateral trade economically meaningful without introducing premature macro-level commodity pricing or forex complexity, Republic requires:
1. An explicit bilateral trade transaction model recording terms (seller, buyer, commodity type, quantity, price in REPU).
2. A physical resource inventory foundation on sovereign countries distinct from National Yield cycles (commodities are physical goods, not free treasury deposits).
3. Mathematical conservation: no money or goods may be created from nothing or destroyed upon transfer.
4. Atomic execution upon agreement acceptance: simultaneous payment from the buyer's treasury to the seller's treasury and commodity delivery from the seller's inventory to the buyer's inventory.
5. Strict failure safety and duplicate processing protection: insufficient funds or goods must fail the transaction cleanly with zero mutations to either nation's treasury or inventory, and re-executing a completed deal must be an idempotent no-op.
6. Interactive CLI participation: the player can inspect foreign partners' stockpiles and treasuries, propose export/import trade deals with custom terms, review pending incoming deals, and execute or reject them transparently.

## Decision

1. **Resource Inventory Model**:
   - Implemented as [`ResourceInventory`](../../../src/Republic.Core/World/ResourceInventory.cs) attached directly to [`Country.Inventory`](../../../src/Republic.Core/World/Models/Country.cs).
   - Provides thread-safe stockpiling with `GetQuantity`, `HasSufficient`, `Add`, `Remove`, `TryRemove`, and `Clone`.
   - Strictly prevents negative balances (deficits throw `InvalidOperationException` or return false on `TryRemove`).
   - Distinct from National Yield: physical economic stockpiles represent actual goods, not abstract periodic rate modifiers.

2. **Explicit Bilateral Trade Terms**:
   - Implemented as immutable record [`BilateralTradeTerms`](../../../src/Republic.Core/World/BilateralTradeTerms.cs).
   - Properties: `SellerCountryId`, `BuyerCountryId`, `ResourceType`, `Quantity`, and `Price`.
   - Validates that `Quantity > 0`, `Price > 0`, `ResourceType` is non-empty, and `SellerCountryId != BuyerCountryId`.
   - Attached to [`DiplomaticAgreement.Terms`](../../../src/Republic.Core/World/DiplomaticAgreement.cs) when `AgreementType == DiplomaticAgreementType.Trade`.

3. **Trade Deal Agreement Lifecycle**:
   - Proposing a deal via `Country.ProposeTradeDeal` or `BilateralDiplomacyHandler.ProposeTradeDeal` verifies an open diplomatic relation exists and that no duplicate pending trade agreement is open between the two countries.
   - Initial status is `DiplomaticAgreementStatus.Pending`, recording the authoritative `ProposedBoundary` from [`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs).
   - Receiving nation accepts or rejects explicitly via `Country.AcceptAgreement` / `Country.RejectAgreement` or `BilateralDiplomacyHandler.RespondToAgreement`.

4. **Atomic Economic Settlement**:
   - On acceptance of an agreement with `Terms`, the receiving country verifies preconditions:
     - Buyer has sufficient funds in `RepuTreasury.Balance >= Terms.Price`.
     - Seller has sufficient goods in `ResourceInventory.GetQuantity(Terms.ResourceType) >= Terms.Quantity`.
   - If either check fails, the transaction is rejected (`DiplomaticActionResult.InsufficientFunds` or `DiplomaticActionResult.InsufficientGoods`), returning null/failure without modifying any balance or inventory.
   - If valid, the transaction executes atomically:
     - `buyer.Treasury.TryWithdraw(terms.Price)`
     - `seller.Treasury.Deposit(terms.Price)`
     - `seller.Inventory.Remove(terms.ResourceType, terms.Quantity)`
     - `buyer.Inventory.Add(terms.ResourceType, terms.Quantity)`
   - The agreement transitions to `DiplomaticAgreementStatus.Completed` and records the authoritative `DecidedBoundary`.

5. **Duplicate Processing Prevention & State Consistency**:
   - `DiplomaticAgreementStatus.Completed` prevents any subsequent acceptance calls from re-executing funds or goods transfers.
   - If an agreement is already completed, subsequent response attempts return `DiplomaticActionResult.AlreadyDecided` without state changes.
   - Rejection transitions the agreement to `DiplomaticAgreementStatus.Rejected` without moving funds or goods.

6. **Sovereign Country Isolation**:
   - Economic transactions are strictly bilateral. Third-party countries remain completely untouched in treasury, inventory, relations, and agreements.

7. **Interactive Executive CLI**:
   - Updated Executive Directive menu option `[14] Diplomacy & International Relations` in [`Program.cs`](../../../src/Republic.Cli/Program.cs):
     - Inspect foreign partner dossiers including primary resource, current stockpile, and treasury balance.
     - Option `[3] Propose Bilateral Trade Deal`: specify export (selling to foreign nation) or import (buying from foreign nation), select resource type, quantity, and price.
     - Option `[4] Review & Resolve Pending Proposals`: view full terms of pending deals and accept or reject them with immediate economic outcome feedback.

## Consequences
- Makes Phase 8 bilateral trade economically real, measurable, and conservative without creating or destroying currency/goods out of nowhere.
- Preserves full backward compatibility with Phase 7 non-trade agreements (alliances) and existing save states.
- All operations adhere strictly to the authoritative Republic simulation clock ([`RepublicTime`](../../../src/Republic.Core/Time/RepublicTime.cs) / [`IRepublicClock`](../../../src/Republic.Core/Time/IRepublicClock.cs)).
- Establishes a modular goods inventory foundation ready for future trade routes, domestic production, and multilateral markets.

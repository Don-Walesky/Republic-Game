# ADR-0003: Country Identity & Founding State

## Status
Accepted

## Context
Republic models the **country itself** as the foundational entity of the simulation. Every player-governed nation requires:
- A persistent, authoritative identity.
- An immutable historical founding moment (Independence Day and Independence Time) anchored to the authoritative Republic simulation clock in West Africa Time (WAT / UTC+1).
- The ability to determine national age relative to current Republic simulation time without creating a secondary clock.
- A clear founding lifecycle state (newly founded vs. established).

## Decision
1. **Authoritative Country Identity**:
   - The `Country` domain model in `Republic.Core.World.Models` encapsulates core identity: `Id`, `Name`, `CapitalCity`, `GovernmentType`, and `TerritorySizeSqKm`.
   - Historical founding properties are directly exposed:
     - `FoundingTime` (`RepublicTime` value struct)
     - `IndependenceDay` (`DateOnly` in Gregorian calendar, WAT)
     - `IndependenceTime` (`TimeOnly` in WAT)
     - `FoundingRepublicDay` (`long` day number from launch)
     - `FoundingStatus` (`CountryFoundingStatus`: `NewlyFounded` or `Established`)

2. **Integration with the Single Source of Truth Clock**:
   - When a country is founded via `ICountryService.FoundCountry(...)` or registered via `ICountryService.RegisterCountry(...)`, its founding moment is established from `IRepublicClock.CurrentTime`.
   - No secondary or decoupled date/time systems exist.

3. **Protection from Accidental Mutation**:
   - `FoundingTime` is `init`-only, preventing casual property reassignment outside object initialization.
   - For unsealed country instances, `EstablishFounding(RepublicTime)` authoritatively records the founding moment and seals the entity; any subsequent call throws an `InvalidOperationException`.

4. **Derived National Age**:
   - Country age is calculated purely as:
     $$\text{Country Age} = \text{Current Republic Time} - \text{Country Founding Time}$$
   - Implemented via `country.GetAge(RepublicTime)` and `country.GetAge(IRepublicClock)`.
   - Age begins at `TimeSpan.Zero` upon creation and advances monotonically with the global Republic simulation clock.

5. **Founding Lifecycle Status**:
   - `CountryFoundingStatus.NewlyFounded`: Represents a newly proclaimed nation.
   - `CountryFoundingStatus.Established`: Reached via `country.MarkEstablished()`.

6. **Persistence Integration**:
   - `WorldState` includes `List<Country> Countries`, ensuring `WorldManager.Snapshot()` and `Restore()` preserve the complete country registry, founding moments, and statuses.

## Consequences
- Systems built in subsequent steps (National Yield, Independence Day celebrations, Founding Day bonuses, and international standing) have an authoritative, persistent country identity to build upon.
- Country age and founding data are verifiable in deterministic tests using controlled clock advancement.

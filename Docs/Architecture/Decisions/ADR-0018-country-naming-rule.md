# ADR-0018: Sovereign Country Naming Rule

## Status
Accepted

## Context
Following the completion of the National Road Program ([ADR-0017](ADR-0017-timed-national-road-program.md)) and prior to introducing the Country Profile in Phase 5.1, the core identity of sovereign states in the Republic simulation requires explicit domain rule enforcement.

The game is fundamentally titled *Republic*, centered on constitutional, sovereign states. A national entity created by the player must constitutionally embody this identity by containing the word "Republic" in its official sovereign name. Previous iterations permitted arbitrary string identifiers without lexical validation.

## Decision

1. **Domain Rule Specification**:
   - Every sovereign player country name must contain the whole word **"Republic"**, matched case-insensitively.
   - Valid conforming examples include `"Republic of Walesky"` and `"Kalakuta Republic"`.
   - Case-insensitivity permits `"republic"`, `"Republic"`, `"REPUBLIC"`, and mixed casings.
   - Standalone word boundary matching (`\bRepublic\b`) is strictly enforced:
     - `"Republican"` alone fails because `"Republic"` is not present as a distinct whole word.
     - `"The Republican Republic"` passes because `"Republic"` is present as an independent whole word.
   - The proposed name is trimmed of leading and trailing whitespace. Empty, whitespace-only, or names lacking the keyword are rejected.
   - Formalized via [`ICountryNameRule`](../../../src/Republic.Core/World/Rules/ICountryNameRule.cs) and implemented in [`CountryNameRule`](../../../src/Republic.Core/World/Rules/CountryNameRule.cs).

2. **Strict Rejection Semantics**:
   - Validation failure throws an `ArgumentException` and refuses country creation.
   - A rejected name does not create a country, does not increment the country count in [`CountryService`](../../../src/Republic.Core/World/Services/CountryService.cs), and does not alter any existing sovereign state.
   - Explicit no-silent-rewrite discipline: The system will never silently prepend or append the keyword (e.g. converting `"Walesky"` into `"Republic of Walesky"`). The player must explicitly supply the word.

3. **Country Creation Integration**:
   - Integrated directly into [`CountryService.FoundCountry`](../../../src/Republic.Core/World/Services/CountryService.cs), ensuring that any programmatic founding of a country enforces the domain rule and persists the trimmed name.

4. **Scenario Bootstrap Backward Compatibility**:
   - In [`ScenarioBootstrapper`](../../../src/Republic.Core/Scenarios/Services/ScenarioBootstrapper.cs), existing Arcadia retains its current name if it already contains the word `"Republic"` (e.g. `"Republic of Arcadia"`).
   - If a preset or scenario provides an Arcadia name lacking the word (e.g. `"Arcadia"`), the bootstrapper automatically sets it to `"Republic of Arcadia"` in the scenario bootstrap only.

5. **CLI Prompt Interface**:
   - Provided interactive prompt support via [`Program.PromptCountryName`](../../../src/Republic.Cli/Program.cs) to print the validation rejection message and re-prompt until a valid sovereign name is provided.
   - No new menu numbers or options were added to the CLI executive menu.

6. **Scope Boundary**:
   - This architectural decision implements only the country naming rule before Phase 5.1.
   - Does not implement country profiles, dashboard metrics, national strength scoring, elections, or networking.

## Consequences
- Sovereign nations created in the Republic world must constitutionally carry the word "Republic" in their name.
- Input strings are sanitized and trimmed consistently.
- Invalid creation attempts fail safely with zero side effects on world state or country registry collections.
- Full compatibility with existing scenario presets and future Phase 5 profile models is preserved.

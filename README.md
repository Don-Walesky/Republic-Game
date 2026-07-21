# Republic

Republic is a persistent geopolitical strategy and democratic governance simulation game.

Wave 0 now establishes a **headless deterministic .NET simulation core** that can be tested independently and later adapted into a Unity shell.

## Canonical Planning Model

The repository now uses a **10-wave player-experience roadmap** as the canonical delivery model:

1. Wave 0 — Foundation
2. Wave 1 — Executive Workspace
3. Wave 2 — World Simulation
4. Wave 3 — Campaign
5. Wave 4 — Government
6. Wave 5 — Economy
7. Wave 6 — Legislature
8. Wave 7 — Military
9. Wave 8 — Diplomacy
10. Wave 9 — Multiplayer

Technical layers still matter, but they support the wave plan rather than replacing it.

## Current Repository Structure

- `src/Republic.Core` — deterministic core systems
- `src/Republic.App` — bootstrap host and dependency injection setup
- `tests/Republic.Core.Tests` — Wave 0 unit tests
- `Config/defaults.json` — default runtime configuration
- `unity` — reserved Unity shell boundary
- `Docs` — canonical product and technical documentation

## Wave 0 Foundation Scope

Wave 0 delivers:
- solution and project scaffolding
- configuration loading
- multi-sink logging
- queued event bus
- deterministic time system
- save/load framework
- world manager shell
- bootstrap host and CI entry points

## Local Development

```bash
./build.sh
```

Or run commands directly:

```bash
dotnet restore Republic.sln
dotnet build Republic.sln --configuration Debug
dotnet test Republic.sln --configuration Debug --no-build
```

## Documentation

Start here:
- `Docs/README.md`
- `ARCHITECTURE.md`
- `DEVELOPMENT.md`
- `.github/PRODUCTION_WAVES.md`

## Legacy Planning Notes

Older sprint and backlog documents remain for historical context, but the canonical source of truth is now the combination of:
- `.github/PRODUCTION_WAVES.md`
- `Docs/Roadmap/README.md`
- `Docs/Features/README.md`
- `Docs/Architecture/Decisions/ADR-0001-canonical-delivery-model.md`

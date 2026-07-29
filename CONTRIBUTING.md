# Contributing

## Branch and PR Expectations

- Keep changes scoped to a single objective.
- Update docs when behavior or architecture changes.
- Open pull requests with linked issues or explicit rationale.
- Describe validation performed before review.

## Development Workflow

1. Restore, build, and test before editing when tooling exists.
2. Make the smallest complete change.
3. Re-run build and tests after implementation.
4. Update canonical docs if roadmap or architecture meaning changes.

## Commit Style

Use concise imperative commit messages, for example:
- `Add deterministic time system`
- `Align roadmap with canonical wave model`

## Review Standard

A change is ready for review when:
- build succeeds
- tests pass
- no warnings are introduced
- docs are current
- no secrets are present

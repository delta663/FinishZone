---
name: vrising-mod-project-defaults
description: Analyze reusable V Rising mod project defaults across real repos. Use when Codex needs to compare shared `csproj` structure, root `.editorconfig`, metadata placement, or README and icon conventions before treating them as shared umbrella defaults.
---

# V Rising Mod Project Defaults

Use this skill to extract stable project-level defaults only after comparing
real V Rising mod repos. This is an evidence-first defaults skill, not a
template copier.

## Workflow

1. Gather real repo evidence first.

- Inspect FinishZone's local evidence first:
  - `FinishZone.csproj`
  - `manifest.json`
  - `README.md`
  - `icon.png`
- Then compare at least one real V Rising mod repo with meaningful project
  defaults.
- In this repo, start with
  [`references/evidence-patterns.md`](./references/evidence-patterns.md).
- If there are no external examples available, stop and report
  `needs real repo examples`.

2. Separate stable defaults from local wiring.

- Look for repeated patterns in:
  - `csproj` properties and package families
  - plugin metadata fields
  - `.editorconfig` rules
  - shared README, icon, or metadata placement
- Explicitly reject:
  - absolute machine paths
  - local post-build copy targets
  - giant explicit interop reference lists without an abstraction layer
  - repo-specific branding, package ids, or release wiring

3. Produce a normalized defaults plan.

- Output should separate reusable defaults, consumer-repo inputs, and risky
  local wiring that stays out of the umbrella.

4. Route adjacent work correctly.

- Hand Thunderstore manifests and release boilerplate to
  `vrising-thunderstore-packaging`.
- Hand mixed or ambiguous extraction work back to `vrising-mod-umbrella`.

## Guardrails

- Do not turn one repo's local install paths into a shared default.
- Do not standardize explicit game-reference or post-build deployment paths
  unless a shared indirection layer already exists.
- Do not move README or icon branding into shared defaults unless the assets and
  their usage truly repeat.
- Do not treat one repo's `.editorconfig` as canonical without comparing it
  against at least one other V Rising mod repo.

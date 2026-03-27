---
name: vrising-thunderstore-packaging
description: Analyze and normalize V Rising Thunderstore packaging and release boilerplate across real mod repos. Use when Codex needs to compare Thunderstore manifests or TOML, package READMEs/icons, tcli release workflows, or decide what Thunderstore behavior belongs in the shared umbrella instead of a single repository.
---

# V Rising Thunderstore Packaging

Use this skill to extract Thunderstore-specific patterns only after grounding the
work in real V Rising mod repos. This is an evidence-first extraction skill, not
a local template emitter.

## Workflow

1. Start from real repo evidence.

- Inspect FinishZone's local package surfaces first:
  - `manifest.json`
  - `README.md`
  - `icon.png`
- Then compare at least one real V Rising repo with Thunderstore package
  metadata and ideally one repo with release automation.
- In this repo, begin with
  [`references/evidence-patterns.md`](./references/evidence-patterns.md).
- If no real examples are available, stop and report `needs real repo examples`
  instead of inventing a shared package shape.

2. Normalize only the reusable contract.

- Compare package folder layout, manifest fields, README/icon placement, and
  release workflow behavior.
- Extract only repeated invariants such as field sets, workflow stages, and
  package assembly conventions.
- Keep mod identity, descriptions, dependencies, branding, and release cadence
  repo-local unless repetition is proven across real repos.

3. Produce an adoption plan, not a template dump.

- Output should be a normalized extraction or adoption plan for the umbrella.
- Explicitly separate:
  - what would become shared contract
  - what stays consumer-repo input
  - what still needs more evidence

4. Route non-Thunderstore concerns away.

- Hand shared `csproj`, `.editorconfig`, and metadata default work to
  `vrising-mod-project-defaults`.
- Hand mixed or ambiguous extraction work back to `vrising-mod-umbrella`.

## Guardrails

- Do not fabricate Thunderstore assets in this repo when source examples are
  absent.
- Do not treat one repo's naming, dependency list, or branding as shared by
  default.
- Do not merge Thunderstore release conventions with unrelated publish lanes
  just because both publish artifacts.
- Do not standardize Thunderstore TOML shape without real repo evidence for TOML
  specifically.

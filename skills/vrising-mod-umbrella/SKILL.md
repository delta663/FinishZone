---
name: vrising-mod-umbrella
description: Route shared V Rising mod extraction work and separate proven umbrella modules from repo-specific details. Use when Codex needs to decide what repeated V Rising boilerplate belongs in the shared layer, or to hand Thunderstore work to `vrising-thunderstore-packaging` versus project-default work to `vrising-mod-project-defaults`.
---

# V Rising Mod Umbrella

Use this skill as the local routing layer for shared V Rising work in a
consumer mod repo.

## Current Focus

- FinishZone is a consumer or adoption repo, not the shared harness or release
  repo.
- The local evidence surfaces here are `FinishZone.csproj`, `manifest.json`,
  `README.md`, and `icon.png`.
- The canonical umbrella taxonomy still applies here, even though this repo is
  only one consumer evidence point.

## Routing

- Use `vrising-thunderstore-packaging` for Thunderstore manifests or TOML,
  package READMEs/icons, `tcli` release workflows, and Thunderstore-specific
  release boilerplate.
- Use `vrising-mod-project-defaults` for shared `csproj` defaults,
  `.editorconfig`, and repeated metadata or packaging conventions.
- If the task mixes both, split the analysis instead of forcing one mega-plan.

## Workflow

1. Start with the primitive taxonomy.

- Read
  [`references/primitive-taxonomy.md`](./references/primitive-taxonomy.md)
  first.
- If the task is about orchestration, external planners, or bigger-loop
  routing, apply the control-plane law there before choosing an extraction
  route.
- Separate the task into one of three lanes:
  - shared consumer primitives
  - repo-local infrastructure primitives
  - reference-only future primitives

2. Keep this repo in the consumer lane.

- Ground the work in FinishZone's local repo evidence first.
- Treat local evidence as an input to compare, not as the default answer.
- Do not pull repo-local gameplay logic, branding, or release wiring into the
  shared layer just because they are nearby.

3. Keep cross-repo extraction evidence-first.

- Use
  [`references/extraction-inventory.md`](./references/extraction-inventory.md)
  as the local adoption map.
- Promote a consumer-facing primitive only after it is evidenced in at least
  two independent V Rising mod repos.
- Keep Bloodcraft reference-only until a second consumer repo proves the same
  AI-guidance surface.

## Guardrails

- Do not collapse every V Rising repo into one mega-template.
- Do not use FinishZone as a template source for repo-local gameplay behavior.
- Do not invent Thunderstore or project-default boilerplate when the evidence
  is still thin.
- Do not let planners act as direct repo operators here; they may propose typed
  work orders and consume receipts, but execution belongs to deterministic
  runners or thin repo-local adoption steps.
- Do not use umbrella extraction as cover for runtime code changes in this repo.

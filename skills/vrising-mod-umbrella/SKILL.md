---
name: vrising-mod-umbrella
description: Route shared V Rising mod extraction work and separate proven umbrella modules from repo-specific details. Use when Codex needs to decide what repeated V Rising boilerplate belongs in the shared layer, or to hand Thunderstore work to `vrising-thunderstore-packaging` versus project-default work to `vrising-mod-project-defaults`.
---

# V Rising Mod Umbrella

Use this skill as the index and routing layer for shared V Rising work in a
consumer mod repo.

## Current Focus

- FinishZone is an adoption repo, not the shared harness or release repo.
- The local evidence surfaces here are `FinishZone.csproj`, `manifest.json`,
  `README.md`, and `icon.png`.
- The next umbrella extraction wave is unblocked: compare these local surfaces
  against real mod repos and lift only the repeated structure.
- The first external pilot repos remain `WhiteFang5/VMods` and
  `Odjit/KindredCommands`.

## Routing

- Use `vrising-thunderstore-packaging` for Thunderstore manifests or TOML,
  package READMEs/icons, `tcli` release workflows, and Thunderstore-specific
  release boilerplate.
- Use `vrising-mod-project-defaults` for shared `csproj` defaults,
  `.editorconfig`, and repeated metadata or packaging conventions.
- If the task mixes both, split the analysis instead of forcing one mega-plan.

## Workflow

1. Start with what is already visible here.

- Ground the work in FinishZone's local repo evidence first.
- Use
  [`references/extraction-inventory.md`](./references/extraction-inventory.md)
  as the umbrella inventory and adoption map.
- Treat local evidence as inputs to compare, not as the default answer.

2. Choose the correct extraction lane.

- Route Thunderstore-facing work to `vrising-thunderstore-packaging`.
- Route project-default work to `vrising-mod-project-defaults`.
- If a pattern is still hypothetical, keep it in the extraction inventory
  rather than implementing it as shared boilerplate.

3. Keep cross-repo extraction evidence-first.

- Promote a pattern only after it exists in real mod repos and has a stable,
  explainable boundary.
- Explicitly separate what would become shared contract from what stays
  consumer-repo input.
- Preserve next-wave evidence sources such as Bloodcraft as references, not as
  extra scope in this repo.

## Guardrails

- Do not collapse every V Rising repo into one mega-template.
- Do not move repo-local gameplay logic, branding, or release wiring into the
  umbrella just because they are nearby.
- Do not invent Thunderstore or project-default boilerplate when the evidence
  is still thin.
- Do not use umbrella extraction as cover for runtime code changes in this repo.

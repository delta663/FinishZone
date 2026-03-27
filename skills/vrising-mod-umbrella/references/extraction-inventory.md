# Extraction Inventory

## Implemented Here

- Umbrella routing scaffolding for shared V Rising extraction work
- Evidence-first references for Thunderstore packaging and project defaults
- Local FinishZone evidence surfaces:
  - `FinishZone.csproj`
  - `manifest.json`
  - `README.md`
  - `icon.png`

## Active Extraction Routes

- `vrising-thunderstore-packaging`: compare Thunderstore manifests, package
  READMEs/icons, and release workflows across real mod repos
- `vrising-mod-project-defaults`: compare shared `csproj`, `.editorconfig`, and
  metadata defaults across real mod repos

## Current Pilot Repos

- `WhiteFang5/VMods`
- `Odjit/KindredCommands`

## Next-Wave Evidence Repo

- `mfoltz/Bloodcraft`
  - `.codex/`
  - `AGENTS.md`
  - `.editorconfig`
  - `thunderstore.toml`
  - `.github/CONTRIBUTING.md`
  - `.github/workflows/*`
- Use Bloodcraft as future evidence for AI-assisted repo guidance surfaces,
  Thunderstore and release boilerplate, and project-default extraction.
- Do not pull Bloodcraft scaffolding into FinishZone during umbrella planning.

## Cross-Repo Extraction Candidates

- Thunderstore publish workflows
- Thunderstore manifest or TOML defaults
- Shared mod `csproj` defaults
- Root `.editorconfig`
- Shared package, README, or icon metadata where it genuinely repeats
- AI-driven repo guidance surfaces once they repeat across more than one mod
  repo

## Adoption Rule

Promote an item into the umbrella only after it exists in a real mod repo and
shows stable, repeated structure. Until then, keep it listed here as an
extraction candidate instead of creating speculative local boilerplate.

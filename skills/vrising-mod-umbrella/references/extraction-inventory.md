# Extraction Inventory

## Local Consumer Repo

- FinishZone is a consumer evidence repo, not the source-of-truth umbrella repo.
- Local evidence surfaces here include:
  - `FinishZone.csproj`
  - `manifest.json`
  - `README.md`
  - `icon.png`

## Shared Consumer Primitive Families

- Thunderstore packaging, manifests or TOML, package READMEs and icons, and
  release boilerplate
- Project defaults such as shared `csproj` structure, `.editorconfig`, and
  repeated metadata surfaces
- Package metadata surfaces that repeat across real mod repos

## Active Extraction Routes

- `vrising-thunderstore-packaging`: compare Thunderstore packaging across real
  mod repos
- `vrising-mod-project-defaults`: compare reusable project defaults across real
  mod repos

## Consumer Evidence Repos

- `FinishZone`
- `WhiteFang5/VMods`
- `Odjit/KindredCommands`

## Reference-Only Future Primitives

- AI-guidance surfaces such as:
  - `.codex/`
  - `AGENTS.md`
  - GitHub instruction files
  - workflow guidance files

## Reference-Only Next-Wave Repo

- `mfoltz/Bloodcraft`
  - `.codex/`
  - `AGENTS.md`
  - `.editorconfig`
  - `thunderstore.toml`
  - `.github/CONTRIBUTING.md`
  - `.github/workflows/*`
- Treat Bloodcraft as evidence for future AI-guidance, Thunderstore, and
  project-default work only. Do not use it as a template source until another
  consumer repo proves the same surface.

## Adoption Rule

Promote an item into the shared umbrella only after it is evidenced in at least
two independent V Rising mod repos and shows stable, repeated structure. Keep
repo-local gameplay logic, branding, and release wiring out of the shared layer
unless that repetition is proven.

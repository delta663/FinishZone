# Thunderstore Evidence Patterns

Use these examples to separate what FinishZone proves from what other consumer
repos prove about Thunderstore packaging.

## What The Current Repo Proves

- FinishZone provides a local root-layout package metadata surface through:
  - `manifest.json`
  - `README.md`
  - `icon.png`
- This repo proves a simple root-layout packaging shape, not a universal
  Thunderstore convention.

## What Other Repos Prove

### WhiteFang5/VMods

- Repo: `https://github.com/WhiteFang5/VMods`
- Evidence:
  - `Thunderstore/<ModName>/manifest.json`
  - `Thunderstore/<ModName>/README.md`
  - shared `Thunderstore/icon.png`
- What it proves:
  - one package directory per mod
  - manifest + package README pairing
  - icon sharing is possible across a mod family
- What not to overfit:
  - exact manifest fields beyond the standard Thunderstore contract
  - package naming, descriptions, and dependency values

### Odjit/KindredCommands

- Repo: `https://github.com/Odjit/KindredCommands`
- Evidence:
  - `.github/workflows/release.yml`
  - root `README.md`
  - root `logo.png`
- What it proves:
  - release-tag-driven Thunderstore publication via `tcli`
  - package docs and icons may live at repo root instead of a dedicated
    Thunderstore directory
- What not to overfit:
  - exact release trigger strategy
  - tag-to-version conventions
  - secret names or workflow step ordering

## What Still Lacks Enough Evidence

- Shared candidates:
  - package folder shape expectations
  - manifest or release metadata checklist
  - release workflow stage outline
  - icon and README presence rules
- Still unproven:
  - a shared `thunderstore.toml` convention
  - AI-guided Thunderstore authoring surfaces
- Keep repo-local:
  - mod name, author namespace, summary text, dependencies, branding, and
    release cadence

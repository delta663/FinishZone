# Thunderstore Evidence Patterns

Use these examples as the first grounding pass for Thunderstore packaging work.

## FinishZone

- Local evidence:
  - root `manifest.json`
  - root `README.md`
  - root `icon.png`
- What it proves:
  - a simple root-layout package metadata shape exists in a real V Rising mod
    repo
  - package docs and icon placement can live at repo root
- What not to overfit:
  - exact manifest fields beyond the standard Thunderstore contract
  - dependency versions, descriptions, and naming

## WhiteFang5/VMods

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

## Odjit/KindredCommands

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

## Extraction Notes

- Shared candidates:
  - package folder shape expectations
  - manifest or release metadata checklist
  - release workflow stage outline
  - icon and README presence rules
- Keep repo-local:
  - mod name, author namespace, summary text, dependencies, branding, and
    release cadence
- TOML note:
  - Do not standardize `thunderstore.toml` from this reference set alone; these
    examples prove manifest and workflow patterns, not a TOML convention.

# Project Defaults Evidence Patterns

Use these examples to separate what FinishZone proves from what other consumer
repos prove about shared project defaults.

## What The Current Repo Proves

- FinishZone provides local evidence for:
  - `FinishZone.csproj`
  - `manifest.json`
  - `README.md`
  - `icon.png`
- This repo proves that a real V Rising mod can keep its project file and
  package metadata at repo root, but it does not prove that its exact
  interop/core wiring should become shared.

## What Other Repos Prove

### Odjit/KindredCommands

- Repo: `https://github.com/Odjit/KindredCommands`
- Evidence:
  - root `.editorconfig`
  - `KindredCommands.csproj`
  - root `README.md`
  - root `logo.png`
- What it proves:
  - a repo can keep strong editor rules at root
  - plugin metadata and package references often live directly in the mod
    project file
  - README and logo may be part of the repo's project-default surface
- What not to overfit:
  - exact package versions
  - giant local reference lists
  - release workflow coupling

### WhiteFang5/VMods

- Repo: `https://github.com/WhiteFang5/VMods`
- Evidence:
  - `Shared/Shared.csproj`
- What it proves:
  - shared project-level library patterns exist in real V Rising mod repos
  - common package families and post-build deployment hooks repeat in practice
- What not to overfit:
  - absolute local paths like `M:\\Games\\...`
  - direct copy targets into local plugin folders
  - hardcoded machine-specific install assumptions

## What Still Lacks Enough Evidence

- FinishZone does not currently provide a local `.editorconfig`, so editor-rule
  defaults still need cross-repo proof before standardization.
- Shared candidates:
  - target framework conventions
  - common package families
  - plugin metadata property names
  - `.editorconfig` rule blocks that clearly repeat
  - README and icon placement conventions when proven across repos
- Still unproven:
  - AI-assisted project-guidance surfaces as shared defaults
- Keep repo-local:
  - absolute drive paths
  - explicit interop/core reference inventories
  - post-build copy destinations
  - branding, package identity, and release wiring

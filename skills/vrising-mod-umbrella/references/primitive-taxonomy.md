# Primitive Taxonomy

Use this taxonomy before promoting any V Rising pattern into the umbrella.

## Shared Consumer Primitives

These are mod-facing patterns that may become shared once they repeat across
consumer repos:

- Thunderstore packaging and release boilerplate
- Shared project defaults such as `csproj`, `.editorconfig`, and metadata
  placement
- Package metadata surfaces that repeat across real mod repos

## Repo-Local Infrastructure Primitives

These stay local to each repo even when they support the broader ecosystem:

- repo-specific release wiring
- repo-specific gameplay logic
- local bootstrap or testing harnesses
- repo-local diagnostics and operational scripts

## Reference-Only Future Primitives

These are worth tracking but are not shared primitives yet:

- `.codex/`
- `AGENTS.md`
- GitHub instruction files
- workflow guidance surfaces

Treat these as reference-only until at least two independent consumer repos
prove the same shape.

## Promotion Rule

- A consumer-facing primitive becomes shared only after it is evidenced in at
  least two independent V Rising mod repos.
- When evidence is incomplete, keep the pattern in the extraction inventory
  instead of turning it into a template or shared module.

## Control-Plane Law

- External planners may classify work and propose typed work orders, but they
  do not replace deterministic executors.
- Promotion and adoption decisions must be backed by receipts, proofs, and
  repo evidence rather than chat state or implied memory.

# Endpoint parity

Measured 2026-10-18 by static scan of both codebases.

| Side | Measure | Count |
|---|---|---|
| Legacy API | `[Http*]` action attributes | 464 |
| Legacy API | named action routes (`[Route("...")]`, excludes class templates) | 413 |
| New API | `[Http*("...")]` actions | 118 |
| Legacy actions with no same-named new action | name match only | ~339 |

## How this was measured, and its limits

The counts come from regular-expression scans of controller source, not from a running OpenAPI diff.

- Legacy controllers declare the action name in a method-level `[Route("Action")]` and the verb in a bare
  `[Http*]` attribute, while the new controllers declare the name inline as `[Http*("Action")]`. A
  verb-and-name match therefore under-reports parity.
- The ~339 "unmatched" figure is an upper bound: it includes class-level route templates such as
  `api/[controller]` and `errors/`, and it ignores renamed-but-equivalent endpoints (for example the
  legacy reporting actions that Reporting deliberately replaces with read models).

Treat this file as a sizing estimate. A per-controller comparison against both running applications'
OpenAPI documents is required before any endpoint is marked ported or retired.

## What the numbers mean

The architectural conversion is complete: nine bounded contexts, per-module schemas, outbox and
projections, module authorization policies, and a green build, test and format gate.

Functional parity is not complete. Roughly three quarters of the legacy action surface has no
counterpart in the new API. Closing that gap is the remaining work.

## Port waves

One PR per wave, module-bounded, each with unit, integration and authorization tests:

1. Identity remainder
2. Catalog remainder
3. Procurement remainder
4. Quality remainder
5. Warehouse remainder
6. Orders remainder
7. Reporting reports — each legacy report action is either ported against a read model or explicitly retired

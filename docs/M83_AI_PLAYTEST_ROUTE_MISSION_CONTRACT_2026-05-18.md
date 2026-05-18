# M83 AI Playtest Route Mission Contract

Date: 2026-05-18

## Lane

QA / playtest.

## Player Problem

The project needs a repeatable way to prove that the tactical slice is playable like a real route, not merely visible in fixed cameras or isolated screenshots.

## Scope

- Add a deterministic AI playtest route gate.
- Wire the gate into the tactical acceptance pipeline.
- Use existing route, gameplay, building, weapon, and player-POV reports as the evidence source.
- Emit a route-level JSON report with pass/fail and blockers.

## Out Of Scope

- New gameplay features.
- New assets or asset promotion.
- Visual production pass.
- Package changes.
- Large scene YAML edits.

## Required Unity Menu Commands

- `AI Tools/Run AI Playtest Route Gate`
- `AI Tools/Run Tactical Acceptance Pipeline`

## Evidence Required

- `docs/AI_PLAYTEST_ROUTE_GATE.json`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- Fresh Console evidence with no compile errors.

## Definition Of Done

- The AI playtest gate passes.
- The full acceptance pipeline includes `ai_playtest_route_gate_passed: true`.
- The full acceptance pipeline still reports `all_required_current_gates_passed: true`.
- The gate reports blockers when route, gameplay, building, weapon, player-POV, loot, combat, reload, death/restart, stuck, or screenshot evidence is missing.

## Rollback Plan

Revert:

- `Assets/Editor/AiPlaytestRouteGate.cs`
- `Assets/Editor/AiPlaytestRouteGate.cs.meta`
- the small `TacticalAcceptancePipeline.cs` integration diff;
- generated M83 docs/reports.

# M83 AI Playtest Route Gate

Date: 2026-05-18

## Mission

Add a deterministic AI playtest route gate to the tactical acceptance pipeline. The gate is not a showcase camera and does not count contact sheets as gameplay proof. It aggregates the existing player-route, gameplay-proof, building-integrity, weapon-feel, and player-POV reports into one route-level pass/fail verdict.

## Route Contract

The route covers:

```text
lobby/start
  -> spawn
  -> move
  -> enter building / traverse map zones
  -> reach upper-floor or ladder-supported traversal proof
  -> pickup
  -> fire / hit or enemy attack
  -> reload or recovery state
  -> death / restart
  -> screenshot evidence from player-camera paths
```

## Implementation

Added `Assets/Editor/AiPlaytestRouteGate.cs`.

The gate reads:

- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`
- `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json`
- `docs/BUILDING_INTEGRITY_GATE.json`
- `docs/WEAPON_FEEL_GATE.json`
- `docs/TACTICAL_PLAYER_POV_GATE.json`

It writes:

- `docs/AI_PLAYTEST_ROUTE_GATE.json`

The gate is also wired into `Assets/Editor/TacticalAcceptancePipeline.cs`, so `AI Tools/Run Tactical Acceptance Pipeline` now reports `ai_playtest_route_gate_passed`.

## Verification

The full Unity tactical acceptance pipeline was run through the active community Unity MCP session on `/Users/yuanshaochen/My project`.

Fresh pipeline evidence:

- `status: complete`
- `console_errors: 0`
- `console_warnings: 0`
- `playable_route_gate_passed: true`
- `gameplay_proof_gate_passed: true`
- `building_integrity_gate_passed: true`
- `weapon_feel_gate_passed: true`
- `player_pov_gate_passed: true`
- `ai_playtest_route_gate_passed: true`
- `all_required_current_gates_passed: true`

AI playtest evidence:

- `passed: true`
- `start_passed: true`
- `movement_passed: true`
- `traversal_passed: true`
- `loot_passed: true`
- `combat_passed: true`
- `reload_or_recovery_passed: true`
- `death_restart_passed: true`
- `no_stuck_detected: true`
- `screenshot_evidence: true`
- `blockers: none`

## Recovery Note

During this mission, Unity initially appeared stuck at `EditorApplication.isCompiling=true` without Console compile errors. Root-cause investigation showed the editor was blocked by a recovered-scene backup dialog and then by a duplicate Unity instance. The recovery was:

1. Inspect Console and Editor.log before patching code.
2. Preserve scene backups via the Unity dialog.
3. Remove the stale duplicate Unity process.
4. Reopen the active project.
5. Confirm `compiling=false` and `updating=false` through community MCP.
6. Run one full deterministic acceptance pipeline.

This is a Unity session recovery issue, not evidence of a C# compile error in the M83 gate.

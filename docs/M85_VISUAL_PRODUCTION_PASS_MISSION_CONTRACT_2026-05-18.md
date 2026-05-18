# M85 Visual Production Pass Mission Contract

Date: 2026-05-18

## Lane

Visual direction / evidence audit.

## Player Problem

The game route is now playable and route-gated, but the player-camera visuals still read as a tactical prototype instead of a dense, believable tactical game slice. Prior screenshots and asset proofs do not by themselves prove in-game visual production quality.

## Goal

Add a scoped player-route visual production pass that improves scene density, wet-surface detail, route-readable lighting, and tactical clutter while preserving all existing gameplay gates.

## Scope

- Player-camera visual details only.
- Wet-route puddles, mud scuffs, faded road paint, cable bundles, wall grime, small casing clutter, and warning markers.
- M85-specific JSON and screenshot evidence.
- Acceptance pipeline integration.

## Out Of Scope

- Claiming final PUBG-like/commercial visual completion.
- Promoting failed Realified non-weapon assets.
- Replacing the intermediate tactical humanoid.
- Broad scene rebuilds.
- New package installation.
- Manual `.unity` or `.prefab` YAML edits.

## Required Evidence

- Before-reference player-route screenshots.
- After screenshots from the actual gameplay player camera.
- Counts for M85 visual detail objects.
- Proof that route, building, weapon feel, AI playtest, and M84 still pass.
- Proof that `full_visual_asset_gate_passed=false` remains truthful while final visual blockers remain.

## Definition Of Done

- `docs/M85_VISUAL_PRODUCTION_PASS.json` has `passed=true`.
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json` has `m85_visual_production_passed=true`.
- Unity Console has no new compile errors or warnings after the pipeline run.
- `full_visual_asset_gate_passed` is not manually overridden.

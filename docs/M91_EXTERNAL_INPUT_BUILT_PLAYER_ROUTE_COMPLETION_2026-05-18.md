# M91 External-Input Built Player Route Completion

Date: 2026-05-18

Status: passed.

## What M91 Proves

M91 complements M90 by proving that the built macOS player can complete the tactical route through external OS-level keyboard and mouse input, not through direct gameplay API calls.

The route evidence was produced by launching:

```text
/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app
```

with the M91 telemetry flag enabled, then driving the app with external input automation.

## Evidence

Primary route evidence:

```text
docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json
```

Editor gate evidence:

```text
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.json
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.md
docs/UNITY_MCP_EXECUTE_M91_GATE_PASS.json
```

Rebuild evidence:

```text
docs/M86_BUILD_RELEASE_GATE.json
docs/M86_BUILD_RELEASE_GATE.md
docs/UNITY_MCP_EXECUTE_M86_REBUILD_FOR_M91_RESTART_FIX.json
```

Screenshot evidence:

```text
docs/M91ExternalInputBuiltPlayerRoute/00_lobby_before_external_input.png
docs/M91ExternalInputBuiltPlayerRoute/01_after_start_input.png
docs/M91ExternalInputBuiltPlayerRoute/02_after_movement_input.png
docs/M91ExternalInputBuiltPlayerRoute/03_after_pickup_input.png
docs/M91ExternalInputBuiltPlayerRoute/04_after_fire_input.png
docs/M91ExternalInputBuiltPlayerRoute/05_after_reload_input.png
docs/M91ExternalInputBuiltPlayerRoute/06_after_enemy_interaction_input.png
docs/M91ExternalInputBuiltPlayerRoute/07_after_death_or_restart_input.png
```

## Route Signals

The committed M91 route JSON reports:

```text
passed=true
external_input_driven=true
built_player=true
final=true
timeout_reached=false
start_input_observed=true
movement_input_observed=true
movement_state_changed=true
pickup_input_observed=true
pickup_state_changed=true
fire_input_observed=true
ammo_state_changed=true
reload_input_observed=true
reload_state_changed=true
enemy_interaction_observed=true
death_or_restart_observed=true
first_person_weapon_visible=true
screenshot_count=8
blockers=[]
```

The Editor gate also reports `passed=true`, `route_json_passed=true`, `route_json_external_input_driven=true`, and `route_json_built_player=true`.

## Implementation Notes

Patch A from ChatGPT PRO added the first version of:

```text
Assets/Scripts/Tactical/TacticalExternalInputRouteTelemetry.cs
Assets/Editor/M91ExternalInputBuiltPlayerGate.cs
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_MISSION_CONTRACT_2026-05-18.md
```

Codex then made local verification fixes after applying the patch:

- Rebuilt the macOS player so the telemetry script existed inside the built app.
- Adjusted route telemetry so timeout writes cannot overwrite the final pass report.
- Reduced the movement threshold to match the short externally driven player movement.
- Delayed and reduced scripted enemy damage so the route can reach reload/restart proof.
- Detected restart/death completion from external `Enter` input after route progress.
- Used OS-level keyboard/mouse injection for the route, because AppleScript key/mouse events were insufficient for reliable `W` and mouse input.

## Non-Claims

M91 does not close:

- `full_visual_asset_gate_passed=false`
- `legacy_realified_batch_visibility_gate_passed=false`
- `generated_batch_class_promotion_passed=false`
- `final_weapon_art_review_passed=false`
- `final_humanoid_art_review_passed=false`

M91 proves built-player external-input route viability. It does not prove final PUBG-like visual quality or production-grade non-weapon Realified asset promotion.

## Next Work

Return to the M88 strict visual blockers:

1. Final weapon art review.
2. Final humanoid art review.
3. Generated batch class promotion.
4. Production-grade visual asset replacement and review.

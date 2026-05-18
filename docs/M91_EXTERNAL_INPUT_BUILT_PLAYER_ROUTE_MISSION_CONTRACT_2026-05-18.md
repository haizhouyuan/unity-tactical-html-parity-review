# M91 External Input Built Player Route Mission Contract

Date: 2026-05-18

Mission ID: M91

Title: External-input built-player route telemetry and gate

Primary lanes:

- Release / LiveOps
- QA / Playtest
- Evidence Audit

## Purpose

M90 proved that the built macOS app can execute the tactical route from inside the runtime recorder. M90 explicitly records `external_input_driven=false`.

M91 adds telemetry and an Editor gate for a separate proof: the built player can complete the route when driven by external keyboard/mouse input or manual human input.

M91 must not fake this by calling gameplay route methods.

## Current Ground Truth

- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json` reports `passed=true`.
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json` reports `external_input_driven=false`.
- `full_visual_asset_gate_passed=false` remains true and must remain true unless a separate strict visual mission proves otherwise.
- M88 strict blockers remain open:
  - final weapon art review;
  - final humanoid art review;
  - generated batch class promotion;
  - clean/external built-player gameplay capture.

## Scope

Patch A adds:

- `Assets/Scripts/Tactical/TacticalExternalInputRouteTelemetry.cs`
- `Assets/Editor/M91ExternalInputBuiltPlayerGate.cs`
- `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_MISSION_CONTRACT_2026-05-18.md`
- README/TODO links and status notes.

## Out Of Scope

- Do not modify `Assets/Scenes/TacticalPrototype.unity`.
- Do not modify prefab YAML.
- Do not hand-edit existing gate JSON to make it pass.
- Do not commit `Builds/`.
- Do not add packages.
- Do not change `full_visual_asset_gate_passed`.
- Do not remove or rewrite M90.
- Do not close M88 strict blockers.

## Runtime Telemetry Contract

`TacticalExternalInputRouteTelemetry` installs only when the built player command line contains:

```text
--m91-external-input-route
```

Optional output argument:

```text
--m91-output=/path/to/output
```

If the output target is a directory, the telemetry writes:

```text
M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json
00_lobby_before_external_input.png
01_after_start_input.png
02_after_movement_input.png
03_after_pickup_input.png
04_after_fire_input.png
05_after_reload_input.png
06_after_enemy_interaction_input.png
07_after_death_or_restart_input.png
```

If the output target ends with `.json`, the telemetry writes the JSON there and screenshots beside it.

The telemetry may do deterministic setup:

- move an existing `TacticalLoot` near the player;
- configure that loot as a real weapon pickup;
- move an existing `TacticalEnemy` into a visible route;
- sync transforms.

The telemetry must not call these route-driving gameplay APIs to pass the gate:

- `StartRound()`
- `FireCurrentWeapon()`
- `Reload()`
- `TryPickupNearest()`
- `DamagePlayer()`

The telemetry mainly observes:

- Unity Input System keyboard/mouse input;
- lobby state;
- player position;
- weapon state;
- magazine/reload state;
- loot count / weapon pickup state;
- enemy health/enemy count/player damage;
- first-person weapon visibility.

## Required JSON Fields

`M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json` must contain at least:

```json
{
  "schema": "m91_external_input_built_player_route_v1",
  "passed": false,
  "external_input_driven": true,
  "built_player": true,
  "start_input_observed": false,
  "movement_input_observed": false,
  "movement_state_changed": false,
  "pickup_input_observed": false,
  "pickup_state_changed": false,
  "fire_input_observed": false,
  "ammo_state_changed": false,
  "reload_input_observed": false,
  "reload_state_changed": false,
  "enemy_interaction_observed": false,
  "death_or_restart_observed": false,
  "first_person_weapon_visible": false,
  "screenshot_count": 0,
  "screenshots": [],
  "events": [],
  "blockers": []
}
```

## Editor Gate Contract

`AI Tools/Run M91 External Input Built Player Gate` must not drive the built player.

It may:

- check whether the runtime telemetry script exists;
- check whether the M86 built app exists;
- check whether M91 output JSON exists;
- parse whether that JSON has:
  - `passed=true`;
  - `external_input_driven=true`;
  - `built_player=true`;
- write:
  - `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.json`;
  - `docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_GATE.md`.

It must not set `passed=true` unless the output JSON already proves the external-input route.

## Launch Command

Expected local launch command:

```bash
"/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app/Contents/MacOS/My project" --m91-external-input-route --m91-output="/Users/yuanshaochen/My project/docs"
```

## Manual Or External Input Route

After launching:

1. Press `Enter`, `Space`, or click to start.
2. Move with `WASD` or arrow keys.
3. Press `F` near the arranged loot.
4. Left-click to fire.
5. Press `R` to reload.
6. Interact with the arranged NPC: damage it or let it damage/down the player.
7. Continue until `07_after_death_or_restart_input.png` is captured.

## Definition Of Done

M91 is complete only when:

- the built app is launched with the M91 flag;
- the route is driven by real keyboard/mouse input or an external input automation tool;
- `M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json` exists;
- the route JSON says:
  - `passed=true`;
  - `external_input_driven=true`;
  - `built_player=true`;
- all required screenshots exist;
- the Editor gate JSON says `passed=true`.

## Non-Claims

M91 does not prove:

- final visual production quality;
- M88 strict visual blockers;
- full visual asset gate pass;
- generated batch class promotion;
- final weapon art review;
- final humanoid art review.

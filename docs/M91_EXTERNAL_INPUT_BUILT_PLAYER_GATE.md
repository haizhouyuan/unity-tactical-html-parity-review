# M91 External Input Built Player Gate

Date: 2026-05-18 14:36:55 UTC

This gate verifies whether the built macOS app has been completed through an external keyboard/mouse or manual input route. It does not call gameplay APIs and does not drive the route itself.

## Result

- `passed`: `true`
- `script_exists`: `true`
- `built_app_exists`: `true`
- `built_app_path`: `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app`
- `route_json_exists`: `true`
- `route_json_path`: `docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json`
- `route_json_passed`: `true`
- `route_json_external_input_driven`: `true`
- `route_json_built_player`: `true`
- `route_json_screenshot_count`: `8`

## Launch Command

```bash
"/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app/Contents/MacOS/My project" --m91-external-input-route --m91-output="/Users/yuanshaochen/My project/docs"
```

## Required External Input Route

After launching with the command above, drive the built app manually or with an external input automation tool:

1. Press `Enter`, `Space`, or click to start from the lobby.
2. Move with `WASD` or arrow keys until the player position changes.
3. Press `F` near the arranged loot.
4. Left-click to fire.
5. Press `R` to reload.
6. Interact with the arranged NPC: either damage it or let it damage/down the player.
7. Keep playing until `07_after_death_or_restart_input.png` is captured.

The telemetry should write:

```text
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json
docs/00_lobby_before_external_input.png
docs/01_after_start_input.png
docs/02_after_movement_input.png
docs/03_after_pickup_input.png
docs/04_after_fire_input.png
docs/05_after_reload_input.png
docs/06_after_enemy_interaction_input.png
docs/07_after_death_or_restart_input.png
```

## Blockers

- none

## Important Non-Claims

- This gate does not set `full_visual_asset_gate_passed=true`.
- This gate does not replace M88 strict visual blockers.
- This gate does not prove external input unless the route JSON says `external_input_driven=true` and `passed=true`.

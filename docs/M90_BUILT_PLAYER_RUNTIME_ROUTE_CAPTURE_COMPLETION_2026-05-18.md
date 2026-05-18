# M90 Built Player Runtime Route Capture Completion

Date: 2026-05-18

## Result

Status: passed, with an explicit limitation.

M90 launches the macOS built player from `/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app` and runs a built-player-only route recorder using the `--m90-built-route-capture` command-line flag. The route produces screenshots, a JSON report, and a player log from the built app rather than the Unity Editor.

This is not an external keyboard/mouse bot route. The report records `external_input_driven=false`; the recorder drives public gameplay APIs inside the built player and captures the resulting route evidence.

## Evidence

- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json`
- `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.log`
- `Assets/Screenshots/M90BuiltPlayerRoute/00_lobby_before_start.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/01_after_start_first_person.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/02_after_runtime_move.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/03_building_lane.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/04_container_lane.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/05_after_pickup_attempt.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/06_after_fire.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/07_after_reload.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/08_after_damage.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/09_after_death.png`
- `Assets/Screenshots/M90BuiltPlayerRoute/10_after_restart.png`

## Verified Route

The built player report records:

- `passed=true`
- `built_player_runtime_route=true`
- `external_input_driven=false`
- `lobby_seen=true`
- `start_passed=true`
- `movement_passed=true`
- `traversal_passed=true`
- `pickup_passed=true`
- `fire_passed=true`
- `reload_passed=true`
- `enemy_interaction_passed=true`
- `death_passed=true`
- `restart_passed=true`
- `first_person_weapon_visible=true`
- `screenshot_count=11`

The pickup event changed gameplay weapon state from pistol to rifle:

```text
pickup_nearest_loot 38->38 distance=0.05 weapon=pistol->rifle
```

## Implementation Note

`Assets/Scripts/Tactical/TacticalBuiltPlayerRouteRecorder.cs` installs only when the built player is launched with the M90 flag. It does not run during normal gameplay.

The recorder intentionally records that it is runtime-driven, not external-input-driven. This prevents the route from being misrepresented as a human-like manual playthrough.

## Remaining Limitations

- This does not solve the strict visual-art blockers from M88.
- This does not make the quarantined generated Realified batch production-ready.
- This does not prove external keyboard/mouse automation.
- This does not claim PUBG-like final visual quality.

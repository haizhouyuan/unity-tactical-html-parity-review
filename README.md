# Unity Tactical HTML Parity Review

Public review/export repository for a Unity recreation of an existing HTML/Three.js tactical game prototype.

The goal is not to present this as finished. The goal is to expose the Unity project, the original HTML baseline, the current evidence gates, and the known gaps so another reviewer or coding agent can propose concrete fixes.

## What This Repo Contains

- `Assets/` - Unity source, scenes, prefabs, scripts, screenshots, and imported tactical asset candidates.
- `Packages/` - Unity package manifest and lock file.
- `ProjectSettings/` - Unity project settings.
- `docs/` - current acceptance reports, parity gates, asset audits, and work logs.
- `reference/html_baseline_final_packet/` - the original HTML/Three.js final packet used as the parity baseline.

## Development Record

For the full development-process handoff, including methods used, staged deliverables, pivots, mistakes, current gates, and next recommended patches, read:

```text
DEVELOPMENT_RECORD.md
```

For the critical triage of the external review recommendations, including which suggestions were adopted, adapted, deferred, or rejected, read:

```text
docs/PRO_REVIEW_CRITICAL_ASSESSMENT_2026-05-18.md
```

For the broader AI game-development operating-system strategy triage, including what is already implemented locally and what should be adopted, adapted, or deferred, read:

```text
docs/AI_GAME_DEV_OS_STRATEGY_TRIAGE_2026-05-18.md
```

For the AI-native game development operating rules that future agents should follow, read:

```text
docs/AI_GAME_STUDIO_OS.md
docs/AI_CLOSED_LOOP_PIPELINE.md
docs/PLAYER_INTENT_LOG.md
docs/MISSION_TEMPLATE.md
docs/ASSET_PROMOTION_STANDARD.md
```

For the long-running execution checklist from M0.5 through build/release and asset-factory work, read:

```text
docs/LONG_RUNNING_AI_GAME_STUDIO_TODO_2026-05-18.md
```

For the current active-Unity verification and first post-M0 gameplay gate, read:

```text
docs/M0_5R_ACTIVE_UNITY_SESSION_VERIFICATION_2026-05-18.md
docs/M81_BUILDING_INTEGRITY_COMPLETION_2026-05-18.md
docs/M82_WEAPON_FEEL_COMPLETION_2026-05-18.md
docs/M83_AI_PLAYTEST_ROUTE_COMPLETION_2026-05-18.md
docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE_COMPLETION_2026-05-18.md
docs/M85_VISUAL_PRODUCTION_PASS_COMPLETION_2026-05-18.md
docs/M86_BUILD_RELEASE_COMPLETION_2026-05-18.md
docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_COMPLETION_2026-05-18.md
docs/M88_STRICT_FULL_VISUAL_ASSET_GATE_COMPLETION_2026-05-18.md
docs/M89_BUILT_PLAYER_CLEAN_CAPTURE_COMPLETION_2026-05-18.md
docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE_COMPLETION_2026-05-18.md
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_MISSION_CONTRACT_2026-05-18.md
docs/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE_COMPLETION_2026-05-18.md
docs/M92_WEAPON_PRODUCTION_PASS_MISSION_CONTRACT_2026-05-18.md
docs/M92_WEAPON_PRODUCTION_PASS_COMPLETION_TEMPLATE_2026-05-18.md
docs/M0_6_COMMUNITY_UNITY_MCP_SKILL_VERIFICATION_2026-05-18.md
docs/COMMUNITY_UNITY_MCP_CODEX_SKILL_PLAN.md
```

Project-local Codex skills live under:

```text
.codex/skills/
```

## Main Unity Entry Point

Open this repository as a Unity project and load:

```text
Assets/Scenes/TacticalPrototype.unity
```

The intended manual route is:

1. Start the tactical prototype from the lobby.
2. Verify first-person weapon visibility.
3. Fire with left click.
4. Reload with `R`.
5. Pick up loot with `F`.
6. Engage an NPC/enemy.
7. Compare behavior and visual completeness with `reference/html_baseline_final_packet/index.html`.

## Current Evidence Snapshot

The latest local acceptance report before export is:

```text
docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json
```

At export time, the local pipeline reported:

- `playable_route_gate_passed: true`
- `gameplay_proof_gate_passed: true`
- `building_integrity_gate_passed: true`
- `weapon_feel_gate_passed: true`
- `ai_playtest_route_gate_passed: true`
- `m84_three_class_asset_factory_spike_passed: true`
- `m85_visual_production_passed: true`
- `m86_build_release_gate_passed: true` as a separate build/release report in `docs/M86_BUILD_RELEASE_GATE.json`
- `m87_class_level_production_visibility_passed: true` as a separate approved-equivalent route gate in `docs/M87_CLASS_LEVEL_PRODUCTION_VISIBILITY_GATE.json`
- `m88_strict_full_visual_asset_gate_passed: false` in `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.json`
- `m89_built_player_clean_capture_passed: true` as launch/window evidence only in `docs/M89_BUILT_PLAYER_CLEAN_CAPTURE.json`
- `m90_built_player_runtime_route_capture_passed: true` in `docs/M90_BUILT_PLAYER_RUNTIME_ROUTE_CAPTURE.json`, with `external_input_driven=false`
- `m91_external_input_built_player_route_passed: true` in `docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json`, with `external_input_driven=true` and `built_player=true`
- `html_tactical_parity_gate_passed: true`
- `all_required_current_gates_passed: true`
- `full_visual_asset_gate_passed: false`
- `realified_asset_gameplay_production_promoted_assets: 1`

The important warning is still the last two lines: only one realified asset has production gameplay promotion evidence, and the full visual asset gate remains false. M84 separately proves one weapon, one approved container, and one approved medkit through gameplay-bound promotion evidence. M85 adds a scoped player-camera visual production pass with wet-route details, decals, clutter, lighting, and before/after screenshots. M86 proves a local macOS build can be produced and launch-smoked. M87 proves five route-level production-visibility classes using approved-equivalent or gameplay-bound assets. M89 adds a cleaner built-player foreground capture. M90 adds a built-player runtime gameplay route proving start, movement, pickup, fire/reload, NPC interaction, death, restart, and first-person weapon visibility, but it is not an external-input-driven route. M91 complements M90 by driving the built macOS app with OS-level external keyboard/mouse input and writing eight player-route screenshots plus a passing Editor gate. M92 adds play-mode weapon production-feel metrics for first-person pose quality, recoil, reload pose, ADS stability, shot feedback, and third-person mount pulse. None of M84, M85, M86, M87, M89, M90, M91, or M92 makes the failed Realified non-weapon batch production-ready or claims final PUBG-like visual quality.

## Known Gaps

- First-person weapon visibility, fire/reload feedback, hit marker, recoil evidence, and third-person mount now have a passing hero-weapon gate, but the authored animation and final weapon art quality are still not commercial-grade.
- Most realified assets are not yet promoted into gameplay entities. M84 intentionally keeps failed Realified non-weapon assets quarantined while promoting semantic-approved container and medkit equivalents for the three-class spike.
- M85 improves player-camera visual density and weathered scene detail, but it deliberately preserves `full_visual_asset_gate_passed=false` because final humanoid, generated asset classes, and commercial-grade visual review remain open.
- M86 produces a local macOS review build under ignored `Builds/M86/`, but the committed visual launch screenshot is only window/process evidence and not clean gameplay-quality evidence.
- M87 closes the route-level class visibility accounting gap with approved-equivalent assets, while preserving the failed legacy Realified batch visibility gate.
- M88 makes the strict full-visual blockers explicit; after M91/M92 accounting refresh, the remaining blockers are final weapon art review, final humanoid art review, and generated batch class promotion.
- M89 improves built-player foreground launch evidence.
- M90 proves the same built-player gameplay route from the macOS app through a runtime recorder, but `external_input_driven=false`.
- M91 proves the same built-player route through external keyboard/mouse automation with `passed=true`, `external_input_driven=true`, and `built_player=true` in `docs/M91ExternalInputBuiltPlayerRoute/M91_EXTERNAL_INPUT_BUILT_PLAYER_ROUTE.json`.
- M92 weapon production pass is locally verified through Unity with `m92_weapon_production_passed=true` in `docs/WEAPON_FEEL_GATE.json` and summarized by `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`.
- Character and environment assets still need stronger production-quality replacements.
- The Unity version still lags the HTML baseline in many interaction and feedback details.
- Some assets are present in `Assets/HtmlTacticalAssets/` but are not yet visible or active in the actual player route.

## Review Request

Please review the project with this priority:

1. Compare Unity `Assets/Scenes/TacticalPrototype.unity` against the HTML baseline in `reference/html_baseline_final_packet/index.html`.
2. Identify concrete code and scene changes needed to reach HTML parity.
3. Prioritize first-person weapon gameplay, reload/fire feedback, pickup, NPC combat, HUD/lobby/death/settings/skins parity, and gameplay-bound asset use.
4. Do not count a GLB as complete unless it is attached to a gameplay entity, visible from the player camera, and participates in a gameplay event.
5. Produce patch-level recommendations, not only high-level advice.

## Unity / MCP Notes

The project uses Unity 6.x conventions. The package manifest includes the community Unity MCP package:

```text
com.coplaydev.unity-mcp
```

Official Unity MCP may require Unity AI plan/seat entitlement. The community MCP route was used locally for menu execution and evidence checks.

## Public Export Safety

This export intentionally excludes Unity generated/cache folders such as:

- `Library/`
- `Temp/`
- `Obj/`
- `Build/`
- `Builds/`
- `Logs/`
- `UserSettings/`

No API keys or private credentials should be committed to this repository.

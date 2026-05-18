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
- `html_tactical_parity_gate_passed: true`
- `all_required_current_gates_passed: true`
- `full_visual_asset_gate_passed: false`
- `realified_asset_gameplay_production_promoted_assets: 1`

The important warning is the last line: only one realified asset has production gameplay promotion evidence. The tactical loop is playable enough for route gates, but the Unity version is still far from the HTML baseline's total gameplay density and far from PUBG-like visual quality.

## Known Gaps

- First-person weapon visibility, fire/reload feedback, hit marker, recoil evidence, and third-person mount now have a passing hero-weapon gate, but the authored animation and final weapon art quality are still not commercial-grade.
- Most realified assets are not yet promoted into gameplay entities.
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

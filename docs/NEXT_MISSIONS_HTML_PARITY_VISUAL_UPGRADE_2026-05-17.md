# Next Missions: HTML Parity Visual Upgrade

Date: 2026-05-17

## Current State

The Unity tactical prototype now has strong gameplay-route evidence:

- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`: PASS.
- Lobby/start/spawn/move/building/warehouse/container/pickup/fire/NPC ranged attack/dynamic spawn/ladder/heal/death/restart are verified.

Recent visual improvements:

- M6 approved crate is bound into gameplay crate objects.
- Wet asphalt and concrete PBR materials are bound into road/wall/floor objects.

Critical remaining issue:

- The generated 12-asset GLB batch imported into Unity but failed visual category correctness. Non-weapon files look weapon-like.
- Nemotron contact-sheet review now confirms the failure: 10 of 12 non-weapon or mixed-category entries are visible as weapon silhouettes, and promotion is blocked.

## Mission 1: Category-Correct Asset Regeneration

Owner: HomePC GPU + Kimi.

Goal:
Regenerate or replace non-weapon assets that failed category shape review.

Priority:

1. Player tactical character.
2. Enemy tactical character.
3. Container.
4. Medkit.
5. Ammo box.
6. Helmet / vest.

Acceptance:

- Each asset has a normalized Unity contact sheet screenshot.
- The silhouette visibly matches the category.
- No asset receives completion credit from file name alone.
- Unity import has no Console errors.
- Only approved assets may be bound into gameplay.

Failure rule:

- If VLM or human visual review says the shape is wrong, mark `asset_quality_status = category_failed` and do not bind it.

Current gate evidence:

- `docs/REALIFIED_ASSETS_NEMOTRON_CONTACT_SHEET_REVIEW.json`
- `docs/REALIFIED_ASSETS_NEMOTRON_CONTACT_SHEET_REVIEW.md`
- Result: `overall_verdict=fail`, `promotion_allowed=false`.

## Mission 2: Player/Enemy Character Upgrade

Owner: Kimi or Codex, with Gemini review.

Goal:
Replace current static/procedural character visual with a rigged or at least articulated tactical character route.

Acceptance:

- Player and enemy are not capsule/proxy-only in player camera evidence.
- Idle/walk/aim/hit/death have visible state changes.
- NPC weapon mount still aligns after the visual change.
- Route gate still passes.

Status update:

- Implemented an intermediate procedural limb animation route in `Assets/Scripts/Tactical/TacticalCharacterMotionVisual.cs`.
- Current gate evidence:
  - `character_procedural_limb_rig_count=11`
  - `character_procedural_limb_animation_count=11`
  - `character_procedural_limb_animation_passed=true`
  - gameplay proof detail: `characterProceduralLimb 12/12 rigged 12`
- This improves over root bobbing/static capsules, but it is not yet final authored humanoid animation or a true rigged-character import.

Next character target:

- Import or build a real rigged humanoid/player-enemy pair with authored clips for idle/walk/aim/fire/hit/down, then make `full_visual_asset_gate` depend on that stronger evidence rather than the current procedural-limb bridge.

## Mission 3: Container And Loot Gameplay Binding

Owner: Codex for Unity integration, HomePC GPU for assets.

Goal:
Import category-approved container and loot assets and bind them into the generated scene.

Acceptance:

- Container yard screenshot shows category-approved container assets.
- Pickup screenshots show category-approved medkit/ammo/armor/helmet assets.
- Pickup state mutation still passes.
- Route gate still passes.

## Mission 4: Decal/Postprocess Pass

Owner: Gemini design review + Codex implementation.

Goal:
Raise the visual grade without changing gameplay by adding decals, lighting, post-processing, fog, and material variation.

Acceptance:

- Before/after player-camera screenshots.
- No route gate regression.
- No large pure-color walls/floors in key screenshots.

## Mission 5: Evidence Gate Upgrade

Owner: Codex.

Goal:
Make the gate fail when category-failed assets are counted as completion.

Acceptance:

- Gate report includes asset ids, asset quality status, and gameplay binding status.
- `category_failed` assets can render but cannot count toward HTML/PUBG visual completion.
- Gate separately reports gameplay pass and visual asset pass.

Status:

- Implemented in `Assets/Editor/TacticalPlayableRouteGate.cs`.
- Latest `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json` includes `gameplay_route_passed`, `approved_incremental_asset_gate_passed`, `full_visual_asset_gate_passed`, `completion_credit`, and an `asset_quality` object.
- Current result: gameplay route passes, approved crate/environment increment passes, full visual asset gate remains false.

Next gate extension:

- Add per-class requirements for approved player, enemy, container, loot, helmet, and vest replacements once regenerated assets exist.

## Mission 6: Unity MCP / AI Factory Efficiency

Owner: Codex/Kimi, with Gemini research review.

Goal:
Stop using MCP as a long fragile interaction channel. Use community practice: deterministic Unity `AI Tools/...` menu commands as the high-level API, community MCP only for short `execute_menu_item` / `read_console` / state checks, and Nemotron for semantic review.

Evidence:

- `docs/COMMUNITY_UNITY_MCP_AI_GAMEDEV_PLAYBOOK_2026-05-17.md`
- `docs/UNITY_MCP_KIMI_FACTORY_HANDOFF_VERIFICATION_2026-05-17.md`

Acceptance:

- One-command MCP smoke script can initialize tools, read console, and run `AI Tools/Run Tactical Acceptance Pipeline`.
- Contact sheet review is required before generated GLBs can be counted in gameplay visual completion.
- No new Unity MCP package is installed into the active project unless the CoplayDev route becomes unreliable.

## Mission Update: Approved Container Bound

Status: partial completion for container class.

- `approved_container_v1.glb` has been generated, imported, and bound to real container-yard and spawn-staging gameplay objects.
- Latest `TACTICAL_PLAYABLE_ROUTE_GATE.json` reports 11 approved container instances and 11 renderer instances.
- The container item should be removed from the immediate failed-category priority list unless the next pass is specifically targeting photoreal/PUBG-quality replacement.

Remaining immediate failed categories:

1. Player tactical character.
2. Enemy tactical character.
3. Ammo box.
4. Helmet.
5. Vest.

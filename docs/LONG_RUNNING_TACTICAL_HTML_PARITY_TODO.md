# Long Running Tactical HTML Parity TODO

This file is the execution rail for the Unity HTML tactical-game parity goal. Do not treat a report, contact sheet, asset import, or script-only state as completion.

## Nonstop Execution Rule

Each work block must land at least one concrete improvement in the playable Unity game before stopping:

- The playable entry must remain `Assets/Scenes/TacticalPrototype.unity`.
- The manual path must remain reproducible: Play -> start round -> first-person weapon visible -> left mouse fire -> `R` reload -> `F` pickup -> NPC engagement.
- A production asset counts only if it is attached to a gameplay entity, visible from player camera, and participates in a gameplay event.
- Contact sheets and fixed preview cameras are diagnostic only.
- `full_visual_asset_gate_passed=false` means the broader goal is still open.

## Execution Queue

### P0: Manual Play Entry

Status: current gate passed on 2026-05-18 03:32 UTC; keep as regression guard.

- Start round defaults to first-person.
- HUD shows current weapon and camera mode.
- Manual start gate verifies first-person weapon renderer is active.
- Keep this gate alive in every later acceptance run.
- Latest evidence:
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `spawn_first_person_weapon_visible=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `spawn_first_person_weapon_enabled_renderers=16`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `spawn_first_person_gameplay_source_glb_renderers=1`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `spawn_camera_clear=true`

### P1: First-Person Weapon Quality

Status: improved and gate-covered, but not final hero-art complete.

- Replace simple block silhouette with a richer in-game weapon kit.
- Add visible optic, rail ridges, foregrip, magazine, muzzle, stock, trigger guard, screws/markings, and gloved hands.
- Ensure the current weapon appears in the player camera on start.
- Gate must count authored weapon-detail parts, enabled first-person renderers, and at least one visible gameplay source GLB renderer.
- Latest evidence:
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `manual_start_first_person_weapon_visible=true`
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `first_person_weapon_polish=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `first_person_weapon_polish_passed=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: fire step detail includes `fpSourceGlbRenderers 1`
- Still open:
  - final first-person hero weapon art review.
  - most production GLB promotion remains blocked by semantic/source-trace/event gates.

### P2: Weapon Feedback

Status: current gate passed; keep as regression guard.

- Keep muzzle flash, tracer, hit marker, casing, recoil, reload offset, and audio events working.
- Verify `fire_ammo_and_enemy_hit`, `reload_state_mutation`, and `weapon_feedback_spawned`.
- Latest evidence:
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `fire_ammo_and_enemy_hit=true`
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `weapon_feedback_spawned=true`
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `reload_state_mutation=true`

### P3: Pickup/Inventory Asset Binding

Status: current approved HTML-parity loot route passed; realified production loot promotion still blocked.

- Promoted ammo, medkit, vest, helmet, and weapon loot must be scene gameplay entities.
- Each asset must show a pickup prompt, mutate state on `F`, and have player-camera evidence.
- Latest evidence:
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `pickup_state_mutation=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `approved_loot_class_route_evidence=true`
- Still open:
  - `realified_loot_class_route_evidence=false`
  - production promoted loot classes remain 0.

### P4: NPC Combat and Dynamic Spawn

Status: current gate passed; keep as regression guard.

- NPCs must keep ranged fire, hit/damage, death, and reinforcement spawn.
- NPC visuals must stay attached to real enemy gameplay entities, not showcase roots.
- Latest evidence:
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `enemy_ranged_attack_state_mutation=true`
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `dynamic_spawn_state_mutation=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `enemy_ranged_attack_mutation=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `dynamic_spawn_mutation=true`

### P5: Environment and Building Parity

Status: current gate passed after restoring container-yard density without re-blocking spawn camera.

- Preserve HTML-style compound, buildings, warehouse, container yard, ladders, loot tables, doors, windows, props, and upper-floor route.
- Add visible detail only when it improves player route readability.
- Latest evidence:
  - `TACTICAL_GAMEPLAY_PROOF_GATE.json`: `environment_player_flow_verified=true`
  - `HTML_TACTICAL_PARITY_GATE.json`: `building_environment_detail_parity=true`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`: `spawn_camera_clear=true`

### P6: Visual Asset Promotion

Status: partially advanced; full visual asset promotion still blocked.

- Fix or replace Realified class assets that failed semantic review.
- Do not globally promote any batch that still renders weapons for character/gear/loot/environment categories.
- Move class-by-class only after technical + semantic + player-camera gameplay evidence passes.
- Latest evidence:
  - `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`: `full_visual_asset_gate_passed=false`
  - `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`: `category_semantic_review_passed=false`
  - `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`: `realified_promotion_production_promoted_classes=0`
  - `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`: `realified_asset_gameplay_production_promoted_assets=1`
  - `REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`: `hero_rifle.production_promoted=true`
  - `REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`: sidearm and secondary weapon are technical/semantic/scene partials, but lack asset-specific event and/or player-camera evidence.
  - `PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`: class-level promoted visibility remains false because no full class is production-promoted.

### P7: Final Completion Audit

Status: blocked until P1-P6 pass.

- `TacticalPrototype.unity` is the sole main playable entry.
- Manual Play route passes.
- HTML parity gate passes.
- Gameplay proof gate passes.
- Playable route gate passes.
- `full_visual_asset_gate_passed=true`.
- Current audit status:
  - Current route/parity gates pass.
  - Final goal remains active because `full_visual_asset_gate_passed=false`.

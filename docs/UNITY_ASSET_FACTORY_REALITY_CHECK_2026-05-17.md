# Unity Asset Factory Reality Check

Date: 2026-05-17

## Short Verdict

The local asset pipeline can generate and import textured GLBs into Unity, but the current batch should not be treated as production-ready gameplay assets.

Evidence:

- `Assets/HtmlTacticalAssets/RealifiedAssets/` contains 48 GLBs.
- Unity imported all 48 GLBs and detected glTF PBR material slots.
- The 12 final `*_textured.glb` assets render in `Assets/Screenshots/glb_pbr_textured_contact_sheet.png`.
- The rendered contact sheet shows that the named asset classes are visually wrong: files named as player, enemy, helmet, vest, ammo, medkit, container, and crate still appear as weapon-like silhouettes.

## What Passed

- Third-party Unity MCP is usable for editor automation.
- Unity Console stayed clean during the import and screenshot gates.
- glTFast imported the GLBs with `Shader Graphs/glTF-pbrMetallicRoughness`.
- Each imported asset has a base color texture and a metallic/roughness texture property.
- The route gate for the HTML-style playable prototype passed separately in `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`.

## What Failed

- Category correctness failed: the visual shape does not match the file name/category.
- Normal map evidence is missing: `normal_slots` is `0` for the current GLBs.
- PBR is minimal: base color plus metallic-roughness is present, but this is not enough for high-realism asset approval.
- The assets are not yet proven as player-camera gameplay replacements.
- The contact sheet proves import/rendering, not gameplay quality.

## Updated Acceptance Rule

An asset only counts toward HTML parity or PUBG-like upgrade when all of these pass:

1. It imports into Unity with no Console errors.
2. It has a rendered visual review screenshot.
3. Its silhouette and category match its asset id.
4. Its material maps are appropriate for the category.
5. It is bound to a real gameplay entity.
6. It appears from the player camera during a route gate.
7. It participates in a gameplay event when applicable: pickup, fire, reload, damage, heal, equip, or obstacle/environment visibility.

## Immediate Next Fix

Do not blindly bind this whole batch into the tactical game. First regenerate or replace category-failed assets in priority order:

1. `RS_04_player_tactical_textured.glb`
2. `RS_05_enemy_tactical_textured.glb`
3. `RS_10_prop_container_textured.glb`
4. `RS_11_prop_crate_textured.glb`
5. `RS_09_loot_medkit_textured.glb`
6. `RS_08_loot_ammo_textured.glb`

The weapon-like assets can stay as temporary weapon candidates, but they cannot stand in for characters, props, and loot.

## Approved Crate Exception

One asset from the M6 PBR asset-factory mission is currently stronger than the failed 12-asset batch:

- Source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M6_pbr_asset_factory/model/tactical_crate_textured_mac_verified.glb`
- Unity copy: `Assets/HtmlTacticalAssets/ApprovedAssets/tactical_crate_v1.glb`
- Blender preview: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M6_pbr_asset_factory/evidence/blender_preview_mac_verified.png`

Unity validation:

- Imported prefab: PASS.
- Renderer count: 1.
- Material count: 1.
- Shader: `Universal Render Pipeline/Lit`.
- Tactical scene usage: 11 `tactical_crate_v1` instances with renderers.
- HTML route gate after replacement: PASS.
- Player route evidence: `Assets/Screenshots/PlayableRoute/04_warehouse_route.png`.

This crate now counts as a partial gameplay-bound approved prop candidate. It is still not a full PUBG-like production prop, but it is a better replacement than the old HTML procedural crate and the failed `RS_11_prop_crate_textured.glb` batch asset.

## Approved Environment Material Binding

Two texture-only asset packets were copied into Unity and wired into the generated tactical scene:

- Wet asphalt source: `/Users/yuanshaochen/Projects/ai-game-generation-research/experiments/pubg_like_asset_factory_20260513/assets/wet_asphalt_material_v1/textures/`
- Concrete wall source: `/Users/yuanshaochen/Projects/ai-game-generation-research/experiments/pubg_like_asset_factory_20260513/assets/concrete_wall_material_v1/textures/`
- Unity material root: `Assets/HtmlTacticalAssets/ApprovedMaterials/`

Unity material results:

- `TacticalWetAsphaltApproved`: 5 texture slots.
- `TacticalConcreteWallApproved`: 5 texture slots.
- `TacticalConcreteFloorApproved`: 5 texture slots.
- Road renderers using approved wet asphalt material: 2.
- Concrete renderers using approved concrete material: 71.
- HTML route gate after replacement: PASS.

Player-camera evidence:

- `Assets/Screenshots/PlayableRoute/02_spawn_exit_route.png`
- `Assets/Screenshots/PlayableRoute/03_building_a_entry.png`
- `Assets/Screenshots/PlayableRoute/04_warehouse_route.png`

Limit:

- These are heuristic material maps, not final art-directed production materials.
- Roughness exists as a source map, but the current URP material wiring uses scalar smoothness plus metallic/occlusion/normal/base maps. A later pass should pack metallic/roughness/AO into a proper Unity mask map or use a shader graph that consumes roughness explicitly.

## Route Gate Asset Quality Upgrade

`docs/TACTICAL_PLAYABLE_ROUTE_GATE.json` now separates gameplay route status from visual completion status.

Current verified fields:

- `passed`: `true`
- `gameplay_route_passed`: `true`
- `approved_incremental_asset_gate_passed`: `true`
- `visual_polish_gate_passed`: `true`
- `full_visual_asset_gate_passed`: `false`
- `completion_credit`: `gameplay_route_plus_incremental_player_enemy_container_crate_medical_ammo_helmet_vest_environment_postprocess_weather_only`
- `known_category_failed_batch_files`: `12`
- `category_failed_scene_instances`: `0`

Meaning:

- The Unity version currently proves the HTML-style playable route and the approved crate/environment material increment.
- It explicitly does not claim the full visual/asset objective is complete.
- The failed 12-asset batch is known and tracked, but not counted as bound scene completion.

## Approved Container Replacement

A category-correct container asset was generated outside the failed 12-asset Hunyuan batch and promoted into gameplay.

- Source script: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/scripts/make_approved_container.py`
- Source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/model/approved_container_v1.glb`
- Unity copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_container_v1.glb`
- Preview: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/evidence/approved_container_v1_preview.png`

Unity validation:

- Container yard and spawn-staging containers now bind to `approved_container_v1.glb`.
- Latest route gate: PASS.
- `approved_container_instances`: 11.
- `approved_container_renderer_instances`: 11.
- `category_failed_scene_instances`: 0.
- Player-camera evidence: `Assets/Screenshots/PlayableRoute/05_container_yard_route.png`.

Limit:

- This container is a category-correct hard-surface gameplay replacement, not a final photoreal/PUBG production asset.
- It still counts only as incremental HTML-parity visual progress.

## Approved Tactical Character Replacements

Two category-correct tactical humanoid assets were generated outside the failed 12-asset Hunyuan batch and promoted into gameplay.

- Player source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/model/approved_player_tactical_v1.glb`
- Enemy source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/model/approved_enemy_tactical_v1.glb`
- Unity player copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_player_tactical_v1.glb`
- Unity enemy copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_enemy_tactical_v1.glb`
- Player preview: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/evidence/approved_player_tactical_v1_preview.png`
- Enemy preview: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M18_category_correct_asset_replacement/evidence/approved_enemy_tactical_v1_preview.png`

Unity validation:

- The gameplay player binds to `approved_player_tactical_v1.glb`.
- Enemy spawns and reinforcement template bind to `approved_enemy_tactical_v1.glb`.
- Latest route gate: PASS.
- `approved_player_instances`: 1.
- `approved_player_renderer_instances`: 1.
- `approved_enemy_instances`: 11.
- `approved_enemy_renderer_instances`: 11.
- Player-camera evidence:
  - `Assets/Screenshots/PlayableRoute/01_spawn_after_start.png`
  - `Assets/Screenshots/PlayableRoute/09_enemy_ranged_attack.png`

Limit:

- These characters are category-correct tactical humanoid replacements, not final rigged or photoreal characters.
- They still use the existing runtime motion/proxy animation layer, so authored humanoid animation remains a blocker for full visual completion.

## Approved Ammo / Helmet / Vest Replacement

The remaining category-missing loot assets have now been replaced with category-correct approved GLBs.

- Ammo source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M21_remaining_category_correct_assets/model/approved_ammo_loot_v1.glb`
- Helmet source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M21_remaining_category_correct_assets/model/approved_helmet_loot_v1.glb`
- Vest source GLB: `/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M21_remaining_category_correct_assets/model/approved_vest_loot_v1.glb`
- Unity ammo copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_ammo_loot_v1.glb`
- Unity helmet copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_helmet_loot_v1.glb`
- Unity vest copy: `Assets/HtmlTacticalAssets/ApprovedAssets/approved_vest_loot_v1.glb`

Latest route gate:

- `approved_ammo_loot_instances`: 4.
- `approved_ammo_loot_renderer_instances`: 4.
- `approved_helmet_loot_instances`: 4.
- `approved_helmet_loot_renderer_instances`: 4.
- `approved_vest_loot_instances`: 4.
- `approved_vest_loot_renderer_instances`: 4.
- `category_failed_scene_instances`: 0.
- `completion_credit`: `gameplay_route_plus_incremental_player_enemy_container_crate_medical_ammo_helmet_vest_environment_only`.

Limit:

- These are category-correct low-poly gameplay replacements, not final photoreal/PUBG production assets.

## Gameplay-Bound Visual Polish / Weather Pass

The generated tactical Unity route now includes a verified visual polish layer.

Unity generator changes:

- global tactical postprocess volume;
- color adjustments, bloom, vignette, and film grain;
- cool fog and ambient lighting;
- three industrial/checkpoint/warehouse lights;
- rain particle field;
- postprocess-enabled tactical camera.

Latest route gate:

- `visual_polish_gate_passed`: `true`.
- `tactical_postprocess_volume_count`: 1.
- `tactical_rain_field_count`: 1.
- `tactical_industrial_light_count`: 3.
- `tactical_postprocess_camera_count`: 1.

Player-camera evidence:

- `Assets/Screenshots/PlayableRoute/01_spawn_after_start.png`
- `Assets/Screenshots/PlayableRoute/05_container_yard_route.png`
- `Assets/Screenshots/PlayableRoute/08_fire_hit_first_person.png`

Limit:

- This improves mood and route screenshots, but it is not a substitute for production assets, authored animation, or first-person weapon polish.

## Current Remaining Visual Blockers

- Authored humanoid rig/animation rather than procedural motion.
- Stronger first-person weapon hero mesh and animation polish.
- Higher-quality production art beyond low-poly category-correct placeholders.

# M108 Context Bundle - Unity Visible Value and Asset Chain Problems

This bundle is for ChatGPT PRO to generate one consolidated patch package. Codex owns local Unity/MCP verification. PRO should not claim Unity verification.

## User Goal

The target is PUBG-like tactical visual realism, not just passing process gates. The user is dissatisfied because recent M97/M98/M100 work technically changed reports but the in-game visual improvement is still too small.

## Current Local Ground Truth

- Unity active project: `/Users/yuanshaochen/My project`
- Main scene: `Assets/Scenes/TacticalPrototype.unity`
- Community Unity MCP: CoplayDev/community MCP at `http://127.0.0.1:8080/mcp`
- Official Unity MCP is no longer mainline.
- Codex can run shell/git/python locally and can execute Unity `AI Tools/...` through community MCP.
- Kimi/acpx was delegated M106B image-to-3D fresh probe, but its Shell tool failed with Internal errors for `ls/cp/ssh/python3`; Codex local shell is working.

## What Changed Locally Before This Bundle

Codex made a small local Unity fix:

1. `M100HeroWeaponViewmodelController.ForcePreview()` now calls `ResolveReferences()`, `EnsureFxObjects()`, `ApplyPose()`, `SuppressOldFirstPersonProxy()`, and `UpdateScreenRect()` before screenshot proof.
2. `M100PubgRealismVerticalSliceTool.InferWeaponViewmodelEuler()` changed `hero_rifle` yaw from `+92` to `-88` because the user observed the muzzle pointed backward.
3. M100 weapon bounds normalized from `0.78` to `1.15`, and viewmodel pose moved inward/up to improve player-camera framing.

## Current Evidence After Local Fix

`docs/M100_PUBG_REALISM_VERTICAL_SLICE_PROOF.json` now shows:

- `passed: true`
- `uses_realified_weapon_asset: true`
- `viewmodel_screen_area: 0.122`
- `shot_visual_events: 1`
- `reload_visual_events: 1`
- `blockers: []`

However the screenshot is still visually not good enough: the rifle looks cropped/awkward, dark/low detail, too much like an imported object jammed into view rather than a polished PUBG-like first-person weapon. See:

- `screenshots/m100/01_hero_weapon_idle.png`
- `screenshots/m100/02_hero_weapon_ads.png`
- `screenshots/m100/03_hero_weapon_fire_fx.png`
- `screenshots/m100/04_hero_weapon_reload.png`

## Major Problems To Solve

### Problem 1: First-person weapon still looks wrong despite M100 proof passing

Symptoms:

- It now passes the M100 local proof, but visually it still reads poorly.
- Rifle is large enough for gate but not composed like a proper FPS viewmodel.
- Earlier user observed muzzle pointed backward; local yaw fix likely improved orientation but visual still needs a robust solution.
- The system has too many overlapping weapon visual layers: M97 candidate slot, M98 authored primitive viewmodel, M100 realified viewmodel overlay, old `TacticalFirstPersonWeaponVisual`.

Ask PRO to generate a consolidated Unity C# patch that simplifies this into one clear runtime/editor path:

- A robust first-person viewmodel composer with explicit per-asset orientation/scale/offset config.
- A menu command to apply best hero rifle visual to first-person and third-person mounts.
- A proof command that captures idle/ADS/fire/reload/player route screenshots.
- No fake M88/M95 pass, no direct scene YAML edits.

### Problem 2: Need actions/effects, not just static asset visibility

User wants visible value: fire, recoil, reload, muzzle flash, casing, hit feedback, enemy reaction. The patch should improve action readability:

- ADS pose should be readable and not block view.
- Fire should show muzzle flash/tracer/hit marker/casing.
- Reload should visibly move/tilt weapon, even if not final animation.
- Enemy should show hit/down feedback in the small combat slice.

### Problem 3: Asset generation chain must be part of the same plan

PRO generated clean-background reference images. Codex archived them:

- `screenshots/pro_refs/pro_clean_refs_contact_sheet_20260519.png`
- `reports/pro_clean_refs_manifest.json`

These images are inputs only, not production assets. M106B Kimi fresh image-to-3D failed because Kimi shell execution broke, not because the images/models are known bad. PRO should provide:

- A practical image-to-3D task plan for enemy_tactical and container_cover using the included clean refs.
- A small script/patch if useful for selecting/copying/packaging candidates.
- Do not claim fresh generation occurred.

### Problem 4: Need a debug-friendly spawn/camera/combat slice

Current player view often faces a low-visibility area/container wall. Need a purpose-built debug/combat slice:

- Spawn facing a readable courtyard/combat lane.
- One container/cover asset, one enemy, one loot item, one building facade/checkpoint prop.
- Player can move, fire, reload, hit enemy, see feedback.
- Use as the visible-value screenshot route, without replacing the full map.

## Desired PRO Deliverables

Please create downloadable artifacts:

1. `M108_visible_value_weapon_enemy_combat_slice.patch`
2. `M108_visible_value_weapon_enemy_combat_slice_patch.zip`
3. `README_FOR_PATCH.md`
4. `changed_files_manifest.json`
5. `validation_notes.md`
6. Optional: `image_to_3d_homepc_commands.md`

## Patch Constraints

Allowed to edit/add:

- `Assets/Scripts/Tactical/*.cs`
- `Assets/Editor/*.cs`
- `docs/M108_*.md` or templates
- optional local repo helper scripts under `tools/` only if lightweight

Do not edit:

- `Assets/Scenes/*.unity`
- `Packages/*`
- `ProjectSettings/*`
- existing gate JSON pass/fail values
- large generated binary assets

Use Unity Editor APIs/menu commands to alter scene state. Do not hand-edit scene YAML.

## Local Verification That Codex Will Run

After applying your patch, Codex will run through community MCP:

1. Refresh Unity and wait for compile idle.
2. Read Console; compile errors must be zero.
3. Execute your install/apply menu command.
4. Execute your proof/capture menu command.
5. Inspect generated JSON and screenshots.
6. Compare before/after screenshots manually.

## Efficiency Requirement

Do not add more big OS/gate infrastructure. This must be a value patch. It should make the game visibly better in screenshots/player camera.

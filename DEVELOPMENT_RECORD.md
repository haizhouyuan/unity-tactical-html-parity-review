# Development Record: Unity Tactical HTML Parity Export

Date: 2026-05-18

Repository: `haizhouyuan/unity-tactical-html-parity-review`

This document records the development process, methods, staged deliverables, pivots, mistakes, evidence gates, and current handoff state for the Unity tactical-game recreation effort.

It is intentionally blunt. This project is not finished. It is a public review/export checkpoint for diagnosing how to get the Unity version to at least match the older HTML/Three.js tactical prototype.

## 1. Product Goal

The active product goal was to recreate the prior HTML/Three.js tactical game in Unity at equal or better quality.

Main Unity scene:

```text
Assets/Scenes/TacticalPrototype.unity
```

HTML baseline:

```text
reference/html_baseline_final_packet/index.html
```

The target was not just "assets are imported" or "screenshots look better." The target was a playable tactical loop:

- lobby/start/death/settings/skin flow;
- first-person and third-person camera modes;
- visible first-person weapon at spawn;
- ADS, fire, reload, and weapon switching;
- crouch, prone, jump;
- pickup and inventory/state mutation;
- NPC ranged combat, hit feedback, and death state;
- dynamic enemy spawn/survival loop;
- floor, ladder, building, warehouse, and container traversal;
- tactical HUD parity;
- imported assets bound to gameplay entities;
- player-camera evidence, not just fixed showcase screenshots.

## 2. Hard Acceptance Rule

An asset only counts as product progress if all of these are true:

1. It is imported into Unity.
2. It is semantically correct for its gameplay role.
3. It is attached to a real gameplay entity.
4. It is visible from the player camera during normal Play.
5. It participates in a gameplay event where relevant.

Gameplay events include fire, reload, pickup, damage, heal, armor equip, enemy attack, player death, or environment traversal.

The following do not count as completion by themselves:

- GLB exists on disk;
- asset packet exists;
- contact sheet screenshot exists;
- fixed evidence camera sees it;
- runtime proxy rig exists;
- visual-only overlay exists;
- hash manifest exists;
- material map count exists without visual/gameplay proof.

## 3. Working Method

### 3.1 Unity Editor Automation First

I avoided directly hand-editing large `.unity` scene YAML. Instead, the project uses Editor scripts and menu items to generate or validate scene state.

Important editor scripts:

```text
Assets/Editor/AiGameProjectTools.cs
Assets/Editor/TacticalPrototypeTools.cs
Assets/Editor/TacticalAcceptancePipeline.cs
Assets/Editor/TacticalPlayableRouteGate.cs
Assets/Editor/TacticalGameplayProofGate.cs
Assets/Editor/HtmlTacticalParityGate.cs
Assets/Editor/RealifiedAssetGameplayPromotionLedger.cs
Assets/Editor/RealifiedAssetPromotionQueue.cs
Assets/Editor/RealifiedImportMaterialGate.cs
Assets/Editor/PromotedAssetPlayerCameraVisibilityGate.cs
```

This made scene creation and gates repeatable from Unity menu commands rather than relying on manual scene edits.

### 3.2 MCP And Fallback Control

The official Unity MCP path was investigated first. Local relay and Unity Bridge configuration were mostly correct, but official Unity MCP was blocked by Unity plan/seat entitlement.

Evidence:

```text
docs/UNITY_MCP_STATUS_2026-05-16.md
```

The key blocker was:

```text
ValidationReason: Your Unity plan doesn't include MCP connections.
```

The fallback path was:

- direct Codex file edits;
- Unity `Assets/Editor/` menu tools;
- community Unity MCP package in `Packages/manifest.json`;
- menu execution and read-console checks through the community MCP route;
- screenshot/gate generation from Unity Editor scripts.

Community MCP package:

```text
com.coplaydev.unity-mcp
```

### 3.3 Evidence-Gated Development

The workflow became gate-driven:

1. Generate or update Unity scene via Editor tool.
2. Run a Unity menu command / acceptance pipeline.
3. Capture screenshots and JSON reports.
4. Fail gates if player-camera route, gameplay state mutation, or asset-binding requirements are not met.
5. Update TODO/status docs with exact current evidence.

Main acceptance reports:

```text
docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json
docs/TACTICAL_PLAYABLE_ROUTE_GATE.json
docs/TACTICAL_GAMEPLAY_PROOF_GATE.json
docs/HTML_TACTICAL_PARITY_GATE.json
docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json
docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json
```

### 3.4 Public Export Discipline

For public GitHub export, I created a separate clean repository folder instead of pushing the original dirty Unity working directory.

Export folder:

```text
/Users/yuanshaochen/Projects/unity-tactical-html-parity-review
```

Excluded:

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
MemoryCaptures/
```

Also scanned for common API key/token/password patterns before pushing.

## 4. Phase Log

### Phase 0: HTML Baseline And Reality Check

Input baseline:

```text
reference/html_baseline_final_packet/
```

Important files:

```text
reference/html_baseline_final_packet/index.html
reference/html_baseline_final_packet/source/14.html
reference/html_baseline_final_packet/assets/asset_registry.json
reference/html_baseline_final_packet/assets/asset_inventory_matrix.json
reference/html_baseline_final_packet/evidence/
reference/html_baseline_final_packet/tools/
```

What was learned:

- The HTML version is a dense tactical prototype with lots of gameplay/HUD detail.
- It already has GLB evidence and CDP reports.
- It is not photoreal or commercial-grade, but it has more gameplay completeness than the first Unity attempts.
- Matching it requires preserving gameplay loops, not just creating a prettier scene.

Deliverables:

```text
reference/html_baseline_final_packet/
docs/CHATGPT_PRO_UNITY_HTML_PARITY_REVIEW_REQUEST_2026-05-18.md
docs/PUBLIC_REVIEW_SCOPE.md
```

### Phase 1: Unity Project Setup

Unity project root originally:

```text
/Users/yuanshaochen/My project
```

Public export root:

```text
/Users/yuanshaochen/Projects/unity-tactical-html-parity-review
```

Work performed:

- Created/used Unity 6.x project.
- Added `.gitignore`.
- Added `AGENTS.md`.
- Confirmed packages and project settings.
- Installed/configured Unity AI Assistant package and later community MCP package.
- Kept Unity generated/cache folders out of Git.

Deliverables:

```text
.gitignore
AGENTS.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/
docs/UNITY_PROJECT_REPORT.md
docs/UNITY_MCP_STATUS_2026-05-16.md
```

### Phase 2: Simple Prototype Infrastructure

Before the tactical recreation became the focus, a child-friendly prototype workflow was created to test Unity Editor automation.

Scripts and scene roots:

```text
Assets/Scripts/Player/
Assets/Scripts/Collectibles/
Assets/Scripts/Game/
Assets/Scenes/Main.unity
```

This was useful to prove Unity script generation and scene automation, but it was not the product target.

Deliverables:

```text
Assets/Scripts/Player/StarCollectorPlayerController.cs
Assets/Scripts/Player/CameraFollow.cs
Assets/Scripts/Collectibles/CollectibleStar.cs
Assets/Scripts/Game/StarCollectorGameManager.cs
Assets/Scenes/Main.unity
Assets/Screenshots/star_collector_playmode.png
```

### Phase 3: First Tactical Unity Recreation

The tactical scene generation moved into:

```text
Assets/Editor/TacticalPrototypeTools.cs
```

Runtime tactical scripts:

```text
Assets/Scripts/Tactical/TacticalGameManager.cs
Assets/Scripts/Tactical/TacticalPlayerController.cs
Assets/Scripts/Tactical/TacticalEnemy.cs
Assets/Scripts/Tactical/TacticalLoot.cs
Assets/Scripts/Tactical/TacticalWeaponSpec.cs
Assets/Scripts/Tactical/TacticalFirstPersonWeaponVisual.cs
Assets/Scripts/Tactical/TacticalThirdPersonWeaponVisual.cs
Assets/Scripts/Tactical/TacticalCameraFollow.cs
Assets/Scripts/Tactical/TacticalLadder.cs
Assets/Scripts/Tactical/HtmlGlbAssetMount.cs
```

Implemented or scaffolded:

- lobby/start flow;
- HUD;
- first-person/third-person camera modes;
- player movement;
- crouch/prone/jump style controls;
- weapon specs;
- fire/reload/ammo state;
- pickup route;
- enemies and ranged attack state;
- compound/building/container/warehouse/ladder style layout;
- tactical route screenshots.

Deliverables:

```text
Assets/Scenes/TacticalPrototype.unity
Assets/Prefabs/
Assets/Screenshots/tactical_html_replica_current_player_pov_verified.png
docs/TACTICAL_PROTOTYPE_REPORT.md
docs/TACTICAL_MANUAL_PLAY_QUICKSTART.md
```

Known problem:

The first versions still felt much worse than the HTML baseline. The scene existed, but the user correctly pointed out that the player route, first-person weapon, and gameplay feel were not acceptable.

### Phase 4: Playability And Player-POV Gates

To stop judging only screenshots, I added player-route gates.

Important gates:

```text
Assets/Editor/TacticalPlayableRouteGate.cs
Assets/Editor/TacticalGameplayProofGate.cs
Assets/Editor/TacticalPlayerPovGate.cs
```

These gates verify:

- spawn camera is clear;
- first-person weapon is visible;
- route traverses important zones;
- ammo changes on fire/reload;
- enemy HP/player HP state changes;
- pickup mutates state;
- visual feedback spawns;
- environment flow is still traversable.

Key evidence files:

```text
docs/TACTICAL_PLAYABLE_ROUTE_GATE.json
docs/TACTICAL_GAMEPLAY_PROOF_GATE.json
docs/TACTICAL_PLAYER_POV_GATE.json
Assets/Screenshots/PlayableRoute/
```

Latest exported values:

```text
TACTICAL_PLAYABLE_ROUTE_GATE.json:
  passed: true
  spawn_first_person_weapon_visible: true
  spawn_first_person_weapon_enabled_renderers: 16
  spawn_first_person_gameplay_source_glb_renderers: 1
  spawn_camera_clear: true

TACTICAL_GAMEPLAY_PROOF_GATE.json:
  passed: true
  manual_start_first_person_weapon_visible: true
  first_person_weapon_polish: true
```

Known limitation:

The gates now prove a playable route, but they do not prove PUBG-like art quality.

### Phase 5: Asset Factory Import And Failure

A local AI/PBR asset pipeline produced GLBs and texture candidates.

Unity asset roots:

```text
Assets/HtmlTacticalAssets/RealifiedAssets/
Assets/HtmlTacticalAssets/ApprovedAssets/
Assets/HtmlTacticalAssets/ApprovedMaterials/
```

The 12-asset Realified batch imported into Unity, but it failed semantic review.

Reality check:

```text
docs/UNITY_ASSET_FACTORY_REALITY_CHECK_2026-05-17.md
```

Key failure:

Files named as character, gear, loot, prop, and environment assets often rendered as weapon-like silhouettes.

This is why many impressive-looking asset screenshots did not appear in the actual game: they were not semantically safe to bind into gameplay, and many were only contact-sheet or import proofs.

Deliverables:

```text
Assets/HtmlTacticalAssets/RealifiedAssets/
docs/REALIFIED_ASSETS_IMPORT_AUDIT.md
docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.md
docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.md
docs/REALIFIED_IMPORT_MATERIAL_GATE.md
Assets/Screenshots/glb_pbr_textured_contact_sheet.png
```

### Phase 6: Approved Category-Correct Replacements

Because the Realified batch was not semantically reliable, I switched to class-by-class approved replacements.

Approved roots:

```text
Assets/HtmlTacticalAssets/ApprovedAssets/
Assets/HtmlTacticalAssets/ApprovedMaterials/
Assets/HtmlTacticalAssets/ApprovedAnimations/
```

Approved or partially approved classes:

- crate;
- container;
- player tactical;
- enemy tactical;
- ammo loot;
- medkit/medical loot;
- helmet loot;
- vest loot;
- wet asphalt material;
- concrete wall/floor material;
- weapon candidates.

Representative deliverables:

```text
Assets/HtmlTacticalAssets/ApprovedAssets/tactical_crate_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_container_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_player_tactical_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_enemy_tactical_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_ammo_loot_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_helmet_loot_v1.glb
Assets/HtmlTacticalAssets/ApprovedAssets/approved_vest_loot_v1.glb
Assets/HtmlTacticalAssets/ApprovedMaterials/wet_asphalt/
Assets/HtmlTacticalAssets/ApprovedMaterials/concrete_wall/
```

Known limitation:

These are category-correct gameplay replacements, not final photoreal or PUBG-quality assets.

### Phase 7: First-Person Weapon Recovery

The user reported that the game had no visible first-person gun and no acceptable shooting/reload feel.

I changed the first-person route to force a visible first-person weapon stack:

- authored silhouette parts;
- source GLB renderer visible under the authored weapon;
- renderer-count gate;
- fire/reload state gates;
- muzzle flash/tracer/hit marker feedback.

Relevant files:

```text
Assets/Editor/TacticalPrototypeTools.cs
Assets/Editor/TacticalPlayableRouteGate.cs
Assets/Scripts/Tactical/TacticalFirstPersonWeaponVisual.cs
Assets/Scripts/Tactical/TacticalGameManager.cs
```

Latest evidence:

```text
docs/TACTICAL_PLAYABLE_ROUTE_GATE.json:
  spawn_first_person_weapon_enabled_renderers: 16
  spawn_first_person_gameplay_source_glb_renderers: 1
```

Screenshots:

```text
Assets/Screenshots/PlayableRoute/01_spawn_after_start.png
Assets/Screenshots/PlayableRoute/08_fire_hit_first_person.png
```

Known limitation:

The gun is now visible and gate-covered, but still not final hero-weapon art. It needs better animation, better authored geometry, stronger GLB integration, hands, ADS polish, and visual review.

### Phase 8: Acceptance Pipeline

The separate gates were gathered into:

```text
Assets/Editor/TacticalAcceptancePipeline.cs
```

The acceptance pipeline writes:

```text
docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json
```

Latest exported status:

```text
status: complete
console_errors: 0
console_warnings: 0
playable_route_gate_passed: true
gameplay_proof_gate_passed: true
html_tactical_parity_gate_passed: true
all_required_current_gates_passed: true
full_visual_asset_gate_passed: false
realified_asset_gameplay_partial_assets: 3
realified_asset_gameplay_production_promoted_assets: 1
```

Interpretation:

- The current route/parity gates pass.
- The broader visual/asset goal is not complete.
- Only one Realified asset has production gameplay promotion evidence.

### Phase 9: Public GitHub Export

I created a separate public export repo:

```text
/Users/yuanshaochen/Projects/unity-tactical-html-parity-review
```

Created GitHub repo:

```text
https://github.com/haizhouyuan/unity-tactical-html-parity-review
```

Commit:

```text
ad74c7d Public Unity tactical parity review export
```

Export checks:

- removed Unity generated/cache folders;
- scanned for common key/token/password patterns;
- checked no files exceeded GitHub hard 100 MB limit;
- included the HTML baseline under `reference/html_baseline_final_packet/`;
- added public README and review scope.

GitHub warning:

```text
Assets/HtmlTacticalAssets/ApprovedAssets/medical_loot_v1.glb is 66.63 MB
```

This is above GitHub's recommended 50 MB size but below the 100 MB hard limit. Future large assets should probably move to Git LFS or release artifacts.

## 5. Staged Deliverables

| Stage | Deliverable | Path |
| --- | --- | --- |
| HTML baseline | Final packet | `reference/html_baseline_final_packet/` |
| Unity project setup | Project settings and package manifest | `ProjectSettings/`, `Packages/` |
| Editor automation | Scene/gate/menu tools | `Assets/Editor/` |
| Runtime tactical loop | Gameplay scripts | `Assets/Scripts/Tactical/` |
| Tactical scene | Main playable scene | `Assets/Scenes/TacticalPrototype.unity` |
| Screenshots | Player route and visual evidence | `Assets/Screenshots/` |
| Approved assets | Category-correct gameplay replacements | `Assets/HtmlTacticalAssets/ApprovedAssets/` |
| Approved materials | Wet asphalt/concrete/PBR maps | `Assets/HtmlTacticalAssets/ApprovedMaterials/` |
| Realified candidates | Imported generated GLB candidates | `Assets/HtmlTacticalAssets/RealifiedAssets/` |
| Route gate | Normal Play route evidence | `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json` |
| Gameplay gate | Fire/reload/pickup/enemy proof | `docs/TACTICAL_GAMEPLAY_PROOF_GATE.json` |
| Parity gate | HTML parity summary | `docs/HTML_TACTICAL_PARITY_GATE.json` |
| Asset ledger | Asset-level production-promotion status | `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json` |
| Current task rail | Long-running TODO | `docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md` |
| Public review request | ChatGPT Pro review prompt | `docs/CHATGPT_PRO_UNITY_HTML_PARITY_REVIEW_REQUEST_2026-05-18.md` |

## 6. Mistakes, Pivots, And Lessons

### Mistake 1: Treating Visual Proof As Product Proof

Some earlier work produced attractive screenshots and contact sheets, but those assets were not actually in the gameplay route. The correct test is player-camera gameplay evidence.

Pivot:

Added route gates and the asset promotion rule:

```text
gameplay entity + player-camera visibility + gameplay event
```

### Mistake 2: Independent Visual Micro-Slice Was Not The Game

A standalone visual scene can look better than the real playable route. That does not satisfy the goal.

Pivot:

The main entry is now explicitly:

```text
Assets/Scenes/TacticalPrototype.unity
```

Diagnostic/contact-sheet scenes are not product completion.

### Mistake 3: Batch AI Assets Were Semantically Wrong

The Realified 12-asset batch imported and rendered, but many categories were visually wrong.

Pivot:

Do not bulk-promote generated assets. Promote class-by-class only after semantic review and gameplay evidence.

### Mistake 4: Official Unity MCP Was Misleadingly Close

Local official MCP config was mostly right, but Unity entitlement blocked actual use.

Pivot:

Use community MCP and Editor menu tools for the current workflow. Keep official MCP as optional future improvement after plan/seat status is solved.

### Mistake 5: Gate Pass Could Still Hide Low Visual Quality

Gameplay gates can prove state mutation while art remains weak.

Pivot:

Keep `full_visual_asset_gate_passed=false` until asset and visual quality reach the required standard.

## 7. Current Status

Current pass/fail summary:

```text
playable_route_gate_passed: true
gameplay_proof_gate_passed: true
html_tactical_parity_gate_passed: true
all_required_current_gates_passed: true
full_visual_asset_gate_passed: false
realified_asset_gameplay_production_promoted_assets: 1
```

The Unity prototype is playable enough for current route gates, but it is not a finished HTML replacement and not a PUBG-like visual result.

## 8. Open Work

Highest-priority remaining work:

1. Improve first-person weapon quality and animation.
2. Add sidearm and secondary-weapon player-camera gameplay event evidence.
3. Replace or regenerate semantically wrong Realified assets.
4. Promote loot/gear/environment assets only after player-camera gameplay evidence.
5. Improve NPC character art and animation beyond runtime/proxy motion.
6. Improve HUD/lobby/death/settings/skin parity.
7. Add stricter manual play and player-camera gates that cannot pass on showcase-only assets.
8. Consider Unity LFS/release-artifact strategy for large GLBs.

## 9. Suggested Next Patch

Recommended first patch:

1. Add sidearm-specific route proof in `TacticalPlayableRouteGate.cs`.
2. Select pistol/sidearm in first-person mode.
3. Fire and reload it during the route gate.
4. Require at least one visible source GLB renderer.
5. Write sidearm event evidence to the route report.
6. Update `RealifiedAssetGameplayPromotionLedger.cs` to count sidearm only if that event evidence exists.
7. Rerun the acceptance pipeline.

Expected result:

```text
realified_asset_gameplay_production_promoted_assets: 2
```

Only if both hero rifle and sidearm have gameplay event evidence.

## 10. How To Review This Repo

Start with:

```text
README.md
docs/PUBLIC_REVIEW_SCOPE.md
docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md
docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json
docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json
reference/html_baseline_final_packet/index.html
```

Then inspect:

```text
Assets/Editor/TacticalPrototypeTools.cs
Assets/Editor/TacticalPlayableRouteGate.cs
Assets/Editor/TacticalGameplayProofGate.cs
Assets/Editor/RealifiedAssetGameplayPromotionLedger.cs
Assets/Scripts/Tactical/TacticalGameManager.cs
Assets/Scripts/Tactical/TacticalPlayerController.cs
Assets/Scripts/Tactical/TacticalEnemy.cs
Assets/Scripts/Tactical/TacticalFirstPersonWeaponVisual.cs
```

Do not assume a report means completion. Read what each report actually proves and what it still marks as false.

## 11. Commands Used For Public Export

Representative commands:

```bash
gh auth status
gh repo create haizhouyuan/unity-tactical-html-parity-review --public --description "Public Unity tactical HTML parity review export" --source . --remote origin
git init
git add .
git commit -m "Public Unity tactical parity review export"
git remote set-url origin https://github.com/haizhouyuan/unity-tactical-html-parity-review.git
git push -u origin main
```

Verification commands used around export:

```bash
find . -maxdepth 2 -type d \( -name Library -o -name Temp -o -name Logs -o -name UserSettings -o -name Obj -o -name Build -o -name Builds \) -print
find . -type f -size +95M -print
rg -n --hidden "<common API key, GitHub token, and password patterns>" .
git status -sb
gh repo view haizhouyuan/unity-tactical-html-parity-review --json nameWithOwner,visibility,url,defaultBranchRef,isPrivate
git ls-remote --heads origin main
```

## 12. Bottom Line

This repository is a useful checkpoint because it finally exposes:

- the Unity implementation;
- the HTML target baseline;
- the current gates;
- the generated/imported assets;
- the failed asset-promotion reality;
- the exact current gap.

It should be reviewed as an unfinished but evidence-bearing Unity tactical-game parity project. The next useful work is not another broad rewrite; it is a sequence of player-camera, gameplay-bound improvements that turn imported assets and systems into actual playable product behavior.

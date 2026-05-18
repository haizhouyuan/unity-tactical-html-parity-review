# M93 Corrected Loot Gameplay Promotion Completion

Date: 2026-05-18

## Summary

M93 now closes the corrected-loot gameplay promotion slice.

This does not make the whole Realified generated batch production-ready. It promotes only the corrected ammo and medkit fallback assets that replaced the mislabeled weapon-shaped loot meshes.

## What Changed

- `TacticalPrototypeTools` now binds:
  - `TacticalLootKind.Ammo` -> `Assets/HtmlTacticalAssets/RealifiedAssets/RS_08_loot_ammo_LOD0.glb`
  - `TacticalLootKind.Medkit` -> `Assets/HtmlTacticalAssets/RealifiedAssets/RS_09_loot_medkit_LOD0.glb`
- `TacticalPlayableRouteGate` now records corrected Realified ammo/medkit route evidence.
- `RealifiedAssetGameplayPromotionLedger` now allows the corrected M93 loot semantic review as the loot-class semantic source.
- `PromotedAssetPlayerCameraVisibilityGate` now reads the gameplay promotion ledger, not only the older class promotion queue.
- `TacticalAcceptancePipeline` now writes the gameplay promotion ledger before running the promoted-asset visibility gate.
- `M84ThreeClassAssetFactorySpikeGate` now uses the corrected Realified medkit as its loot target.

## Evidence

- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`
  - `realified_loot_class_route_evidence=true`
  - `realified_ammo_loot_route_evidence=true`
  - `realified_medkit_loot_route_evidence=true`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
  - `summary.production_promoted_assets=3`
  - promoted: `hero_rifle`, `ammo`, `medkit`
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
  - `passed=true`
  - `summary.production_promoted_classes=2`
  - `summary.visible_promoted_classes=2`
- `docs/M84_THREE_CLASS_ASSET_FACTORY_SPIKE.json`
  - `passed=true`
  - `promoted_asset_count=3`
- `docs/M85_VISUAL_PRODUCTION_PASS.json`
  - `passed=true`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
  - `all_required_current_gates_passed=true`
  - `realified_asset_gameplay_production_promoted_assets=3`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.json`
  - `passed=false`

## Remaining Strict Visual Blockers

M88 still correctly blocks full visual completion on:

- generated batch class promotion for all required classes;
- final weapon art review;
- final humanoid art review.

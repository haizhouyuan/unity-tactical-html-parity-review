# Public Review Scope

This repository is a public export for review, not a finished game release.

## Included

- Unity runtime and editor scripts needed to inspect the tactical prototype.
- Unity scenes, prefabs, materials, screenshots, package manifest, and project settings.
- Current acceptance reports and asset-promotion ledgers.
- The HTML/Three.js final packet used as the behavioral and visual baseline.

## Excluded

- Unity generated cache folders.
- Local logs and user settings.
- Model-download caches and external AI model weights.
- Any private runner configuration outside this Unity project.

## Most Important Files For Review

- `Assets/Scenes/TacticalPrototype.unity`
- `Assets/Editor/TacticalPrototypeTools.cs`
- `Assets/Editor/TacticalPlayableRouteGate.cs`
- `Assets/Editor/TacticalGameplayProofGate.cs`
- `Assets/Editor/HtmlTacticalParityGate.cs`
- `Assets/Editor/RealifiedAssetGameplayPromotionLedger.cs`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
- `docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md`
- `reference/html_baseline_final_packet/index.html`

## Review Standard

An asset should only count as production-promoted when all of the following are true:

1. It is imported into Unity.
2. It is semantically correct for its gameplay class.
3. It is attached to a real gameplay entity.
4. It is visible from the player camera.
5. It participates in at least one gameplay event such as fire, reload, pickup, hit, heal, or enemy attack.

Fixed-camera screenshots, standalone GLB contact sheets, and asset packets alone are not enough.

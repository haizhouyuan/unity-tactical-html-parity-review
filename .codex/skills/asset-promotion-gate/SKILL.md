---
name: asset-promotion-gate
description: Use when deciding whether a generated or imported GLB can move from candidate to approved gameplay asset in this Unity tactical project.
---

# Asset Promotion Gate

An asset counts only if all are true:

1. Imported into Unity.
2. Material/PBR technically validated.
3. Semantically correct for its gameplay class.
4. Bound to a real gameplay entity.
5. Visible from the player camera.
6. Tied to a gameplay event when applicable.
7. Recorded in JSON evidence.

Read:

- `docs/REALIFIED_IMPORT_MATERIAL_GATE.json`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
- `docs/TACTICAL_PLAYABLE_ROUTE_GATE.json`

Do not promote from filename, hash, contact sheet, standalone preview, or showcase scene.

If promotion is blocked, record which of the seven requirements failed.

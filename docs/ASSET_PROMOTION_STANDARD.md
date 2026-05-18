# Asset Promotion Standard

Date: 2026-05-18

This document defines when a generated, imported, or hand-authored asset may count as production progress in the Unity tactical prototype.

The short rule:

```text
An asset is not production-promoted until it is imported, technically ready, semantically correct, gameplay-bound, visible from the actual player camera, tied to a gameplay event, and recorded in the ledger.
```

## Why This Exists

The project has already produced many candidate assets. Some imported successfully, some rendered in contact sheets, and some looked useful at a distance. That is not enough.

False completion happens when an agent says an asset is done because:

- a GLB exists;
- a filename says `container` or `medkit`;
- a contact sheet rendered;
- a hash manifest includes it;
- it appears in a showcase camera;
- Unity imported it with no fatal error.

None of those prove the player can see or use the asset in gameplay.

## Promotion Stages

### Stage 0: Candidate

Definition:

- Asset exists as a generated, downloaded, hand-authored, or procedural candidate.

Required evidence:

- asset path;
- source note;
- intended semantic class;
- license/provenance status if available.

Completion credit:

- none.

### Stage 1: Imported

Definition:

- Unity can import or reference the asset without fatal errors.

Required evidence:

- import report;
- Console status;
- missing-material and missing-texture notes.

Completion credit:

- technical diagnostic only.

### Stage 2: Technical-ready

Definition:

- Scale, orientation, materials, collider/proxy, and performance basics have been reviewed.

Required evidence:

- material/PBR report;
- scale/orientation note;
- collider or interaction proxy status;
- LOD/poly budget status where relevant.

Completion credit:

- technical readiness only.

### Stage 3: Semantic-ready

Definition:

- The asset visually matches its intended class.

Required evidence:

- contact sheet or neutral review screenshot;
- human or VLM semantic review;
- explicit category verdict.

Failure examples:

- a `medkit` that looks like a weapon;
- a `container` that is weapon-shaped;
- a character that is still a capsule/proxy;
- a loot item that can only be identified by filename.

Completion credit:

- can be considered for gameplay binding, but not production-promoted.

### Stage 4: Gameplay-bound

Definition:

- The asset is attached to a real gameplay entity in the tactical route.

Required evidence:

- entity id or prefab binding;
- gameplay system reference;
- report proving it is not only in showcase/contact-sheet/debug roots.

Examples:

- rifle attached to real weapon state;
- medkit attached to pickup logic;
- container attached to cover/collision logic;
- enemy visual attached to NPC combat entity.

Completion credit:

- partial gameplay integration.

### Stage 5: Player-camera-visible

Definition:

- The asset is visible from the actual gameplay player camera, not only from a diagnostic camera.

Required evidence:

- screenshot path;
- gate report linking visible object to gameplay entity;
- screen-space visibility or comparable player-camera proof.

Completion credit:

- visual gameplay proof.

### Stage 6: Gameplay-event-proven

Definition:

- The asset participates in a gameplay event.

Examples:

- weapon fires or reloads;
- loot is picked up;
- cover blocks shots;
- enemy takes damage or attacks;
- ladder/building collider changes route outcome;
- armor/helmet affects state.

Required evidence:

- JSON event record;
- before/after state where applicable;
- player route or gate report.

Completion credit:

- eligible for production promotion.

### Stage 7: Production-promoted

Definition:

- All prior stages pass and the promotion ledger is updated.

Required evidence:

- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`;
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`;
- relevant tactical acceptance report;
- screenshot or route proof.

Completion credit:

- counts toward production visual/gameplay asset progress.

## Required Sidecar Fields

Each promoted or promotion-candidate asset should be traceable with:

```json
{
  "asset_id": "",
  "semantic_class": "",
  "source_type": "generated | hand_authored | imported | procedural | unknown",
  "source_prompt": "",
  "source_references": [],
  "license_status": "unknown | reviewed | safe | blocked",
  "asset_path": "",
  "unity_path": "",
  "material_maps": [],
  "collider_status": "missing | proxy | authored | not_required",
  "lod_status": "missing | reviewed | authored | not_required",
  "import_status": "candidate | imported | failed",
  "technical_status": "not_checked | ready | failed",
  "semantic_status": "not_checked | ready | failed",
  "gameplay_binding_status": "none | bound | failed",
  "player_camera_status": "not_visible | visible | failed",
  "gameplay_event_status": "none | proven | failed",
  "promotion_status": "candidate | quarantined | partial | production_promoted"
}
```

Do not use private credentials or local-only absolute paths in public sidecars.

## Class-specific Promotion Requirements

### Weapon

Must prove:

- first-person visibility;
- third-person or world placement where relevant;
- fire event;
- reload or ammo state event;
- muzzle or hit feedback;
- no completion credit from static close-up only.

### Loot

Must prove:

- visible pickup object;
- interaction prompt or pickup condition;
- inventory/state mutation;
- semantic class is clear from player view.

### Environment prop

Must prove:

- visible in route;
- correct class;
- collision, occlusion, cover, or navigation role if claimed;
- no pure showcase-only placement.

### Character / Enemy

Must prove:

- not capsule/proxy-only if production visual completion is claimed;
- visible from player camera;
- combat or interaction event;
- animation/state feedback appropriate to current phase.

### Building / Structural Asset

Must prove:

- route correctness;
- collider integrity;
- floor/door/ladder/cover behavior;
- player-camera evidence.

## Quarantine Rules

An asset must remain quarantined or partial if:

- semantic class fails;
- material/PBR data is missing for a PBR-required claim;
- it is only visible in a contact sheet;
- it is only visible in a fixed showcase camera;
- it is not attached to a gameplay entity;
- no gameplay event uses it;
- report paths cannot trace it to current Unity project state.

## Ledger Rules

Promotion ledgers must separate:

- candidates;
- imported assets;
- technical-ready assets;
- semantic-ready assets;
- gameplay-bound assets;
- player-camera-visible assets;
- gameplay-event-proven assets;
- production-promoted assets.

Reports should never collapse these stages into one `done` field.

## Done Definition

An asset class is done only when at least one asset in that class has passed all required stages and the relevant gate report confirms it.

For broad visual completion, one promoted asset is not enough. Each required class must have its own player-camera and gameplay-event proof.

# M93 Realified Batch Root Cause And Regeneration Contract

Date: 2026-05-19

## Result

The remaining generated-batch blocker is not a Unity ledger problem. It is an asset-generation/category problem.

The current Realified batch must stay quarantined for non-weapon classes because multiple category contact sheets show weapon-shaped meshes under non-weapon labels:

- `Assets/Screenshots/RealifiedCategorySheets/realified_character_contact_sheet.png`
  - expected: player/enemy tactical humanoids
  - observed: two rifle-like weapon models
- `Assets/Screenshots/RealifiedCategorySheets/realified_loot_contact_sheet.png`
  - expected: ammo and medkit loot
  - observed: two rifle-like weapon models
- existing source trace: `docs/REALIFIED_ASSET_SOURCE_TRACE.json`
  - `source_hash_matches: 0`
  - `lod0_or_primary_failures: 12`
  - `nonweapon_semantic_failures: 8`
  - verdict: do not promote the Unity Realified batch
- existing category review: `docs/REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json`
  - `promotion_allowed: false`

M91 and M92 reduce route and weapon-feel blockers, but they do not change this generated-batch truth.

## Corrected Loot Fallback Update

The first executable M93 slice now replaces the two failed Realified loot stems with deterministic category-correct fallback assets:

- `RS_08_loot_ammo`
  - now: olive ammo crate with visible crate silhouette, handle, latches, and ammo-case markings
  - previous LOD0 SHA256: `fa461ca8bbddcb1723813548dfa8263b563c157c18cef241b09a947ea93daabb`
  - corrected LOD0 SHA256: `7d284020182a12bae34f707dc99418fa806f1a0c661fa37f272c57412f77da5c`
- `RS_09_loot_medkit`
  - now: red medkit case with white-cross markings, straps, buckles, and handle
  - previous LOD0 SHA256: `6e590d95a9baaaeceb74c10cbea212689b4114d2a27284222741067787c1e21c`
  - corrected LOD0 SHA256: `3b2268ed847b57b158d511d879b7f29c6d68c0709887e516b2527f57eaebafc0`

Evidence:

- generator: `tools/m93_generate_corrected_loot_assets.py`
- generation trace: `docs/M93_CORRECTED_LOOT_GENERATION_TRACE.json`
- refreshed Unity contact sheet: `Assets/Screenshots/RealifiedCategorySheets/realified_loot_contact_sheet.png`
- refreshed contact-sheet report: `docs/REALIFIED_CATEGORY_CONTACT_SHEETS.json`
- refreshed import/material gate: `docs/REALIFIED_IMPORT_MATERIAL_GATE.json`
- local Nemotron semantic review: `docs/M93_CORRECTED_LOOT_NEMOTRON_REVIEW.json`

This is intentionally recorded as a corrected fallback slice. It does not claim that the original AI Realified batch is fixed, does not override the older stale failed review for unrelated classes, and does not promote the loot class until gameplay binding, player-camera pickup evidence, and promotion ledger evidence exist.

## What Not To Do

- Do not flip `full_visual_asset_gate_passed`.
- Do not mark `generated_batch_class_promotion_passed=true`.
- Do not promote the mislabeled Realified batch by editing ledgers.
- Do not treat filename categories as semantic truth.
- Do not bind weapon-shaped assets as characters, medkits, helmets, vests, crates, or containers.

## M93 Goal

Regenerate or replace one complete non-weapon class with semantically correct assets, then prove it through the existing promotion chain.

Recommended first class: `loot`.

Reason:

- It is smaller than humanoids.
- It has clear visual targets: ammo box, medkit, first aid, bandage.
- It can be bound to existing pickup gameplay events.
- It should be easier to validate by player-camera screenshot than final humanoid art.

## Required Pipeline

For each M93 asset candidate:

1. Generate or create the asset outside the production path.
2. Place it in quarantine, not directly in gameplay.
3. Record source/prompt/provenance sidecar.
4. Verify category silhouette in a contact sheet.
5. Verify import/material sidecars.
6. Bind only one approved candidate into gameplay.
7. Capture player-camera evidence.
8. Verify pickup/fire/block/etc. event depending on class.
9. Update promotion ledger only from generated evidence.

## Acceptance For One Loot-Class Slice

M93 can pass a loot-class slice when all are true:

- at least one regenerated loot candidate is visibly not weapon-shaped;
- `REALIFIED_CATEGORY_NEMOTRON_REVIEWS` or an equivalent semantic review allows the loot class;
- Unity import/material gate sees the asset and textures;
- player-camera screenshot shows the loot item in the playable route;
- pickup state changes after interacting with the item;
- promotion ledger records the class as production-promoted or explicitly approved-equivalent;
- M88 still remains false unless final weapon art, final humanoid art, and all generated-batch requirements also pass.

## Next Agent Contract

Mission owner should work on the asset factory side, not Unity gameplay first:

- inspect the asset-generation prompts/manifests that produced `RS_08_loot_ammo` and `RS_09_loot_medkit`;
- identify why non-weapon categories became weapon-shaped;
- regenerate two loot assets with category-specific references;
- run Blender/LOD/PBR checks;
- copy only approved loot candidates into Unity;
- run semantic/contact-sheet review before gameplay binding.

If regeneration is blocked, deliver a blocker report with commands, prompts, model/tool versions, generated previews, and the smallest corrected prompt set for the next run.

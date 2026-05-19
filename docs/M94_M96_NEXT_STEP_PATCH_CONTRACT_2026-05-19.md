# M94-M96 Next Step Patch Contract

Date: 2026-05-19

Status: patch-prepared / pending local Codex and Unity verification.

## Purpose

This patch prepares the local ingestion and validation path for ChatGPT Pro M94-M96 reference-image batches after commit `9aefe7d Add strict M94-M96 visual gates`.

The strict gates already exist in the repository. This patch does not duplicate them. It adds:

- a staging script for Pro reference-image bundles;
- a Unity Editor reference-image validation gate;
- M95/M96 review input templates;
- a mission note for the next local Codex handoff.

## Non-Claims

This patch does not claim:

- M94 is passed;
- M95 is passed;
- M96 is passed;
- M88 is passed;
- `full_visual_asset_gate_passed=true`;
- any reference image is production-ready;
- any generated GLB is production-promoted.

## New Local Flow

1. Download the Pro image zip.
2. Stage it into quarantine/reference storage:

```bash
python tools/stage_pro_batch_images_to_quarantine.py   pro_m94_m96_batch_images_first_wave_2026-05-19.zip   --require-first-wave   --report docs/M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION.json
```

3. Open Unity and run:

```text
AI Tools/Run M94 Pro Batch Reference Image Validation
AI Tools/Run M94 Generated Batch Class Promotion Gate
AI Tools/Run M95 Final Weapon Art Review Gate
AI Tools/Run M96 Final Humanoid Art Review Gate
AI Tools/Run M88 Strict Full Visual Asset Gate
```

## Expected Initial Result

- `M94_PRO_BATCH_REFERENCE_IMAGE_VALIDATION` can pass if the image bundle structure and manifest are valid.
- M94 generated batch class promotion should remain false until local source-batch GLBs are generated/imported and gameplay evidence exists per class.
- M95 should remain false until local review input clears final weapon art blockers.
- M96 should remain false until local review input clears final humanoid art blockers.
- M88 should remain false until all strict blockers are genuinely closed.

## Files Added

```text
Assets/Editor/M94ProBatchReferenceImageValidationGate.cs
tools/stage_pro_batch_images_to_quarantine.py
docs/M95_FINAL_WEAPON_ART_REVIEW_INPUT_TEMPLATE_2026-05-19.json
docs/M96_FINAL_HUMANOID_ART_REVIEW_INPUT_TEMPLATE_2026-05-19.json
docs/M94_M96_NEXT_STEP_PATCH_CONTRACT_2026-05-19.md
```

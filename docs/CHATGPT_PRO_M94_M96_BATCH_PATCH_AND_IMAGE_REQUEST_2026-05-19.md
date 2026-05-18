# ChatGPT Pro Request: M94-M96 Patch Plus Batch Reference Images

Date: 2026-05-19

## Purpose

Use ChatGPT Pro as a batch producer for the next visual-production slice. The expected output is not just prompt packs. It should include:

1. A reviewable code/docs patch for the next gates.
2. Batch-generated reference images for the asset factory.
3. Prompt packs and metadata that make those images reproducible and traceable.
4. A manifest that lets Codex import the returned files into quarantine and run local Unity/Blender/Hunyuan validation.

The images are reference inputs only. They must not be claimed as production assets until the local pipeline proves image-to-3D, Blender cleanup, Unity import, gameplay binding, player-camera visibility, and gameplay event evidence.

## Current Ground Truth

The repository is `haizhouyuan/unity-tactical-html-parity-review`.

Current evidence state:

- `all_required_current_gates_passed=true`
- `m84_three_class_asset_factory_spike_passed=true`
- `m85_visual_production_passed=true`
- `promoted_asset_visibility_gate_passed=true`
- `realified_asset_gameplay_production_promoted_assets=3`
- `full_visual_asset_gate_passed=false`

The strict full visual gate still has these blockers:

1. `generated_batch_class_promotion_passed=false`
2. `final_weapon_art_review_passed=false`
3. `final_humanoid_art_review_passed=false`

Read these files first:

- `README.md`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.json`
- `docs/M88_STRICT_FULL_VISUAL_ASSET_GATE.md`
- `docs/ASSET_PROMOTION_STANDARD.md`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
- `docs/M93_CORRECTED_LOOT_GAMEPLAY_PROMOTION_COMPLETION_2026-05-18.md`

## Non-Negotiable Rules

- Do not edit any existing gate result JSON just to flip pass/fail values.
- Do not modify `.unity` scenes directly.
- Do not install packages.
- Do not claim PUBG-like final visual quality.
- Do not claim an image, prompt, or generated GLB is production-ready.
- Do not use real firearm manufacturing, assembly, or operational details. Weapons are fictional appearance-only game props.
- Keep the patch reviewable and scoped.

## Deliverable A: Patch Package

Provide a zip and a standalone unified diff. The patch should prepare the next local implementation slices:

### M94 Generated Batch Class Promotion

Goal: create local gate/editor scaffolding that can evaluate a regenerated batch class-by-class, without promoting the entire batch blindly.

Expected local classes:

- weapon
- humanoid
- gear
- loot
- environment_prop

The gate must distinguish:

- reference image exists
- prompt/metadata exists
- generated GLB exists
- Unity import succeeds
- semantic review passes
- gameplay binding exists
- player-camera visibility exists
- gameplay event evidence exists
- production promotion allowed

### M95 Final Weapon Art Review Gate

Goal: define a stricter weapon art review for first-person and third-person gameplay.

It should check or require evidence for:

- first-person weapon is visible and correctly framed
- third-person/NPC mount looks plausible
- ADS pose does not block too much screen space
- reload pose exists
- muzzle flash / shot feedback exists
- hit marker or impact feedback exists
- material/PBR sidecar exists
- no visible placeholder/procedural block look

### M96 Final Humanoid Art Review Gate

Goal: define a stricter humanoid visual review.

It should check or require evidence for:

- player/enemy are not box/proxy rigs
- tactical silhouette is readable
- helmet/vest/gear are visible and correctly scaled
- at least idle/aim/fire/hit/down states are represented
- player camera can see NPC/enemy in gameplay context
- humanoid asset is tied to a gameplay entity

## Deliverable B: Batch Reference Images

Generate batch images in addition to prompts. The images should be suitable as inputs for Hunyuan3D, Tripo, TRELLIS, Blender reference modeling, or Unity concept review.

### Required Output Shape

Return a zip with this structure:

```text
pro_m94_m96_batch_images_2026-05-19/
  README.md
  manifest.json
  prompts/
    weapon_prompts.md
    humanoid_prompts.md
    gear_prompts.md
    loot_prompts.md
    environment_prop_prompts.md
  contact_sheets/
    weapon_contact_sheet.png
    humanoid_contact_sheet.png
    gear_contact_sheet.png
    loot_contact_sheet.png
    environment_prop_contact_sheet.png
  images/
    weapon/
    humanoid/
    gear/
    loot/
    environment_prop/
```

### Image Naming

Use this pattern:

```text
M94_<class>_<asset_id>_<view>_v01.png
```

Examples:

```text
M94_weapon_hero_rifle_front_v01.png
M94_weapon_hero_rifle_side_v01.png
M94_loot_medkit_three_quarter_v01.png
M94_environment_prop_container_front_v01.png
```

### Minimum First Wave

If generation quota is limited, prioritize this first wave:

| Class | Asset IDs | Required Views |
| --- | --- | --- |
| weapon | `hero_rifle`, `sidearm`, `shotgun`, `dmr` | front, side, three_quarter |
| humanoid | `player_tactical`, `enemy_tactical` | front, back, three_quarter |
| gear | `helmet`, `vest`, `backpack` | front, side, three_quarter |
| loot | `ammo_box`, `medkit`, `armor_pickup` | top, front, three_quarter |
| environment_prop | `shipping_container`, `wood_crate`, `concrete_cover`, `sandbag_stack` | front, side, three_quarter |

This is 16 assets x 3 views = 48 images plus 5 contact sheets. If that is too much for one run, generate at least:

- 1 weapon: `hero_rifle`
- 1 humanoid: `enemy_tactical`
- 1 gear item: `vest`
- 1 loot item: `medkit`
- 1 environment prop: `shipping_container`
- 3 views each, total 15 images plus contact sheets.

### Image Requirements

- PNG preferred.
- 1024x1024 minimum; 1536x1536 or 2048x2048 preferred if available.
- Plain neutral gray or transparent background.
- Orthographic or near-orthographic asset view.
- Neutral studio lighting.
- No dramatic shadows baked into albedo.
- No watermarks.
- No UI text, labels, logos, brand marks, flags, or real-world insignia.
- No dismemberment, gore, horror, or adult content.
- For weapons: fictional tactical game props only, appearance reference only.
- For humanoids: full-body tactical game character reference, readable silhouette, no real-person likeness.
- For loot and props: category must be visually obvious at thumbnail scale.

### Prompt Metadata

Every generated image must have a manifest entry:

```json
{
  "image_file": "images/weapon/M94_weapon_hero_rifle_side_v01.png",
  "class": "weapon",
  "asset_id": "hero_rifle",
  "view": "side",
  "intended_unity_asset_id": "weapon_hero_rifle_final_candidate",
  "prompt": "...",
  "negative_prompt": "...",
  "generator": "ChatGPT image generation",
  "model_or_tool": "if known",
  "seed": "if available",
  "dimensions": "1024x1024",
  "background": "neutral_gray",
  "usage": "reference_image_only",
  "production_status": "quarantine_reference",
  "notes": "Do not count as production until local image-to-3D and Unity gameplay evidence pass."
}
```

## Prompt Pack Requirements

For each class, provide reusable prompts and negative prompts. The prompt pack should include:

- one concise prompt per image;
- one reusable class prompt;
- one negative prompt per class;
- notes for image-to-3D suitability;
- notes for Blender cleanup;
- notes for Unity import and gameplay binding;
- likely failure modes.

## Local Codex Follow-Up After Download

Codex will import the Pro output into a quarantine path such as:

```text
external/pro_outputs/m94_m96_batch_images_2026-05-19/
```

Then Codex will:

1. Verify zip structure and image count.
2. Run a manifest schema check.
3. Create contact sheets if missing.
4. Compare image categories against target classes.
5. Select one asset per class for image-to-3D.
6. Run local Hunyuan/Tripo/TRELLIS/Blender workflow where feasible.
7. Import only validated outputs into Unity.
8. Run local Unity gates before promotion.

## Acceptance For Pro Output

Pro output is acceptable if it includes:

- a patch zip and standalone `.patch`;
- image zip or image artifact bundle;
- `manifest.json`;
- contact sheets;
- prompt packs;
- no secrets;
- no direct edits to existing gate pass/fail JSON;
- clear distinction between reference image, candidate asset, and production-promoted asset.

If all images cannot be generated in one response, provide:

1. the patch;
2. the complete prompt/manifest plan;
3. the first wave of images;
4. a clear continuation request for the next batch.

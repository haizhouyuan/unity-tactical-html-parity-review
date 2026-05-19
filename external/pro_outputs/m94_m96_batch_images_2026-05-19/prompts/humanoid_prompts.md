# humanoid Prompt Pack

Usage: reference_image_only. Production status: quarantine_reference.

## Reusable Class Prompt

full-body tactical game enemy character reference, readable silhouette, helmet, vest, pouches, boots, gloves, neutral pose, no real person likeness, neutral studio lighting, orthographic asset view, neutral gray background

## Reusable Negative Prompt

real person likeness, gore, horror, dismemberment, logos, flags, readable text, watermark, UI labels, exaggerated fantasy armor, weapon aimed at viewer

## Per-Image Prompts

### enemy_tactical / front

Prompt:

full-body tactical game enemy character reference, readable silhouette, helmet, vest, pouches, boots, gloves, neutral pose, no real person likeness, neutral studio lighting, orthographic asset view, neutral gray background, asset_id enemy_tactical, front view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real person likeness, gore, horror, dismemberment, logos, flags, readable text, watermark, UI labels, exaggerated fantasy armor, weapon aimed at viewer

### enemy_tactical / back

Prompt:

full-body tactical game enemy character reference, readable silhouette, helmet, vest, pouches, boots, gloves, neutral pose, no real person likeness, neutral studio lighting, orthographic asset view, neutral gray background, asset_id enemy_tactical, back view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real person likeness, gore, horror, dismemberment, logos, flags, readable text, watermark, UI labels, exaggerated fantasy armor, weapon aimed at viewer

### enemy_tactical / three_quarter

Prompt:

full-body tactical game enemy character reference, readable silhouette, helmet, vest, pouches, boots, gloves, neutral pose, no real person likeness, neutral studio lighting, orthographic asset view, neutral gray background, asset_id enemy_tactical, three_quarter view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real person likeness, gore, horror, dismemberment, logos, flags, readable text, watermark, UI labels, exaggerated fantasy armor, weapon aimed at viewer

## Image-to-3D Suitability Notes

- Use the image as an orthographic or near-orthographic class reference.
- Keep generated 3D outputs in quarantine until semantic review and Unity gameplay binding pass.
- Prefer clean silhouette, correct scale, collider proxy, and PBR sidecar over ornamental detail.

## Blender Cleanup Notes

- Retopologize obvious lumpy geometry.
- Normalize scale and pivot.
- Prepare LOD or a low-poly proxy where needed.
- Create collider-friendly proxy geometry.

## Unity Import And Gameplay Binding Notes

- Import only validated outputs.
- Record source/reference in sidecar metadata.
- Bind to gameplay entity before any promotion credit.
- Capture player-camera and gameplay event evidence.

## Likely Failure Modes

- Wrong semantic class from image-to-3D.
- Overly soft or melted silhouette.
- Missing PBR/material sidecar.
- Showcase-only visibility without gameplay binding.
# environment_prop Prompt Pack

Usage: reference_image_only. Production status: quarantine_reference.

## Reusable Class Prompt

weathered shipping container cover prop reference for tactical game, corrugated metal panels, dents, corner castings, no logos, neutral studio lighting, orthographic asset view, neutral gray background

## Reusable Negative Prompt

logos, shipping company marks, readable text, flags, people, gore, watermark, UI labels, dramatic background

## Per-Image Prompts

### shipping_container / front

Prompt:

weathered shipping container cover prop reference for tactical game, corrugated metal panels, dents, corner castings, no logos, neutral studio lighting, orthographic asset view, neutral gray background, asset_id shipping_container, front view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, shipping company marks, readable text, flags, people, gore, watermark, UI labels, dramatic background

### shipping_container / side

Prompt:

weathered shipping container cover prop reference for tactical game, corrugated metal panels, dents, corner castings, no logos, neutral studio lighting, orthographic asset view, neutral gray background, asset_id shipping_container, side view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, shipping company marks, readable text, flags, people, gore, watermark, UI labels, dramatic background

### shipping_container / three_quarter

Prompt:

weathered shipping container cover prop reference for tactical game, corrugated metal panels, dents, corner castings, no logos, neutral studio lighting, orthographic asset view, neutral gray background, asset_id shipping_container, three_quarter view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, shipping company marks, readable text, flags, people, gore, watermark, UI labels, dramatic background

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
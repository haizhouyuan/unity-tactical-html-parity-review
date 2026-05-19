# loot Prompt Pack

Usage: reference_image_only. Production status: quarantine_reference.

## Reusable Class Prompt

game medkit loot pickup reference, hard plastic case, obvious medical/utility shape without logos, clear thumbnail readability, neutral studio lighting, orthographic asset view, neutral gray background

## Reusable Negative Prompt

logos, readable text, red cross logo, medical brand marks, gore, pills, adult content, watermark, UI labels, dramatic background

## Per-Image Prompts

### medkit / top

Prompt:

game medkit loot pickup reference, hard plastic case, obvious medical/utility shape without logos, clear thumbnail readability, neutral studio lighting, orthographic asset view, neutral gray background, asset_id medkit, top view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, readable text, red cross logo, medical brand marks, gore, pills, adult content, watermark, UI labels, dramatic background

### medkit / front

Prompt:

game medkit loot pickup reference, hard plastic case, obvious medical/utility shape without logos, clear thumbnail readability, neutral studio lighting, orthographic asset view, neutral gray background, asset_id medkit, front view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, readable text, red cross logo, medical brand marks, gore, pills, adult content, watermark, UI labels, dramatic background

### medkit / three_quarter

Prompt:

game medkit loot pickup reference, hard plastic case, obvious medical/utility shape without logos, clear thumbnail readability, neutral studio lighting, orthographic asset view, neutral gray background, asset_id medkit, three_quarter view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

logos, readable text, red cross logo, medical brand marks, gore, pills, adult content, watermark, UI labels, dramatic background

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
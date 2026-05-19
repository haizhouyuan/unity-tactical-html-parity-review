# weapon Prompt Pack

Usage: reference_image_only. Production status: quarantine_reference.

## Reusable Class Prompt

fictional tactical game hero rifle appearance reference only, compact modern silhouette, unified receiver, stylized barrel shroud, magazine, rails, neutral studio lighting, orthographic asset view, neutral gray background

## Reusable Negative Prompt

real brand markings, logos, readable text, disassembly instructions, internal mechanism diagram, gore, hands, human figure, watermark, UI labels, dramatic background, manufacturing blueprint

## Per-Image Prompts

### hero_rifle / front

Prompt:

fictional tactical game hero rifle appearance reference only, compact modern silhouette, unified receiver, stylized barrel shroud, magazine, rails, neutral studio lighting, orthographic asset view, neutral gray background, asset_id hero_rifle, front view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real brand markings, logos, readable text, disassembly instructions, internal mechanism diagram, gore, hands, human figure, watermark, UI labels, dramatic background, manufacturing blueprint

### hero_rifle / side

Prompt:

fictional tactical game hero rifle appearance reference only, compact modern silhouette, unified receiver, stylized barrel shroud, magazine, rails, neutral studio lighting, orthographic asset view, neutral gray background, asset_id hero_rifle, side view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real brand markings, logos, readable text, disassembly instructions, internal mechanism diagram, gore, hands, human figure, watermark, UI labels, dramatic background, manufacturing blueprint

### hero_rifle / three_quarter

Prompt:

fictional tactical game hero rifle appearance reference only, compact modern silhouette, unified receiver, stylized barrel shroud, magazine, rails, neutral studio lighting, orthographic asset view, neutral gray background, asset_id hero_rifle, three_quarter view, 1024 square PNG, clean silhouette, production concept reference for game asset factory

Negative prompt:

real brand markings, logos, readable text, disassembly instructions, internal mechanism diagram, gore, hands, human figure, watermark, UI labels, dramatic background, manufacturing blueprint

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
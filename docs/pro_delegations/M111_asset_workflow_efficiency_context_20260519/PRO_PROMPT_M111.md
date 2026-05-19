# M111 Request To ChatGPT PRO: Make The Asset/Unity Workflow Higher-Value And Less Wasteful

You are advising a local Codex agent that can run shell, Python, Blender headless, HomePC dual RTX 3090 Hunyuan3D jobs, Unity 6.4 through CoplayDev/community Unity MCP, and local verification screenshots. You cannot run the user's Unity session yourself, but you can inspect this GitHub context bundle and generate patch packages / scripts / workflow recommendations.

## User's Core Complaint

The current workflow is too slow and too low-value. Codex keeps fixing small Unity/Blender/MCP issues locally, but the visible game result remains far from PUBG-like realism. The user wants Codex to do less low-level problem solving and use PRO more as a senior advisor / bulk patch generator / toolchain researcher.

Please be critical. If this workflow is wrong, say so. If there are better known tools, methods, or asset pipelines, identify them and explain exactly how Codex should adopt or test them.

## Current Ground Truth

- Official Unity MCP is abandoned for now. Main bridge is CoplayDev/community Unity MCP at `http://127.0.0.1:8080/mcp`.
- Unity active project is local: `/Users/yuanshaochen/My project`.
- Public review repo is `unity-tactical-html-parity-review`.
- Asset factory repo is local: `ai-game-generation-research`.
- HomePC dual RTX 3090 is available and authorized for Hunyuan3D / Blender / ComfyUI style GPU jobs.
- Codex has already run Hunyuan3D shape+paint for three clean-background references:
  - hero rifle side view,
  - container side/module,
  - checkpoint booth.
- Codex has Blender-previewed the assets and produced LOD GLBs.
- Codex has tried binding the M110 hero rifle into Unity first-person/third-person via existing M97 tools. The JSON proof can pass, but the screenshot still looks unacceptable and too placeholder-like.

## Evidence In This Bundle

### Models

Under `models/`:

- `hero_rifle_hunyuan_lod1_60000.glb`
- `container_side_hunyuan_lod1_80000.glb`
- `checkpoint_booth_hunyuan_lod1_150000.glb`

These are reduced versions intended for review / Unity tests / GitHub sharing, not final production art.

### Previews

Under `previews/`:

- raw Blender previews for rifle, container, checkpoint booth,
- LOD Blender previews for container and checkpoint booth.

Important observation: raw previews look better; naive Blender decimation causes visible faceting and surface damage on environment assets. Please advise a better optimization path than naive decimate if one exists.

### Unity Screenshots

Under `screenshots/`:

- `unity_first_person_idle_after_m110_bind.png`
- `unity_first_person_fire_after_m110_bind.png`

These show the core failure: even with a better generated rifle asset, Unity viewmodel binding / materials / lighting / camera composition still make the result look primitive. The JSON proof is not enough.

### Reports

Under `reports/`:

- Blender preview reports with triangle counts and texture info.
- LOD reports.
- Unity M97 visual proof.
- Kimi asset quality review.

Key numbers:

- Rifle raw: 208,356 triangles; LOD: 59,999 triangles; LOD GLB about 4.5MB.
- Container raw: 965,824 triangles; LOD: 80,000 triangles; LOD GLB about 5.4MB.
- Checkpoint booth raw: 2,581,254 triangles; LOD: 150,000 triangles; LOD GLB about 9.8MB.
- Raw booth GLB is about 87MB and raw OBJ is about 205MB, so raw assets should stay local/HomePC unless explicitly needed.

## What I Need From You

Please answer in two layers.

### Layer 1: Advisory / Strategy

Give a direct, efficiency-first diagnosis:

1. Is it a good idea to put selected 3D assets on GitHub so PRO can inspect and assemble them into Unity patches?
2. Which assets should go to GitHub, and which should stay local / Git LFS / release artifact / not shared?
3. Is the current image-to-3D + naive Blender decimate + Unity bind approach wrong or incomplete?
4. What are the better known workflows/tools for:
   - Hunyuan3D/TRELLIS/Tripo/Meshy style asset generation,
   - retopo / mesh cleanup,
   - UV/material preservation,
   - Unity import and prefab binding,
   - first-person weapon viewmodel composition,
   - environment prop optimization,
   - humanoid/tactical enemy generation.
5. What should Codex stop doing immediately because it wastes time?
6. What should Codex do locally vs what should PRO generate?

Please be very concrete. Avoid more governance. The user wants visible game improvement.

### Layer 2: Batch Patch Request

If you can, generate a downloadable patch package that improves the workflow and/or Unity integration. Preferred patch scope:

1. A Unity Editor tool that imports these three LOD GLBs into a dedicated `Assets/Generated/Quarantine/M111/` area and creates usable prefabs/material bindings without editing scene YAML manually.
2. A Unity Editor tool/menu:
   - `AI Tools/Install M111 Visible Asset Slice`
   - `AI Tools/Capture M111 Visible Asset Slice Proof`
3. The tool should place:
   - rifle as first-person test viewmodel with configurable per-asset orientation/scale/material preservation,
   - container as cover prop,
   - checkpoint booth as a visible facade or combat slice prop.
4. It should output:
   - `docs/M111_VISIBLE_ASSET_SLICE_PROOF.json`,
   - screenshots under `Assets/Screenshots/M111VisibleAssetSlice/`.
5. It must not directly mark M88/M95/M96 final gates as passed.
6. It must not fake promotion ledger values.
7. It must not install new Unity packages.
8. It must not edit large `.unity` YAML by hand.

If the existing M97/M108 code is the wrong foundation, say so and provide a cleaner replacement strategy.

## Desired Artifacts

Please provide:

- `m111_visible_asset_slice_patch.zip`
- `m111_visible_asset_slice.patch`
- `README_FOR_PATCH.md`
- `changed_files_manifest.json`
- `validation_notes.md`
- If patch generation is not possible, provide a precise implementation plan and code snippets for the files Codex should edit.

## Hard Constraint

Do not assume Unity verification. Codex will apply your patch locally, run community Unity MCP, read Console, run the menus, and inspect screenshots. Your output should help Codex move faster and avoid repeating low-value local trial-and-error.

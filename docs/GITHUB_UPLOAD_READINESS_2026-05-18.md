# GitHub Upload Readiness for Pro Review

Date: 2026-05-18

## Current local repository state

Project root:

- `/Users/yuanshaochen/My project`

Git state:

- Git repository exists.
- Current branch: `main`.
- Current local commit: `18132c0 Initial Unity AI-assisted project setup`.
- No Git remote is configured yet.
- Only 76 files are currently tracked.
- Most Unity implementation files, scenes, assets, screenshots, and reports are still untracked.

## Ignore policy

Existing `.gitignore` already excludes the most important Unity generated folders:

- `Library/`
- `Temp/`
- `Obj/`
- `Build/`
- `Builds/`
- `Logs/`
- `UserSettings/`
- `MemoryCaptures/`

This is correct. Do not upload Unity generated cache folders.

Keep `.meta` files for committed Unity assets. Do not delete or ignore asset `.meta` files.

## Size risk

Current measured sizes:

- `Assets/HtmlTacticalAssets`: about `626M`
- `Assets/Screenshots`: about `90M`
- `docs`: about `1.0M`
- `Assets/Scripts`: about `252K`
- `Assets/Editor`: about `488K`

Large non-generated file found:

- `Assets/HtmlTacticalAssets/ApprovedAssets/medical_loot_v1.glb`

Asset count:

- `Assets/HtmlTacticalAssets` contains about 303 image/model files.

## Recommended upload strategy

Use a private GitHub repository first.

Commit source and review evidence:

- `Assets/Scripts/**`
- `Assets/Editor/**`
- `Assets/Scenes/TacticalPrototype.unity`
- needed `.meta` files
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `ProjectSettings/EditorBuildSettings.asset`
- `ProjectSettings/ProjectSettings.asset`
- `docs/**`
- selected screenshots that prove manual gameplay

For assets, choose one of two strategies:

### Strategy A: Full private repo with Git LFS

Use this if ChatGPT Pro needs to inspect actual models/textures through GitHub.

Track large binary assets with Git LFS:

- `*.glb`
- `*.fbx`
- `*.png`
- `*.jpg`
- `*.jpeg`
- `*.tga`
- `*.exr`
- `*.psd`

Pros:

- Pro/reviewers can see the full asset tree.
- Easier to reproduce the project.

Cons:

- More upload time.
- LFS quota and repo size need attention.

### Strategy B: Source-first review branch

Use this if the first Pro pass is mostly architecture/code/gate review.

Commit only:

- code
- scenes
- docs
- asset registry reports
- a small curated subset of GLBs/screenshots

Pros:

- Fast.
- Less GitHub/LFS friction.
- Good enough for code review and acceptance-gate design.

Cons:

- Pro cannot fully validate model import quality.

## Recommended first Pro pass

Use Strategy B first.

Reason:

- The most urgent problem is not that Pro needs all 626MB of assets.
- The urgent problem is that gameplay integration, gates, asset promotion, and first-person weapon presentation are not good enough.
- A source-first review can catch architecture and acceptance problems quickly.

Then create a second asset-focused pass if needed, with Git LFS or a separate release artifact.

## Do not upload before confirming

Do not create a public repo without explicit confirmation.

Before pushing, confirm:

1. Repository name.
2. Private or public.
3. Whether to use Git LFS.
4. Whether to include the full `Assets/HtmlTacticalAssets` directory.
5. Whether screenshots under `Assets/Screenshots` are safe to upload.

## Suggested GitHub branch / PR title

Branch:

- `review/unity-html-tactical-parity-2026-05-18`

Draft PR title:

- `Review: Unity HTML tactical parity and asset/gameplay binding`

Draft PR description should link to:

- `docs/CHATGPT_PRO_UNITY_HTML_PARITY_REVIEW_REQUEST_2026-05-18.md`
- `docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- `docs/REALIFIED_ASSET_CLASS_PROMOTION_QUEUE.md`

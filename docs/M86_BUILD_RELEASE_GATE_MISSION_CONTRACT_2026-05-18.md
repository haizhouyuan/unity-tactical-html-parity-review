# M86 Build / Release Gate Mission Contract

Date: 2026-05-18

## Mission

Produce a reviewable local build/release packet for the Unity tactical prototype without claiming final PUBG-like visual completion.

## Lane

Release / LiveOps.

## Scope

- Build the current `Assets/Scenes/TacticalPrototype.unity` scene for macOS Standalone.
- Preserve the latest gameplay, M84, and M85 gate truth.
- Run a built-player launch smoke test outside the Unity Editor.
- Record the build artifact path, size, executable path, launch log, and release limitations.

## Out Of Scope

- Do not change gameplay behavior.
- Do not change Unity packages.
- Do not promote additional generated assets.
- Do not flip `full_visual_asset_gate_passed`.
- Do not commit the generated app bundle under `Builds/`.

## Required Evidence

- `docs/M86_BUILD_RELEASE_GATE.json`
- `docs/M86_BUILD_RELEASE_GATE.md`
- `docs/M86_LAUNCH_SMOKE.json`
- `docs/M86_LAUNCH_SMOKE.log`
- `docs/M86_LAUNCH_VISUAL_SMOKE.json`
- `docs/M86_LAUNCH_VISUAL_SMOKE.log`
- `Assets/Screenshots/M86BuildRelease/01_build_launch_smoke_screen.png`

## Definition Of Done

- Unity build result is `Succeeded`.
- Build reports `0` errors.
- App bundle exists locally under ignored `Builds/M86/`.
- Built executable starts and survives a smoke interval.
- Release notes disclose build warnings and remaining visual gate blockers.
- The public repo commits reports and evidence only, not the generated app bundle.

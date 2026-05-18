# M86 Release Notes

Date: 2026-05-18

## Review Build

This release packet adds a local macOS Standalone build gate for the Unity tactical prototype.

Build output:

```text
/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app
```

The generated app bundle is local-only and ignored by Git. Reviewers should use the committed reports to inspect the build result and ask the maintainer for the local app bundle if they need to run the exact generated player.

## Included Evidence

- `docs/M86_BUILD_RELEASE_GATE.json`
- `docs/M86_BUILD_RELEASE_GATE.md`
- `docs/M86_LAUNCH_SMOKE.json`
- `docs/M86_LAUNCH_SMOKE.log`
- `docs/M86_LAUNCH_VISUAL_SMOKE.json`
- `docs/M86_LAUNCH_VISUAL_SMOKE.log`
- `Assets/Screenshots/M86BuildRelease/01_build_launch_smoke_screen.png`

## Build Status

- Build target: `StandaloneOSX`
- Build result: `Succeeded`
- Build errors: `0`
- Build warnings: `345`
- App directory bytes: `501932730`
- Launch smoke: passed

The build warnings are retained as a release risk rather than hidden. They were non-fatal for this local build, but they should be reviewed before treating the packet as a polished release candidate.

## Honest Quality Status

This is not a finished PUBG-like production build.

The current packet is a reviewable tactical prototype build whose editor gates were already passing through M85 and whose local build now launches. The full visual asset gate remains open because final humanoid quality, generated asset-class promotion, and stronger player-camera production review are still incomplete.

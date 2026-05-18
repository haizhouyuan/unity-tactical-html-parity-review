# M86 Build / Release Gate Completion

Date: 2026-05-18

## Result

M86 produced a local macOS Standalone review build and launch smoke evidence.

`docs/M86_BUILD_RELEASE_GATE.json` reports:

- `passed: true`
- `build_result: Succeeded`
- `build_total_errors: 0`
- `build_total_warnings: 345`
- `app_bundle_exists: true`
- `app_executable_exists: true`
- `app_file_count: 321`
- `app_directory_size_bytes: 501932730`
- `launch_smoke_tested: true`
- `launch_smoke_passed: true`
- `visual_launch_smoke_passed: true`

## Build Artifact

The local build was written to:

```text
/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app
```

The executable path is:

```text
/Users/yuanshaochen/My project/Builds/M86/TacticalPrototypeM86.app/Contents/MacOS/My project
```

The app bundle is intentionally not committed because `Builds/` is ignored. This repo commits the build gate report, launch smoke report, logs, and screenshot evidence instead.

## Launch Smoke

Headless launch smoke:

- report: `docs/M86_LAUNCH_SMOKE.json`
- log: `docs/M86_LAUNCH_SMOKE.log`
- `process_started: true`
- `survived_seconds: 18`
- `log_contains_unity: true`
- `log_contains_crash: false`
- `passed: true`

Visual launch smoke:

- report: `docs/M86_LAUNCH_VISUAL_SMOKE.json`
- log: `docs/M86_LAUNCH_VISUAL_SMOKE.log`
- screenshot: `Assets/Screenshots/M86BuildRelease/01_build_launch_smoke_screen.png`
- `process_started: true`
- `screenshot_exists: true`
- `passed: true`

The visual screenshot is only window/process evidence. The macOS screenshot includes desktop foreground content, so it must not be counted as gameplay-quality visual evidence.

## Remaining Blockers

M86 does not resolve the larger production-visual blockers:

- `full_visual_asset_gate_passed` remains false.
- Class-level promoted asset visibility still needs stronger production evidence.
- The local build still needs a human or AI-playtester route through visible weapon, pickup, NPC combat, reload, building traversal, and restart in the built player.

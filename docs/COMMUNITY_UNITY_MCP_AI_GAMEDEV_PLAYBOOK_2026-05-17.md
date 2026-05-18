# Community Unity MCP / AI Game Dev Playbook

Date: 2026-05-17

## Decision This Supports

Use community and official practice to make the Unity tactical remake faster and less fragile.

The immediate decision is not "which MCP is coolest." The decision is:

1. Which Unity control path should Codex/Kimi/Gemini use day to day?
2. Which work should be done by Unity MCP, and which should be done by deterministic Editor tools?
3. How do generated assets move from local AI output into playable Unity gameplay without fake completion?

## Current Local Verdict

Use this stack now:

```text
Codex/Kimi/Gemini mission packet
  -> code/file edits
  -> Unity Editor menu command as the stable high-level API
  -> CoplayDev MCP execute_menu_item/read_console for short editor operations
  -> route gate JSON + screenshots/contact sheets
  -> Nemotron semantic review
  -> promote or quarantine assets
```

Do not make MCP the whole brain. MCP should run short, explicit Unity operations and return state. Asset quality and game design judgment should come from deterministic reports, screenshots, and VLM/human review.

## Sources Checked

| Source | Tier | What It Changes For This Project |
|---|---|---|
| Unity Support: Unity AI MCP capacity-limit / plan or seat entitlement error | Official | Confirms official Unity MCP being blocked by plan/seat is not a relay-path bug. Keep official MCP as blocked until entitlement is fixed. |
| CoplayDev `unity-mcp` GitHub | Primary project source | Confirms the currently installed community MCP is the active route and exposes useful tools such as menu execution, console reading, scene/asset/material/script management, tests, and code execution. |
| CoderGamester `mcp-unity` GitHub | Primary project source | Good backup lane: Unity-side WebSocket bridge plus Node MCP server. Probe only in a copy/worktree if Coplay becomes unreliable. |
| Unity `MenuItem` Scripting API | Official | Supports the chosen pattern of turning complex Unity operations into stable `AI Tools/...` commands. |
| Unity `AssetDatabase.Refresh` Scripting API | Official | Supports the rule that refresh/import is a controlled gate step, not something to spam during compile/reload churn. |
| Unity assembly definition manual | Official | Supports the next efficiency lane: split runtime/editor/import validation assemblies to reduce reload cost. |
| Unity command-line and Test Framework docs | Official | Supports batchmode/test validation only when the interactive Editor is not holding the same project open. |

Links:

- https://support.unity.com/hc/en-us/articles/48958235901460-Unity-AI-MCP-Connection-Fails-Unity-AI-Gateway-connection-Error
- https://github.com/CoplayDev/unity-mcp
- https://github.com/CoderGamester/mcp-unity
- https://docs.unity3d.com/ScriptReference/MenuItem.html
- https://docs.unity3d.com/ScriptReference/AssetDatabase.Refresh.html
- https://docs.unity3d.com/Manual/assembly-definition-files.html
- https://docs.unity3d.com/Manual/CommandLineArguments.html
- https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/reference-command-line.html

## What We Should Copy From Community Practice

### 1. Treat Unity as a stateful editor, not a normal CLI app

The recurring trap is trying to drive Unity like a stateless command line program. In practice:

- script compilation and asset import can still be running while MCP calls answer;
- new menu items can be stale until assembly reload finishes;
- Play Mode gates can run old code if reload is locked.

Working rule:

```text
stop play mode
wait until not compiling/updating
clear console
refresh only when needed
run one deterministic menu/gate command
read console and JSON report
```

### 2. Make project-specific menu commands the real automation API

Good commands for this project:

- `AI Tools/Start Community Unity MCP Server`
- `AI Tools/Run Tactical Acceptance Pipeline`
- `AI Tools/Render GLB Contact Sheet`
- `AI Tools/Validate Realified Assets`
- `AI Tools/Promote Approved Asset To Gameplay`
- `AI Tools/Write Tactical Project Report`

MCP then only has to call `execute_menu_item` or read console output. This is much more reliable than many small GameObject edits through a long MCP session.

### 3. Keep asset generation separate from asset promotion

Kimi's chain is valuable:

```text
Reference Image -> Hunyuan3D Shape/Tex -> Blender LOD -> Unity GLB import
```

But generated files are still only candidates. Promotion requires:

1. GLB import success.
2. Material/texture technical audit.
3. Contact sheet screenshot.
4. Nemotron or stronger VLM semantic category pass.
5. Gameplay binding to a real entity.
6. Player-camera route evidence.
7. Route gate JSON proving it counted for the correct class.

The current Realified batch fails step 4 and the technical PBR audit, so it stays quarantined.

### 4. Use local VLMs as quality gates, not as magic asset generators

The new local split is right:

- Nemotron: observe/review/extract/quality judge.
- UI-TARS: bounded GUI actions when no structured tool exists.
- Unity MCP: short structured editor actions.
- Codex/Kimi/Gemini: code, mission planning, integration, review.

This is a good factory shape. The missing part was making quality gates mandatory before gameplay promotion; that has now started with the Realified contact-sheet gate.

## What To Avoid

1. Do not install multiple Unity MCP bridges into the active project at once.
2. Do not keep rerunning MCP calls while Unity is compiling or reload-locked.
3. Do not let file names determine asset category.
4. Do not count fixed showcase/contact-sheet rendering as gameplay integration.
5. Do not mark Hunyuan/TRELLIS/Comfy output production-ready until it passes class-specific Unity player-camera evidence.
6. Do not keep broadening the game before the current visual gate is honest.

## Recommended Next Lanes

### Lane A: MCP Hardening

Owner: Codex/Kimi.

Deliverable:

- one shell or Python smoke script that performs MCP initialize, tools/list, read_console, and `AI Tools/Run Tactical Acceptance Pipeline`;
- writes command log and pass/fail JSON.

Why:

This stops us from arguing whether MCP is "working" by feeling. It becomes a one-command check.

Status:

- Implemented as `/Users/yuanshaochen/Projects/ai-game-generation-research/tools/unity_mcp_smoke.py`.
- Latest report: `docs/UNITY_MCP_SMOKE_REPORT.json`.
- Result: MCP initialized, 43 tools listed, `read_console` worked, `execute_menu_item` ran `AI Tools/Run Tactical Acceptance Pipeline`.
- Acceptance status: `complete`.
- `all_required_current_gates_passed=true`.
- `full_visual_asset_gate_passed=false`.

### Lane B: Asset Promotion Gate

Owner: Codex + Nemotron.

Deliverable:

- `tools/nemotron_contact_sheet_review.py`
- `docs/REALIFIED_ASSETS_NEMOTRON_CONTACT_SHEET_REVIEW.json`
- `docs/REALIFIED_ASSETS_NEMOTRON_CONTACT_SHEET_REVIEW.md`

Status:

- Implemented.
- Current verdict: fail.
- Promotion allowed: false.

### Lane C: Character/Enemy Replacement

Owner: Kimi/HomePC for asset, Codex for Unity binding, Gemini for visual critique.

Deliverable:

- one category-correct enemy or player visual;
- visible non-capsule gameplay-bound model;
- route gate field that fails if character remains proxy-only.

Why:

This is the biggest visible gap against the HTML/PUBG-like target.

### Lane D: Assembly Definition Split

Owner: Codex/Kimi.

Deliverable:

- `TacticalRuntime.asmdef`
- `TacticalEditor.asmdef`
- compile clean;
- acceptance pipeline still passes.

Why:

This is the most likely efficiency improvement for Unity reload pain.

### Lane E: Community Probe Backup

Owner: Gemini/Kimi read-only first.

Deliverable:

- CoderGamester MCP probe in a throwaway copy/worktree only;
- compare setup complexity, tool surface, reliability, and console/read/write behavior against Coplay.

Decision:

Do not install into the active project unless Coplay MCP becomes persistently blocked.

## Bottom Line

The efficient route is not "more MCP everywhere." It is:

```text
fewer Unity round trips
+ stronger Editor menu commands
+ stricter contact-sheet and player-camera gates
+ one active MCP bridge
+ mission-level agent work
```

That matches both the community tools and the local failure evidence.

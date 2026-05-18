# Community Unity MCP Codex Skill Plan

Date: 2026-05-18

This repo currently uses the community Unity MCP bridge rather than relying on official Unity MCP. That deserves a repo-local Codex skill because the bridge's tool names, resources, connection lifecycle, and failure modes differ from official Unity MCP.

## Decision

Add a focused adapter skill:

```text
.codex/skills/community-unity-mcp-adapter/
```

This skill should not replace existing skills. It complements:

```text
.codex/skills/unity-mcp-gate-runner/
.codex/skills/unity-compile-recovery/
.codex/skills/game-feel-tuning/
.codex/skills/asset-promotion-gate/
.codex/skills/player-camera-visual-proof/
.codex/skills/html-parity-review/
```

## Why This Is Needed

The existing `unity-mcp-gate-runner` skill states the correct high-level loop, but it does not explicitly teach Codex the current community MCP tool contract.

Without this adapter, future agents may accidentally assume official Unity MCP behavior, official tool names, or official bridge approval flow.

## What The Skill Must Enforce

- MCP is transport, not the brain.
- Community MCP resources should be read before tools.
- Unity compile/update/domain reload state must be checked.
- One deterministic `AI Tools/...` menu command per verification step.
- Console must be read before and after command execution.
- JSON report paths must be named.
- Screenshots must be captured for visual/player-camera missions.
- Missing/disabled menu items must be reported as active-session failure.
- No broad scene authoring through MCP.
- No second MCP bridge installation in the active project.

## Skill Discovery Note

The repo currently has project-local skills under `.codex/skills/`.

Codex documentation also describes repo-scoped skills under `.agents/skills`. If a local Codex session does not discover `.codex/skills`, run a skill discovery verification mission before moving or mirroring skills.

Do not create duplicate skills in both locations until the active Codex environment proves which path it loads.

## Recommended Next Mission

```text
M0.6 Community Unity MCP Skill Verification
```

Steps:

1. Confirm whether Codex discovers `.codex/skills/community-unity-mcp-adapter`.
2. If not, decide whether to mirror skills into `.agents/skills` or configure Codex to load the current path.
3. Open Unity.
4. Wait for compile/update idle.
5. Through community MCP:
   - read editor state,
   - read Console,
   - execute `AI Tools/Run Tactical Preflight`,
   - read Console,
   - read `docs/TACTICAL_PREFLIGHT_REPORT.json`.
6. Write a report distinguishing:
   - repo skill discovery,
   - Unity editor readiness,
   - external MCP transport proof,
   - remaining risks.

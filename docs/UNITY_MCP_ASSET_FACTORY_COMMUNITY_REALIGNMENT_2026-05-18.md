# Unity MCP / Asset Factory Community Realignment

Date: 2026-05-18

## Current Decision

Use this working loop:

```text
Codex/Kimi/Gemini mission packet
  -> code or Editor-tool change
  -> Unity AI Tools menu command
  -> CoplayDev community MCP execute/readback
  -> JSON + screenshot evidence
  -> Nemotron/UI-TARS visual or GUI assist only where needed
  -> promote or quarantine one asset class
```

Do not treat MCP as the whole brain. MCP should execute short Unity operations and read state. Long work belongs in Unity Editor scripts, external generation scripts, and mission-level agents.

## What Is Actually Verified

- Kimi's local visual stack is alive: Nemotron 8083, UI-TARS 8082, and MCP self-test passed.
- Community Unity MCP is the active working editor-control route.
- The current tactical game route/parity gates pass.
- The Realified generated batch exists in Unity.

## What Is Not Complete

- `full_visual_asset_gate_passed` is still false.
- The Realified batch is not globally promotable.
- Existing source-trace says the Unity Realified GLBs do not hash-match expected research asset packet outputs.
- Semantic review still blocks promotion for generated non-weapon classes.
- Candidate files on disk are not the same as production gameplay assets.

## Adopt

- CoplayDev `unity-mcp` as the active Unity bridge.
- Unity `AI Tools/...` menu commands as the stable automation API.
- Nemotron as visual quality gate.
- UI-TARS as bounded GUI fallback.
- Class-by-class asset promotion.

## Defer

- Official Unity MCP, until Unity AI plan/seat entitlement is fixed.
- Assembly-definition optimization, until current asset/gameplay gates stabilize.

## Probe Only In Isolation

- CoderGamester `mcp-unity` or another Unity MCP bridge. Probe only in a copied project, not this active project.

## Reject For Now

- Installing multiple Unity MCP bridges into this active project.
- Promoting the whole Realified batch at once.
- Calling generated GLBs production-ready before source, semantic, material, gameplay-binding, and player-camera evidence pass.

## Next Missions

1. One-class Realified salvage, starting with the class most likely to pass.
2. Real character asset lane: rigged/skinned or explicitly marked static-only.
3. Generator quality loop: generate -> preview -> Nemotron -> pass/copy or quarantine.
4. MCP reliability runbook v2.
5. Optional backup MCP probe in an isolated project copy.

Canonical mission output:

`/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M82_community_mcp_asset_factory_realignment/`

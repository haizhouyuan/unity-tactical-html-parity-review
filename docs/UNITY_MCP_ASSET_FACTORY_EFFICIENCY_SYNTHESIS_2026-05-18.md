# Unity MCP / Asset Factory Efficiency Synthesis

Date: 2026-05-18

## Conclusion

The low-efficiency feeling is valid. The fix is not more MCPs in the active Unity project. The current efficient path is:

```text
Codex/Kimi/Gemini mission packet
  -> deterministic Unity AI Tools menu command
  -> CoplayDev community MCP execute/readback
  -> JSON + screenshot evidence
  -> Nemotron/UI-TARS semantic or GUI assist only when useful
  -> promote or quarantine one asset class
```

MCP should execute short operations and read state. Unity Editor scripts should own long deterministic operations such as scene generation, asset import validation, promotion queues, screenshots, and acceptance gates.

## What Was Rechecked

- `~/Projects/local-coding-runners/HANDOFF.md`
- `quick-verify.sh`
- M69 Kimi handoff verification
- M88 first-person weapon refinement result
- Kimi sidecar review through `runner-mcp-review kimi`
- Current Unity reports:
  - `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
  - `TACTICAL_PLAYABLE_ROUTE_GATE.json`

## Verified

- Nemotron observe service on `8083`: OK.
- UI-TARS service on `8082`: OK.
- MCP self-test: OK.
- Kimi sidecar can run read-only review and returned useful recommendations.
- CoplayDev community MCP is still the active practical Unity control bridge.

## Still False

- `full_visual_asset_gate_passed=false`.
- Generated Realified assets are not globally production-promoted.
- Non-weapon classes still need semantic, material, gameplay-binding, and player-camera gates.
- The first-person weapon is improved but not final hero art.

## Adopt

- Keep CoplayDev `unity-mcp`.
- Keep project-specific Unity `AI Tools/...` menu commands.
- Use MCP for `execute_menu_item`, console/state readback, and short verification.
- Use class-by-class promotion instead of global asset promotion.
- Use Nemotron as a fast local visual review layer.

## Probe

- CoderGamester `mcp-unity` only in a copied project if CoplayDev becomes unreliable.
- Unity batchmode tests only when the editor is closed or in a separate project copy.
- Hunyuan3D/ComfyUI/TextureAlchemy route on one low-risk prop before broader regeneration.

## Defer

- Official Unity MCP until Unity AI entitlement/seat is fixed.
- More active-project MCP bridges.
- Full production completion claims.

## Next Missions

1. Character production promotion and player-camera evidence.
2. Hunyuan3D 2.1 HomePC shape+texture reality check on one low-risk prop.
3. Realified quarantine manifest v2 with per-asset semantic labels.
4. Environment material gameplay close-up.
5. Loot pickup player-POV visibility gate.

## References

- CoplayDev Unity MCP: https://github.com/CoplayDev/unity-mcp
- CoderGamester MCP Unity: https://github.com/CoderGamester/mcp-unity
- Unity Editor command-line arguments: https://docs.unity3d.com/Manual/EditorCommandLineArguments.html
- Unity Test Framework command-line execution: https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html
- Unity MCP entitlement error: https://support.unity.com/hc/en-us/articles/48958235901460-Unity-AI-MCP-connection-fails-with-the-error-message-Status-Capacity-Limit

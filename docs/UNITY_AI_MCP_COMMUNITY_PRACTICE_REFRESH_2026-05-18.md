# Unity AI/MCP Community Practice Refresh

Date: 2026-05-18

## Conclusion

The current low-efficiency feeling is real, but the fix is not to add more random MCP tools. The best working pattern is:

```text
mission-level agent work
  -> deterministic Unity Editor menu commands
  -> community MCP executes short commands and reads Console
  -> JSON/screenshot evidence
  -> Nemotron/UI-TARS quality review
  -> promote or quarantine assets
```

MCP should be transport, not the brain. Unity is a stateful editor with compile/import/reload timing, so long chains of small MCP edits become fragile. The stable move is to make Unity menu commands do bigger deterministic operations.

## Local Evidence

- Community Unity MCP is installed through `com.coplaydev.unity-mcp`.
- Unity project dependency points at `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`.
- Local reports show the community MCP path can run the tactical acceptance pipeline.
- `RealifiedAssets` exists and currently contains:
  - 48 `.glb`
  - 145 `.png`
  - 13 `.mat`
- `REALIFIED_CATEGORY_NEMOTRON_REVIEWS.json` still reports:
  - `promotion_allowed=false`
  - failed categories: weapon, character, gear, loot, environment_prop
- `TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json` still reports:
  - `full_visual_asset_gate_passed=false`

## Community-Aligned Decisions

### Adopt

- Keep CoplayDev `unity-mcp` as the active Unity MCP bridge.
- Use Unity `AI Tools/...` menu commands as the project-specific automation API.
- Use MCP mostly for `execute_menu_item` and `read_console`.
- Keep generated assets quarantined until technical, semantic, and gameplay gates pass.
- Use Nemotron as the local visual quality judge and UI-TARS as the bounded GUI executor.

### Probe

- CoderGamester `mcp-unity` only in a project copy/worktree as backup.
- glTFast import/material validation for GLB/PBR workflows.
- Assembly definition split to reduce Unity reload pain.
- Unity Test Framework batchmode checks in a separate project copy when needed.

### Defer

- Official Unity MCP until Unity AI entitlement/seat is fixed.
- Full production visual completion claims until player-camera gameplay evidence sees promoted assets.

### Reject For Now

- Installing multiple Unity MCP bridges into the active project at once.
- Promoting the whole Realified batch just because files exist.
- Counting contact-sheet/showcase proof as playable game integration.

## Why This Matters

The Kimi handoff proves the generation side can produce files. It does not prove the Unity game is visually upgraded. The next efficient step is not more generation; it is stricter class-by-class promotion:

1. Import/refresh/material validation.
2. Category contact sheet.
3. Nemotron semantic pass.
4. Gameplay binding.
5. Player-camera evidence.
6. Acceptance report updates.

## Next Practical Missions

1. `M69_kimi_handoff_factory_verification`: mark trusted vs stale handoff claims.
2. `M70_realified_import_material_validation_menu_gate`: add one Unity menu command to refresh and validate Realified GLB/PBR import.
3. `M71_asset_class_promotion_queue`: promote one asset class at a time instead of global promotion.
4. `M72_player_camera_promoted_asset_visibility_gate`: prove promoted assets appear from the actual player camera during gameplay.

## Source Pointers

- CoplayDev `unity-mcp`: https://github.com/CoplayDev/unity-mcp
- CoderGamester `mcp-unity`: https://github.com/CoderGamester/mcp-unity
- Unity `MenuItem`: https://docs.unity3d.com/ScriptReference/MenuItem.html
- Unity `AssetDatabase.Refresh`: https://docs.unity3d.com/ScriptReference/AssetDatabase.Refresh.html
- Unity command-line arguments: https://docs.unity3d.com/Manual/EditorCommandLineArguments.html
- Unity Test Framework command line: https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html
- Unity assembly definitions: https://docs.unity3d.com/Manual/assembly-definition-files.html
- glTFast: https://github.com/atteneder/glTFast

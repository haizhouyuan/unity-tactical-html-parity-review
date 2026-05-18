# Critical Assessment Of External Review Recommendations

Date: 2026-05-18

This document records how the external review recommendations were evaluated against the actual local Unity project state.

The external review is useful, but it is not authoritative. It was based on the public repository export and report files, while local Codex has direct access to the working tree, Unity menu scripts, acceptance reports, and prior MCP failure modes. Therefore every suggestion is triaged as `adopt`, `adapt`, `already-present`, `defer`, or `reject`.

## Current Ground Truth

Current exported evidence:

- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
  - `all_required_current_gates_passed=true`
  - `full_visual_asset_gate_passed=false`
  - `category_semantic_review_passed=false`
  - `realified_asset_gameplay_production_promoted_assets=1`
  - `promoted_asset_visibility_gate_passed=false`
- `docs/REALIFIED_ASSET_GAMEPLAY_PROMOTION_LEDGER.json`
  - 12 assets checked.
  - 3 technical/semantic/scene partial assets.
  - 1 production-promoted asset.
- `docs/PROMOTED_ASSET_PLAYER_CAMERA_VISIBILITY_GATE.json`
  - `production_promoted_classes=0`
  - `candidate_visible_objects=103`
  - `blocked_reason=no_production_promoted_assets`
- `docs/UNITY_MCP_RELIABILITY_REPORT.md`
  - community MCP transport has worked.
  - Unity compile/reload state has previously made new menu commands temporarily invisible.

## Recommendation Triage

| Recommendation | Decision | Reason |
| --- | --- | --- |
| Strengthen `AGENTS.md` with tactical parity, MCP discipline, and asset promotion rules. | adopt | The original `AGENTS.md` was too generic and child-game oriented. This is low-risk and directly prevents repeated false-completion claims. |
| Create 6 focused Codex skills instead of a large agent zoo. | adapt | Good idea, but keep skills lightweight and project-local. No large subagent framework or extra docs per skill. |
| Treat MCP as transport, not as the design brain. | adopt | Matches actual failures in `UNITY_MCP_RELIABILITY_REPORT.md`. Long chains of tiny MCP edits are fragile. |
| Add deterministic `AI Tools/...` menu commands. | adapt | Some commands already exist. Add only safe report/gate aliases. Do not add destructive or ambiguous one-click promotion. |
| Add `AI Tools/Promote One Asset Class To Gameplay`. | defer | A no-argument promotion menu is dangerous. Asset promotion needs an explicit class, semantic evidence, player-camera proof, and gameplay event. This should be a future mission-specific tool, not a generic button. |
| Use `AI Tools/Run Tactical Acceptance Pipeline` as the main gate. | already-present | Existing command is present and writes `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`. |
| Add `AI Tools/Run Tactical Preflight`. | adopt | Safe, static, and useful before running the heavier pipeline. |
| Add `AI Tools/Run Unity MCP Smoke Check`. | adapt | An in-Editor command cannot prove MCP transport by itself, but if invoked through MCP it can produce a smoke JSON. The report must distinguish editor readiness from external transport proof. |
| Add `AI Tools/Run Game Feel Evidence Gate`. | adapt | Safe as a wrapper around existing gameplay proof fields. It should not broaden into new weapon work by itself. |
| Treat HTML parity pass as distinct from visual realism pass. | adopt | This is already true in reports and should be repeated in instructions. |
| Delegate broad work to many agents. | defer | The repo needs tighter mission contracts first. More agents without stricter gates will produce more noisy files. |

## Menu Command State

Commands already present before this pass:

- `AI Tools/Open Tactical Playable Scene`
- `AI Tools/Run Tactical Acceptance Pipeline`
- `AI Tools/Capture Tactical Verified Player POV Screenshot`
- `AI Tools/Write Promoted Asset Player Camera Visibility Gate`
- `AI Tools/Validate Realified Import And Materials`

Commands added or planned in this pass:

- `AI Tools/Run Tactical Preflight` - added as a static readiness report.
- `AI Tools/Run Unity MCP Smoke Check` - added as an in-Editor smoke report; external MCP transport is only proven when a client invokes it successfully.
- `AI Tools/Run Game Feel Evidence Gate` - added as a focused report wrapper around existing gameplay-proof evidence.

Command intentionally not added:

- `AI Tools/Promote One Asset Class To Gameplay`

Reason: it would hide a critical decision behind a menu item without an explicit class argument. Promotion must remain evidence-driven and class-specific.

## Implementation Standard Going Forward

External review suggestions should be handled in this order:

1. Check whether the capability already exists.
2. Check whether the suggestion would lower a gate or blur a completion definition.
3. Prefer documentation/rules for process recommendations.
4. Prefer deterministic report-writing menu commands for Unity automation.
5. Avoid one-click promotion or broad scene mutation commands.
6. Require JSON evidence and player-camera screenshots before claiming a gameplay/visual change.

## Verification Note

During this pass, MCP transport responded at `http://127.0.0.1:8080/mcp`, but executing the newly added `AI Tools/Run Tactical Preflight` menu returned:

```text
Failed to execute menu item 'AI Tools/Run Tactical Preflight'. It might be invalid, disabled, or context-dependent.
```

This is not counted as a working Unity-menu verification. The likely explanation is that the active Unity Editor session was not compiled/reloaded against this public export checkout, or the new Editor script had not been imported into the active AppDomain. This matches the existing recovery guidance in `docs/UNITY_MCP_RELIABILITY_REPORT.md`.

Therefore, this commit claims only repository-level changes and static validation. A future local Unity pass must open this checkout in Unity, wait for compile/update idle, then run `AI Tools/Run Tactical Preflight` and commit the generated report.

## Next Useful Mission

Do not start with broad art upgrade. The next high-value mission is:

1. Add sidearm-specific player-camera gameplay event evidence.
2. Update `RealifiedAssetGameplayPromotionLedger.cs` to count sidearm only from that evidence.
3. Run `AI Tools/Run Tactical Acceptance Pipeline`.
4. Confirm `realified_asset_gameplay_production_promoted_assets` increases only if the route proves sidearm visibility and event participation.

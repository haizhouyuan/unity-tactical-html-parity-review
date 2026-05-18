# Unity MCP + Kimi Factory Handoff Verification

Date: 2026-05-17

## Result

The community Unity MCP route is now verified as the working automation path for this project.

- Official Unity MCP remains blocked by Unity AI plan / seat entitlement.
- CoplayDev MCP for Unity is installed and configured in `Packages/manifest.json`.
- Codex config includes `unityMCPCommunity` at `http://localhost:8080/mcp`.
- The Unity-side helper `AI Tools/Start Community Unity MCP Server` now sets the HTTP URL explicitly and retries bridge verification.
- Live verification reached `Transport 'websocket' connected [ws://127.0.0.1:8080/hub/plugin]`.
- Direct MCP `initialize` returned `mcp-for-unity-server` with 43 tools.
- Direct MCP `read_console` works.
- Direct MCP `execute_menu_item` successfully ran `AI Tools/Run Tactical Acceptance Pipeline`.

## Kimi Handoff Verification

Kimi's handoff at `/Users/yuanshaochen/Projects/local-coding-runners/HANDOFF.md` is partly verified.

Verified locally:

- `~/Projects/local-coding-runners/scripts/quick-verify.sh` passed.
- `llama-server` on `8083` is reachable.
- `UI-TARS` on `8082` is reachable.
- MCP self-test passed.
- The Nemotron + UI-TARS split is usable as a future quality gate: Nemotron observes / judges; UI-TARS acts.

Not promoted to production completion:

- The generated 48 GLBs in `Assets/HtmlTacticalAssets/RealifiedAssets/` are still quarantined.
- Current audit says `48` textured probes and `0` promotable PBR candidates.
- They can prove the asset factory pipeline is runnable, but not that the output is semantically correct or game-ready.

## Unity Acceptance Evidence

The latest acceptance pipeline was triggered through the community MCP route, not AppleScript.

Report:

- `/Users/yuanshaochen/My project/docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- timestamp: `2026-05-17T13:37:45.6980810Z`
- `console_errors`: `0`
- `player_pov_gate_passed`: `true`
- `gameplay_proof_gate_passed`: `true`
- `playable_route_gate_passed`: `true`
- `approved_incremental_asset_gate_passed`: `true`
- `visual_polish_gate_passed`: `true`
- `all_required_current_gates_passed`: `true`
- `full_visual_asset_gate_passed`: `false`

Route asset quality highlights:

- `approved_crate_pbr_renderer_instances`: `22`
- `approved_container_pbr_renderer_instances`: `990`
- `approved_player_pbr_renderer_instances`: `36`
- `approved_enemy_pbr_renderer_instances`: `396`
- `approved_weapon_pbr_renderer_instances`: `1468`
- `wet_asphalt_renderer_count`: `2`
- `concrete_renderer_count`: `71`
- `character_animation_state_evidence_count`: `12`
- `realified_audit_total_glb`: `48`
- `realified_audit_promotable_pbr_candidates`: `0`

## What Changed In Unity

`Assets/Editor/TacticalPrototypeTools.cs`

- Weapon PBR materials are now created from approved texture sets:
  - `TacticalWeaponSidearmPbrApproved`
  - `TacticalWeaponHeroRiflePbrApproved`
  - `TacticalWeaponSecondaryPbrApproved`
- First-person weapon visuals now receive weapon PBR override materials.
- Third-person player weapon visuals now receive weapon PBR override materials.
- NPC weapon visuals now receive weapon PBR override materials.
- Weapon loot GLB attachments now receive weapon PBR override materials.

`Assets/Editor/TacticalPlayableRouteGate.cs`

- Added `approved_weapon_pbr_renderer_instances`.
- `first_person_weapon_polish_passed` now requires weapon PBR renderer evidence.

`Assets/Editor/CommunityMcpTools.cs`

- Explicitly sets `MCPForUnity.HttpUrl` to `http://127.0.0.1:8080`.
- Enables HTTP local/project-scoped/autostart settings.
- Retries bridge verification so the server can finish startup before failing.

## Community Pattern Adopted

The current best working pattern is:

```text
Codex / Kimi / Gemini mission packet
  -> deterministic Unity Editor menu command
  -> community MCP execute_menu_item / read_console
  -> route gate JSON + screenshots
  -> Nemotron semantic review
  -> asset promotion or quarantine
```

MCP should not be the whole brain. It should run short Unity operations and read state. Asset quality should be judged by deterministic reports plus VLM review.

## Remaining Blockers

`full_visual_asset_gate_passed` remains false because:

1. The generated Realified GLB batch is pipeline-ready but not production-ready.
2. Character animation is still placeholder/proxy evidence, not authored humanoid clips.
3. First-person weapon art is now PBR-bound and route-gated, but still needs hero-asset visual review and model replacement.
4. The game is playable and closer to HTML parity, but still far from PUBG-like realism.

## Next Work

1. Add a stable MCP smoke script that performs initialize, tools/list, read_console, and execute_menu_item.
2. Add Nemotron contact-sheet review for `RealifiedAssets` before any category promotion.
3. Replace the proxy character path with a real humanoid rig and authored clips.
4. Replace the current first-person weapon geometry with a final hero energy/tactical weapon asset.
5. Keep all generated assets quarantined until semantic category review passes.

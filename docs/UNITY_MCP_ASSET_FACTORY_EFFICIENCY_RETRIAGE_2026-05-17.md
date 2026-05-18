# Unity MCP + Asset Factory Efficiency Retriage

Date: 2026-05-17

## Short Answer

The next efficiency gain is not another MCP install. It is making Unity do larger deterministic operations through Editor menu commands, while MCP only triggers those commands and reads reports.

Current active route:

```text
Codex/Kimi/Gemini mission
  -> community Unity MCP execute_menu_item/read_console
  -> Unity Editor command writes JSON + screenshots
  -> Nemotron/UI-TARS visual review where needed
  -> class-by-class asset promotion
```

## Local State

- Community Unity MCP is active: `mcp-for-unity-server 3.3.1`, 43 tools.
- Official Unity MCP remains deferred because the blocker was Unity plan/seat entitlement.
- Local Kimi/Nemotron/UI-TARS stack passed `quick-verify.sh`.
- Tactical route gates pass with zero console errors.
- `full_visual_asset_gate_passed` is still false.
- `RealifiedAssets` currently contains 48 GLBs and 138 texture sidecars.
- Current audit: 27 promotable PBR candidates, 21 textured probes.
- Nemotron contact-sheet review still blocks global promotion.

## What Kimi Handoff Means

Kimi's handoff is valuable but must be read narrowly:

- File-generation/copy pipeline: verified.
- Some PBR/LOD assets: usable inputs.
- Full production gameplay art: not complete.
- Global asset promotion: blocked.
- First-person weapon visual proof: still needs work.
- Character art/retarget quality: still intermediate.

## Community Lesson Applied

Use MCP as transport, not as the development brain.

Good:

```text
MCP -> Execute one Unity menu item -> JSON report -> screenshots -> review
```

Bad:

```text
MCP -> dozens of small Unity edits/clicks -> compile reload churn -> stale state -> unclear proof
```

## Next Missions

1. **M55 First-Person Hero Weapon Truth Gate**
   - Fix `PlayableRoute/08_fire_hit_first_person.png`.
   - It must show a real foreground weapon from actual player camera.

2. **M56 Realified Asset Class Promotion Queue**
   - Promote weapon/crate/container/gear/loot/character classes separately.
   - No probe-only batch promotion.

3. **M57 Character Art / Retarget Upgrade**
   - Move from intermediate skinned tactical imports to believable humanoid art and clips.

4. **M58 One-Button Unity Acceptance Pipeline Hardening**
   - One MCP call should run refresh, compile idle check, route gate, screenshots, material audit, and semantic review summary.

5. **M59 Generator Script Normalization + Nemotron Integration**
   - Locate/recreate the real generator entrypoint and integrate fail-closed post-generation quality gates.

Canonical M54 output:

`/Users/yuanshaochen/Projects/ai-game-generation-research/tasks/mission_outputs/M54_unity_mcp_asset_factory_community_retriage/summary.md`

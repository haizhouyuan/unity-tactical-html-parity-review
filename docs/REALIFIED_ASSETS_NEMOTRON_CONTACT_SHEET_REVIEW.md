# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T18:28:37.141213+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/glb_pbr_textured_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `True`

## Summary

All visible items are weapons, but expected categories include non-weapon types; technical status is textured_probe_not_production for all.

## Promotion Blockers

- `All assets are textured_probe_not_production with missing PBR maps and mesh attributes`
- `No sidecar PBR maps or sufficient mesh attributes (NORMAL/TANGENT) visible for any asset`
- `Visible items are all weapons, but expected categories include non-weapon types (character, gear, loot, environment_prop)`
- `missing_normal_texture`
- `missing_occlusion_or_ao_texture`
- `missing_primitive_normal_attribute`
- `missing_tangent_attribute`
- `no_final_textured_asset_has_production_ready_status`
- `technical_audit_marks_assets_as_textured_probe_not_production`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_02_sidearm_textured.glb` | `weapon` | `weapon` | `True` | `high` | Visible silhouette matches sidearm weapon category. |
| 2 | `RS_03_secondary_weapon_textured.glb` | `weapon` | `weapon` | `True` | `high` | Visible silhouette matches secondary weapon category. |
| 3 | `RS_04_player_tactical_textured.glb` | `character` | `weapon` | `False` | `high` | Visible item is a weapon, not a character model. |
| 4 | `RS_05_enemy_tactical_textured.glb` | `character` | `weapon` | `False` | `high` | Visible item is a weapon, not a character model. |
| 5 | `RS_06_gear_helmet_textured.glb` | `gear` | `weapon` | `False` | `high` | Visible item is a weapon, not a helmet gear item. |
| 6 | `RS_07_gear_vest_textured.glb` | `gear` | `weapon` | `False` | `high` | Visible item is a weapon, not a vest gear item. |
| 7 | `RS_08_loot_ammo_textured.glb` | `loot` | `weapon` | `False` | `high` | Visible item is a weapon, not ammo loot. |
| 8 | `RS_09_loot_medkit_textured.glb` | `loot` | `weapon` | `False` | `high` | Visible item is a weapon, not medkit loot. |
| 9 | `RS_10_prop_container_textured.glb` | `environment_prop` | `weapon` | `False` | `high` | Visible item is a weapon, not a container prop. |
| 10 | `RS_11_prop_crate_textured.glb` | `environment_prop` | `weapon` | `False` | `high` | Visible item is a weapon, not a crate prop. |
| 11 | `RS_12_shotgun_textured.glb` | `weapon` | `weapon` | `True` | `high` | Visible silhouette matches shotgun weapon category. |
| 12 | `hero_rifle_textured.glb` | `weapon` | `weapon` | `True` | `high` | Visible silhouette matches rifle weapon category. |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "fail",
  "summary": "All visible items are weapons, but expected categories include non-weapon types; technical status is textured_probe_not_production for all.",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "RS_02_sidearm_textured.glb",
      "expected_category": "weapon",
      "visible_category": "weapon",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible silhouette matches sidearm weapon category."
    },
    {
      "grid_index": 2,
      "expected_file": "RS_03_secondary_weapon_textured.glb",
      "expected_category": "weapon",
      "visible_category": "weapon",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible silhouette matches secondary weapon category."
    },
    {
      "grid_index": 3,
      "expected_file": "RS_04_player_tactical_textured.glb",
      "expected_category": "character",
      "visible_category": "weapon",
      "category_match": false,
      "confidence": "high",
      "reason": "Visible item is a weapon, not a character model."
    },
    {
      "grid_index": 4,
      "expected_file": "RS_05_enemy_tactical_textured.glb",
      "expected_category": "character",
      "visible_category": "weapon",
      "category_match": false,
      "confidence": "high",
      "reason": "Visible item is a weapon, not a character model."
    },
    {
      "grid_index": 5,
      "expected_file": "RS_06_gear_helmet_textured.glb",
      "expected_category": "gear",
      "visible_category": "weapon",
      "category_match": false,
      "confidence": "high",
      "reason": "Visible item is a weapon, not a helmet gear item."
    },
    {
      "grid_index": 6,
      "expected_file": "RS_07_gear_vest_textured.glb",
      "expected_category": "gear",
      "visible_category": "weapon",
      "category_match": false,
      "confidence": "high",
      "reason": "Visible item is a weapon, not a vest gear item."
    },
    {
      "grid_
```

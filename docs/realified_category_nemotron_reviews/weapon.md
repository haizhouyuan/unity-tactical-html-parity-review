# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:25:27.665945+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/RealifiedCategorySheets/realified_weapon_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `True`

## Summary

Three weapons match expected categories but quality blockers prevent promotion.

## Promotion Blockers

- `expected_assets_not_technically_promotable: RS_12_shotgun_LOD0.glb`
- `missing TANGENT attribute for production normal mapping`
- `missing normalTexture`
- `missing occlusionTexture/AO`
- `missing_normal_texture`
- `missing_occlusion_or_ao_texture`
- `missing_tangent_attribute`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_02_sidearm_LOD0.glb` | `weapon` | `weapon` | `True` | `high` | visible sidearm silhouette matches expected weapon |
| 2 | `RS_03_secondary_weapon_LOD0.glb` | `weapon` | `weapon` | `True` | `high` | visible secondary weapon silhouette matches expected |
| 3 | `RS_12_shotgun_LOD0.glb` | `weapon` | `weapon` | `True` | `medium` | visible shotgun silhouette but missing PBR maps |
| 4 | `hero_rifle_LOD0.glb` | `weapon` | `weapon` | `True` | `high` | visible rifle silhouette matches expected |

## Raw Model Output Excerpt

```text
<think>
We need to compare each visible object (grid_index 1-4) against expected file/category list.

We have 4 items in grid, each presumably a weapon. Expected files: RS_02_sidearm_LOD0.glb (weapon), RS_03_secondary_weapon_LOD0.glb (weapon), RS_12_shotgun_LOD0.glb (weapon), hero_rifle_LOD0.glb (weapon). All expected category weapon.

We need to see visible category for each. The image shows four weapons, all appear to be rifles/weapons. So visible_category likely "weapon". Need to check if any looks like character, gear, etc. All are weapons, so category_match true for each.

Confidence: high (clearly weapons). Reason: matches expected weapon silhouette.

Now technical status: The technical audit summary says 27 promotable_pbr_candidate, 21 textured_probe_not_production. But we need to see if any blockers. The expected items have technical_status: promotable_pbr_candidate for 1,2,4; textured_probe_not_production for 3. However the visible objects are weapons; we need to see if any quality blockers (e.g., missing textures). The technical audit summary indicates many assets are textured probes (embedding textures). But we only have visual info; we cannot see texture maps. However the expected file for index 3 is "RS_12_shotgun_LOD0.glb" with status textured_probe_not_production and reasons missing normalTexture etc. That suggests that the visible object at index 3 may have quality issues. But the question: "promotion_allowed": true only when all visible objects match categories and there are no meaningful quality blockers. Since index 3 has a blocker (missing normalTexture etc.), promotion_allowed should be false.

But we need to see if the visible object at index 3 indeed has those blockers. The technical audit summary indicates 21 assets are textured_probe_not_production, meaning they lack proper PBR maps. So likely index 3 is one of those. So promotion_blockers should include the reasons for index 3.

We need to list promotion_blockers as strings. Probably includ
```

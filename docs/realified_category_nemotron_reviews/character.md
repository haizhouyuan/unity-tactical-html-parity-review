# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:25:45.002719+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/RealifiedCategorySheets/realified_character_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `True`

## Summary

Two weapon assets visible, but expected character categories, causing category mismatch and promotion blocker.

## Promotion Blockers

- `category mismatch (visible weapon vs expected character)`
- `expected_assets_not_technically_promotable: RS_04_player_tactical_LOD0.glb, RS_05_enemy_tactical_LOD0.glb`
- `missing_normal_texture`
- `missing_occlusion_or_ao_texture`
- `missing_tangent_attribute`
- `textured_probe_not_production (missing normalTexture, occlusionTexture/AO, TANGENT)`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_04_player_tactical_LOD0.glb` | `character` | `weapon` | `False` | `high` | Visible object is a rifle, not a character. |
| 2 | `RS_05_enemy_tactical_LOD0.glb` | `character` | `weapon` | `False` | `high` | Visible object is a rifle, not a character. |

## Raw Model Output Excerpt

```text
<think>
We need to parse the context. The user says: "You are reviewing a Unity contact sheet of AI-generated GLB assets. The grid is ordered left-to-right, top-to-bottom. Compare each visible object against the expected file/category list below."

We have expected assets list with two entries:

grid_index 1: expected_file "RS_04_player_tactical_LOD0.glb", expected_category "character", technical_status "textured_probe_not_production", technical_reasons list.

grid_index 2: expected_file "RS_05_enemy_tactical_LOD0.glb", expected_category "character", same technical status.

So expected categories are both "character". The visible objects: there are two weapons (rifle-like). The grid is left-to-right, top-to-bottom. So grid_index 1 is left object (blue/teal rifle). grid_index 2 is right object (pink/red rifle). Both appear to be weapons (rifles). The expected category is "character". But the visible category is "weapon". According to instructions: "if character, gear, loot, container, or crate items visibly look like rifle/weapon silhouettes, mark category_match=false." So category_match should be false for both items because they are weapons but expected category is character. Also need to set visible_category to appropriate category: "weapon". So visible_category = "weapon". category_match = false.

Confidence: Since we can clearly see they are weapons, but expected category is character, confidence maybe "high" that visible category is weapon, but category_match false. The confidence refers to how confident we are about the visible_category classification. I'd say "high".

Reason: "Visible objects are rifles, not characters."

Now overall_verdict: We need to decide pass/fail/uncertain. The items do not match expected categories, so likely fail. Also technical status indicates textured_probe_not_production, which is a blocker. So overall verdict likely "fail".

Summary: short sentence: "Two weapon assets visible, but expected character categories, causing category 
```

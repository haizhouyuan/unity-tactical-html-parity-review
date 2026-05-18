# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:26:08.258885+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/RealifiedCategorySheets/realified_gear_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `False`

## Summary

Two weapon objects visible, but expected gear items; category mismatch.

## Promotion Blockers

- `category mismatch (visible weapon vs expected gear)`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_06_gear_helmet_LOD0.glb` | `gear` | `weapon` | `False` | `high` | Visible object is a rifle, not a helmet or vest. |
| 2 | `RS_07_gear_vest_LOD0.glb` | `gear` | `weapon` | `False` | `high` | Visible object is a rifle, not a vest. |

## Raw Model Output Excerpt

```text
<think>
We need to parse the context. The user says: "You are reviewing a Unity contact sheet of AI-generated GLB assets. The grid is ordered left-to-right, top-to-bottom. Compare each visible object against the expected file/category list below."

We have expected assets list with two entries:

grid_index 1: expected_file "RS_06_gear_helmet_LOD0.glb", expected_category "gear", technical_status "promotable_pbr_candidate", technical_reasons [].

grid_index 2: expected_file "RS_07_gear_vest_LOD0.glb", expected_category "gear", technical_status "promotable_pbr_candidate", technical_reasons [].

So expected categories are both "gear". The visible objects: there are two objects in the image: two blue rifles (looks like weapons). They are clearly weapon silhouettes, not gear (helmet, vest). So category_match should be false for both items. Also expected categories are gear, but visible category is "weapon". So category_match false.

We need to fill items array with entries for each visible object. The grid is ordered left-to-right, top-to-bottom. There are two objects, presumably grid_index 1 is left object, grid_index 2 is right object. So we need two items.

For each item, we need:
- grid_index: 1 or 2
- expected_file: from expected list (matching index)
- expected_category: "gear"
- visible_category: "weapon|character|gear|loot|environment_prop|unknown". Since they are weapons, visible_category = "weapon".
- category_match: true/false. Since expected is gear, visible is weapon => false.
- confidence: "high|medium|low". The visual evidence is clear, so confidence high.
- reason: short reason.

Also need overall_verdict: pass|fail|uncertain. Since category_match false for both, likely fail. But we need to consider promotion_allowed: true only when all visible objects match expected categories and no blockers. Since they don't match, promotion_allowed = false. However the schema expects promotion_allowed boolean. So set false.

promotion_blockers: list of strings. Since c
```

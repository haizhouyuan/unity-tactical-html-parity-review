# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:26:45.631163+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/RealifiedCategorySheets/realified_environment_prop_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `False`

## Summary

Two weapon assets visible, mismatching expected environment prop categories.

## Promotion Blockers

- `Category mismatch: visible objects are weapons, expected environment props.`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_10_prop_container_LOD0.glb` | `environment_prop` | `weapon` | `False` | `high` | Visible object is a rifle, not an environment prop. |
| 2 | `RS_11_prop_crate_LOD0.glb` | `environment_prop` | `weapon` | `False` | `high` | Visible object is a rifle, not an environment prop. |

## Raw Model Output Excerpt

```text
<think>
We need to parse the context. The user says: "You are reviewing a Unity contact sheet of AI-generated GLB assets. The grid is ordered left-to-right, top-to-bottom. Compare each visible object against the expected file/category list below."

We have expected assets list with two entries:

grid_index 1: expected_file "RS_10_prop_container_LOD0.glb", expected_category "environment_prop", technical_status "promotable_pbr_candidate", technical_reasons [].

grid_index 2: expected_file "RS_11_prop_crate_LOD0.glb", expected_category "environment_prop", technical_status "promotable_pbr_candidate", technical_reasons [].

So expected categories are both "environment_prop". The visible objects: there are two objects in the image: a blue rifle-like weapon on left, and a tan rifle-like weapon on right. They look like weapons (rifle silhouettes). So visible categories: likely "weapon". The expected categories are "environment_prop". So category_match = false for both items (since they are weapons, not environment props). Also need to check if any visible object looks like rifle/weapon silhouette => mark category_match=false. Indeed they are weapons.

Now we need to fill items array with entries for each grid_index (1 and 2). For each, we need visible_category (choose from weapon|character|gear|loot|environment_prop|unknown). Since they are weapons, visible_category = "weapon". category_match = false (since expected is environment_prop). confidence: maybe "medium" because it's clear they are weapons, but maybe ambiguous? The image clearly shows weapons, so confidence high? But the expected category is environment_prop, which is mismatched. Confidence maybe "high" because we are sure they are weapons, not environment props. However the instruction: "if character, gear, loot, container, or crate items visibly look like rifle/weapon silhouettes, mark category_match=false." So we must set category_match false. Confidence maybe "high" because it's obvious.

Reason: "Visible obje
```

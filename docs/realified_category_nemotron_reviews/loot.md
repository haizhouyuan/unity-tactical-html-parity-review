# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:26:25.669063+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/RealifiedCategorySheets/realified_loot_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/REALIFIED_ASSETS_IMPORT_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `False`

## Summary

Two weapon assets visible, mismatching expected loot categories, causing failure.

## Promotion Blockers

- `category mismatch (weapon vs loot) for both items`
- `visual objects are rifles, not ammo or medkit`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `RS_08_loot_ammo_LOD0.glb` | `loot` | `weapon` | `False` | `high` | Visible object is a rifle, not ammo or medkit. |
| 2 | `RS_09_loot_medkit_LOD0.glb` | `loot` | `weapon` | `False` | `high` | Visible object is a rifle, not medkit. |

## Raw Model Output Excerpt

```text
<think>
We need to parse the context. The user says: "You are reviewing a Unity contact sheet of AI-generated GLB assets." The grid is ordered left-to-right, top-to-bottom. Compare each visible object against the expected file/category list below.

Expected assets list includes two entries:

grid_index 1: expected_file "RS_08_loot_ammo_LOD0.glb", expected_category "loot", technical_status "promotable_pbr_candidate", technical_reasons [].

grid_index 2: expected_file "RS_09_loot_medkit_LOD0.glb", expected_category "loot", technical_status "promotable_pbr_candidate", technical_reasons [].

So we have two items in the grid. The image shows two objects: two rifles (looks like weapons). They are not ammo or medkit; they are weapons. So visible_category likely "weapon". But expected_category is "loot". So category_match = false.

We need to fill items array with each grid_index (1 and 2). For each, we need visible_category (weapon|character|gear|loot|environment_prop|unknown). Since they are weapons, visible_category = "weapon". category_match = false (since expected is loot). confidence: maybe "medium" because it's clear they are weapons, but expected is loot, so mismatch. reason: "Visible objects are rifles, not ammo or medkit."

Now overall_verdict: Since category_match is false for both items, the overall verdict likely "fail". But we need to consider if any blockers. Promotion_allowed: only true when all visible objects match expected categories and no meaningful quality blockers. Here they don't match categories, so promotion_allowed = false.

promotion_blockers: list of reasons. Could include "category mismatch (visible weapon vs expected loot)" for each item. Also maybe "quality" but not needed. So we can list "category mismatch for grid_index 1 and 2" or separate entries. The schema expects a list of strings. We'll include "visible objects are weapons, not loot items". Also maybe "insufficient semantic category alignment".

Now summary: short sentence. Something 
```

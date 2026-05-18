# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:24:36.959245+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/HtmlBaselineCategorySheets/html_baseline_loot_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/HTML_BASELINE_ASSET_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `pass`
- Promotion allowed: `True`
- Deterministic guard applied: `False`

## Summary

All items match expected loot categories with no quality blockers.

## Promotion Blockers

- none

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `models/loot_ammo_final.glb` | `loot` | `loot` | `True` | `high` | Ammo box clearly represents loot item type. |
| 2 | `models/loot_medkit_final.glb` | `loot` | `loot` | `True` | `high` | Medkit with red cross is standard loot visual representation. |
| 3 | `models/loot_firstaid_final.glb` | `loot` | `loot` | `True` | `high` | First aid kit with red cross matches expected loot category. |
| 4 | `models/loot_bandage_final.glb` | `loot` | `loot` | `True` | `high` | Bandage roll with red cross is consistent with loot item type. |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "pass",
  "summary": "All items match expected loot categories with no quality blockers.",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "models/loot_ammo_final.glb",
      "expected_category": "loot",
      "visible_category": "loot",
      "category_match": true,
      "confidence": "high",
      "reason": "Ammo box clearly represents loot item type."
    },
    {
      "grid_index": 2,
      "expected_file": "models/loot_medkit_final.glb",
      "expected_category": "loot",
      "visible_category": "loot",
      "category_match": true,
      "confidence": "high",
      "reason": "Medkit with red cross is standard loot visual representation."
    },
    {
      "grid_index": 3,
      "expected_file": "models/loot_firstaid_final.glb",
      "expected_category": "loot",
      "visible_category": "loot",
      "category_match": true,
      "confidence": "high",
      "reason": "First aid kit with red cross matches expected loot category."
    },
    {
      "grid_index": 4,
      "expected_file": "models/loot_bandage_final.glb",
      "expected_category": "loot",
      "visible_category": "loot",
      "category_match": true,
      "confidence": "high",
      "reason": "Bandage roll with red cross is consistent with loot item type."
    }
  ],
  "promotion_allowed": true,
  "promotion_blockers": []
}<|im_end|>
```

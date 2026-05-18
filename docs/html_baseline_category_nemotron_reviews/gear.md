# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:24:29.848519+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/HtmlBaselineCategorySheets/html_baseline_gear_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/HTML_BASELINE_ASSET_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `False`

## Summary

Two assets visible but category and file mismatches detected

## Promotion Blockers

- `grid_index 1: visible object does not resemble helmet`
- `grid_index 2: visible object does not resemble vest`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `models/loot_helmet_final.glb` | `gear` | `unknown` | `False` | `medium` | Object appears spherical with small attachments, not helmet-shaped |
| 2 | `models/loot_vest_final.glb` | `gear` | `unknown` | `False` | `medium` | Object is a flat platform with blocks, not vest-shaped |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "fail",
  "summary": "Two assets visible but category and file mismatches detected",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "models/loot_helmet_final.glb",
      "expected_category": "gear",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object appears spherical with small attachments, not helmet-shaped"
    },
    {
      "grid_index": 2,
      "expected_file": "models/loot_vest_final.glb",
      "expected_category": "gear",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object is a flat platform with blocks, not vest-shaped"
    }
  ],
  "promotion_allowed": false,
  "promotion_blockers": [
    "grid_index 1: visible object does not resemble helmet",
    "grid_index 2: visible object does not resemble vest"
  ]
}<|im_end|>
```

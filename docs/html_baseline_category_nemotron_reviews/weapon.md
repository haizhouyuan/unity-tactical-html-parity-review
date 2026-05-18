# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:24:20.031588+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/HtmlBaselineCategorySheets/html_baseline_weapon_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/HTML_BASELINE_ASSET_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `fail`
- Promotion allowed: `False`
- Deterministic guard applied: `False`

## Summary

Visible objects do not match expected weapon categories

## Promotion Blockers

- `All visible objects appear as generic boxes rather than weapon silhouettes`
- `No clear weapon category indicators visible in any grid item`

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `models/pistol_m5_candidate.glb` | `weapon` | `unknown` | `False` | `medium` | Object appears as generic box, not pistol silhouette |
| 2 | `models/shotgun_m5_candidate.glb` | `weapon` | `unknown` | `False` | `medium` | Object appears as generic box, not shotgun silhouette |
| 3 | `models/groza_procedural_candidate.glb` | `weapon` | `unknown` | `False` | `medium` | Object appears as generic box, not rifle silhouette |
| 4 | `models/dmr_m5_candidate.glb` | `weapon` | `unknown` | `False` | `medium` | Object appears as generic box, not rifle silhouette |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "fail",
  "summary": "Visible objects do not match expected weapon categories",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "models/pistol_m5_candidate.glb",
      "expected_category": "weapon",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object appears as generic box, not pistol silhouette"
    },
    {
      "grid_index": 2,
      "expected_file": "models/shotgun_m5_candidate.glb",
      "expected_category": "weapon",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object appears as generic box, not shotgun silhouette"
    },
    {
      "grid_index": 3,
      "expected_file": "models/groza_procedural_candidate.glb",
      "expected_category": "weapon",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object appears as generic box, not rifle silhouette"
    },
    {
      "grid_index": 4,
      "expected_file": "models/dmr_m5_candidate.glb",
      "expected_category": "weapon",
      "visible_category": "unknown",
      "category_match": false,
      "confidence": "medium",
      "reason": "Object appears as generic box, not rifle silhouette"
    }
  ],
  "promotion_allowed": false,
  "promotion_blockers": [
    "All visible objects appear as generic boxes rather than weapon silhouettes",
    "No clear weapon category indicators visible in any grid item"
  ]
}<|im_end|>
```

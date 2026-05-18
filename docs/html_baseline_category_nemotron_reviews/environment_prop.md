# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:24:45.716405+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/HtmlBaselineCategorySheets/html_baseline_environment_prop_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/HTML_BASELINE_ASSET_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `pass`
- Promotion allowed: `True`
- Deterministic guard applied: `False`

## Summary

All visible objects match expected environment props with no weapon-like silhouettes or quality blockers.

## Promotion Blockers

- none

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `models/prop_container_final.glb` | `environment_prop` | `environment_prop` | `True` | `high` | Visible as a container with stacked items, matching expected prop type. |
| 2 | `models/prop_crate_final.glb` | `environment_prop` | `environment_prop` | `True` | `high` | Visible as a standard crate, matching expected prop type. |
| 3 | `models/prop_ladder_stair_final.glb` | `environment_prop` | `environment_prop` | `True` | `high` | Visible as a ladder/stair structure, matching expected prop type. |
| 4 | `models/prop_interior_furniture_final.glb` | `environment_prop` | `environment_prop` | `True` | `high` | Visible as interior furniture (e.g., desk/shelf), matching expected prop type. |
| 5 | `models/prop_ground_wall_fence_final.glb` | `environment_prop` | `environment_prop` | `True` | `high` | Visible as a wall/fence structure, matching expected prop type. |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "pass",
  "summary": "All visible objects match expected environment props with no weapon-like silhouettes or quality blockers.",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "models/prop_container_final.glb",
      "expected_category": "environment_prop",
      "visible_category": "environment_prop",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible as a container with stacked items, matching expected prop type."
    },
    {
      "grid_index": 2,
      "expected_file": "models/prop_crate_final.glb",
      "expected_category": "environment_prop",
      "visible_category": "environment_prop",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible as a standard crate, matching expected prop type."
    },
    {
      "grid_index": 3,
      "expected_file": "models/prop_ladder_stair_final.glb",
      "expected_category": "environment_prop",
      "visible_category": "environment_prop",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible as a ladder/stair structure, matching expected prop type."
    },
    {
      "grid_index": 4,
      "expected_file": "models/prop_interior_furniture_final.glb",
      "expected_category": "environment_prop",
      "visible_category": "environment_prop",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible as interior furniture (e.g., desk/shelf), matching expected prop type."
    },
    {
      "grid_index": 5,
      "expected_file": "models/prop_ground_wall_fence_final.glb",
      "expected_category": "environment_prop",
      "visible_category": "environment_prop",
      "category_match": true,
      "confidence": "high",
      "reason": "Visible as a wall/fence structure, matching expected prop type."
    }
  ],
  "promotion_allowed": true,
  "promotion_blockers": []
}<|im_end|>
```

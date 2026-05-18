# RealifiedAssets Nemotron Contact Sheet Review

- Timestamp: `2026-05-17T19:24:24.732861+00:00`
- Image: `/Users/yuanshaochen/My project/Assets/Screenshots/HtmlBaselineCategorySheets/html_baseline_character_contact_sheet.png`
- Audit: `/Users/yuanshaochen/My project/docs/HTML_BASELINE_ASSET_AUDIT.json`
- Endpoint: `http://127.0.0.1:8083/v1/chat/completions`
- Model: `nemotron-omni`
- Verdict: `pass`
- Promotion allowed: `True`
- Deterministic guard applied: `False`

## Summary

Two character models match expected categories and quality.

## Promotion Blockers

- none

## Item Review

| Grid | File | Expected | Visible | Match | Confidence | Reason |
|---:|---|---|---|---|---|---|
| 1 | `models/character_player_final.glb` | `character` | `character` | `True` | `high` | Model shows humanoid form with head, torso, limbs, and base stand consistent with player character. |
| 2 | `models/character_enemy_final.glb` | `character` | `character` | `True` | `high` | Model shows humanoid form with head, torso, limbs, and base stand consistent with enemy character. |

## Raw Model Output Excerpt

```text
<think></think>
{
  "overall_verdict": "pass",
  "summary": "Two character models match expected categories and quality.",
  "items": [
    {
      "grid_index": 1,
      "expected_file": "models/character_player_final.glb",
      "expected_category": "character",
      "visible_category": "character",
      "category_match": true,
      "confidence": "high",
      "reason": "Model shows humanoid form with head, torso, limbs, and base stand consistent with player character."
    },
    {
      "grid_index": 2,
      "expected_file": "models/character_enemy_final.glb",
      "expected_category": "character",
      "visible_category": "character",
      "category_match": true,
      "confidence": "high",
      "reason": "Model shows humanoid form with head, torso, limbs, and base stand consistent with enemy character."
    }
  ],
  "promotion_allowed": true,
  "promotion_blockers": []
}<|im_end|>
```

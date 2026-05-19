# M95 Final Weapon Art Review Gate

Generated: 2026-05-19T00:51:36.2123790Z

- Passed: `False`
- Policy: final weapon art readiness requires gameplay evidence, visible screenshots, PBR sidecars, and explicit final art review clearance.

| Check | Passed | Meaning |
|---|---:|---|
| `weapon_feel_gate_passed` | True | M92 weapon feel gate passed in play mode. |
| `first_person_weapon_framing_passed` | True | First-person rifle framing and ADS readability are supported by screenshot evidence. |
| `third_person_npc_mount_passed` | True | Third-person/NPC rifle mount has quality and pulse evidence. |
| `ads_reload_fire_feedback_passed` | True | ADS, reload, fire, recoil, muzzle/tracer/casing, hit, and ammo mutation evidence all exist. |
| `material_pbr_sidecar_present` | True | Hero rifle GLB, PBR material, and basecolor/normal/metallic/roughness/AO sidecars exist. |
| `visible_screenshot_evidence_paths_exist` | True | Required first-person, fire, reload, third-person, and external-input route screenshots exist on disk. |
| `playable_route_weapon_evidence_passed` | True | Playable route evidence includes external-input built-player weapon use and gameplay-bound weapon polish. |
| `explicit_final_weapon_art_review_approval` | False | Final weapon art approval must come from docs/M95_FINAL_WEAPON_ART_REVIEW_APPROVAL.json, not renderer/material counts. |
| `placeholder_procedural_block_risk_cleared` | False | Placeholder/procedural-block risk must be cleared by final review and not by contact-sheet-only evidence. |

## Blockers
- explicit_final_weapon_art_review_approval: Final weapon art approval must come from docs/M95_FINAL_WEAPON_ART_REVIEW_APPROVAL.json, not renderer/material counts.
- placeholder_procedural_block_risk_cleared: Placeholder/procedural-block risk must be cleared by final review and not by contact-sheet-only evidence.

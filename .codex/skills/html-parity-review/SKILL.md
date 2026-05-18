---
name: html-parity-review
description: Use when comparing the Unity tactical prototype against the HTML/Three.js baseline and producing concrete missing-feature or file-level review notes without broad rewrites.
---

# HTML Parity Review

Baseline:

- Unity scene: `Assets/Scenes/TacticalPrototype.unity`
- HTML target: `reference/html_baseline_final_packet/index.html`

Read first:

- `README.md`
- `DEVELOPMENT_RECORD.md`
- `docs/LONG_RUNNING_TACTICAL_HTML_PARITY_TODO.md`
- `docs/TACTICAL_ACCEPTANCE_PIPELINE_REPORT.json`
- `docs/HTML_TACTICAL_PARITY_GATE.json`

Review dimensions:

- lobby/start/death/settings/skin flow;
- first/third person, ADS, crouch/prone/jump;
- fire/reload/weapon switching/hit feedback;
- pickup/inventory state mutation;
- NPC ranged combat and dynamic spawn;
- floor/ladder/building/container/warehouse traversal;
- HUD and player-camera evidence.

Output file-level, patch-oriented notes. Do not call contact sheets or fixed cameras parity completion.

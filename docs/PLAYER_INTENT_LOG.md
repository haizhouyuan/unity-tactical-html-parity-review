# Player Intent Log

Date: 2026-05-18

This log turns player, parent, reviewer, and agent feedback into structured design data.

The project started from natural-language game requests and player-view complaints. Those comments are part of the product record. They should not remain only in chat history.

## Why This Exists

Future agents need to know the difference between:

- a player fantasy;
- a gameplay blocker;
- a visual complaint;
- a toolchain failure;
- an asset-production issue;
- an external reviewer suggestion.

Without this log, agents may keep optimizing screenshots while the player still cannot play the route they care about.

## Entry Schema

Use this schema for new entries:

```yaml
intent_id: INTENT-000
date: YYYY-MM-DD
source: child | parent | reviewer | agent | playtest
raw_quote: ""
interpreted_need: ""
category: gameplay | visual | UX | bug | fantasy | reference | toolchain | asset | release
severity: blocker | high | medium | low
design_response: ""
implementation_status: todo | in_progress | shipped | rejected | deferred
linked_mission: ""
evidence_required:
  - ""
linked_reports:
  - ""
linked_screenshots:
  - ""
notes: ""
```

## Field Guidance

### `source`

Use the real source of feedback:

- `child`: direct player feedback from the child tester.
- `parent`: project owner or product framing.
- `reviewer`: human or AI reviewer feedback.
- `agent`: Codex, ChatGPT, Kimi, Gemini, MiniMax, or another coding/research agent.
- `playtest`: deterministic route or future AI playtest.

### `raw_quote`

Preserve the original wording when it is useful and safe for the public repo. If the raw wording is too private or too conversational, store a sanitized paraphrase and keep the interpreted need precise.

### `interpreted_need`

Translate feedback into an actionable development need.

Examples:

- "gun looks wrong" -> third-person weapon mount or first-person framing needs evidence.
- "cannot enter building" -> building collision, floor support, or ladder route needs a gate.
- "enemy shoots through cover" -> line-of-sight and blocker collision need proof.

### `evidence_required`

Do not leave this vague.

Good examples:

- player-camera screenshot showing weapon mount correctness;
- AI playtest route JSON proving player can enter first floor and climb;
- cover line-of-sight report proving NPC shot is blocked by container;
- asset promotion ledger entry proving gameplay binding and event participation.

## Status Meanings

- `todo`: accepted as real work, not started.
- `in_progress`: mission exists or implementation is underway.
- `shipped`: implemented and backed by required evidence.
- `rejected`: not aligned with current project constraints.
- `deferred`: valid, but later than the current phase.

## Severity Meanings

- `blocker`: prevents normal play or evidence generation.
- `high`: strongly affects player trust or core route quality.
- `medium`: important but does not block the next gate.
- `low`: polish, preference, or later-phase issue.

## Evidence Rules

Every intent needs evidence appropriate to its category.

Gameplay:

- route JSON;
- state mutation;
- player-camera screenshot or video.

Visual:

- before/after screenshot;
- semantic review where assets are involved;
- player-camera evidence, not only contact sheets.

Toolchain:

- command log;
- MCP smoke report;
- Console status;
- blocker report if not resolved.

Asset:

- import report;
- material/PBR report;
- semantic review;
- gameplay binding;
- promotion ledger entry.

## Starter Backlog

These are normalized starter intents derived from the current public review state. They are intentionally phrased as product needs, not private chat quotes.

| Intent ID | Source | Category | Severity | Interpreted need | Status | Next mission |
| --- | --- | --- | --- | --- | --- | --- |
| INTENT-001 | parent/playtest | gameplay | blocker | The Unity route must prove real building entry, valid floors, ladder movement, and cover collision. | todo | M81 Building Integrity Gate |
| INTENT-002 | parent/playtest | gameplay | high | First-person weapons must be visible, fire, reload, hit, and provide feedback comparable to the HTML baseline. | todo | M82 Weapon Feel Gate |
| INTENT-003 | parent/playtest | visual | high | Generated tactical assets must appear in the actual player route, not only in contact sheets or showcase views. | todo | M84 Asset Factory Spike |
| INTENT-004 | reviewer | toolchain | high | The active Unity session must be verified before relying on new `AI Tools/...` menu commands. | todo | M0.5 Active Unity Session Verification |
| INTENT-005 | reviewer | process | medium | Future AI tasks must use mission contracts and evidence gates instead of broad prompts. | in_progress | M0 Docs-only OS Pass |
| INTENT-006 | parent/playtest | gameplay | high | NPC line-of-sight and cover checks must include containers and other gameplay blockers, not only walls. | todo | M81 Building Integrity Gate |
| INTENT-007 | parent/reference | asset | medium | Reference-driven weapon assets must pass semantic comparison and gameplay binding before they count as progress. | todo | M84 Asset Factory Spike |

## Append New Entries Below

Use the YAML schema above or add table rows with the same fields. Do not mark an intent as `shipped` until its evidence requirements are met.

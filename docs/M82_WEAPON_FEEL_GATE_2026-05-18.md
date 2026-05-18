# M82 Weapon Feel Gate

Date: 2026-05-18

## Goal

Prove the current hero weapon from actual gameplay state, not from static contact sheets.

## Scope

This mission covers:

- first-person rifle visibility;
- ADS/readability;
- fire event with ammo mutation and enemy hit;
- muzzle flash, casing, tracer, hit marker, and SFX hooks;
- recoil/polish event evidence;
- reload state mutation;
- third-person weapon mount visibility.

## Out Of Scope

This mission does not cover:

- building/collision fixes;
- new AI-generated weapon assets;
- full animation authored reloads;
- final commercial weapon art quality.

## Expected Command

Run through community MCP or Unity Editor:

`AI Tools/Run Weapon Feel Gate`

When running the full pipeline:

`AI Tools/Run Tactical Acceptance Pipeline`

## Evidence

Primary report:

`docs/WEAPON_FEEL_GATE.json`

Expected screenshot directory:

`Assets/Screenshots/WeaponFeel/`

## Completion Standard

The mission is complete only when:

- `docs/WEAPON_FEEL_GATE.json` exists;
- `passed=true`;
- fire/reload state changes are proven;
- first-person and third-person screenshots exist;
- tactical acceptance pipeline records the weapon feel gate;
- Unity Console has no compile errors.

# M81 Building Integrity Gate

Date: 2026-05-18

## Goal

Prove that the tactical prototype building route is playable from actual runtime objects, not only from static scene screenshots.

## Scope

This mission covers:

- entering Building A through its ground-floor doorway;
- moving into the first floor without doorway collider blockage;
- using an interior ladder to reach an upper floor;
- verifying upper-floor support and roof-edge/drop structure;
- verifying that container/cover blocks enemy ranged damage.

## Out Of Scope

This mission does not cover:

- weapon feel tuning;
- visual production polish;
- new AI-generated assets;
- asset promotion;
- broad map rebuilds;
- package installation.

## Expected Command

Run through community MCP or Unity Editor:

`AI Tools/Run Building Integrity Gate`

When running the full pipeline:

`AI Tools/Run Tactical Acceptance Pipeline`

## Evidence

Primary report:

`docs/BUILDING_INTEGRITY_GATE.json`

Expected screenshot directory:

`Assets/Screenshots/BuildingIntegrity/`

## Completion Standard

The mission is complete only when:

- `docs/BUILDING_INTEGRITY_GATE.json` exists;
- `passed=true`;
- player route checks pass;
- cover blocks enemy ranged damage;
- tactical acceptance pipeline records the building integrity gate;
- Unity Console has no compile errors.


# Phase 4.1 Floating Perimeter Visual Audit

**Status: Builder source corrected; automation passed; human visual recheck pending**

Audit date: 2026-07-22 (Asia/Tokyo)
Branch: `feature/phase4-external-asset-integration`
Defect baseline: `e63cdba5096f605546290ec7b714be293c52b12d` / `phase4-assets-automation-passed`

## Root cause

The floating cyan/orange bars were not gameplay wiring. `Phase4SceneBuilder.BuildEnvironment` generated independent primitive `West Wall Lamp` and `East Wall Lamp` objects at the old perimeter coordinates. The gray diagonal/rail-like pieces came from rotating partial wall wrappers and `Column_MetalSupport` into the perimeter wall sequence without a continuous backing. Their transforms were valid, but the meshes had no visually credible attachment to the floor or another wall in the gameplay-camera composition.

## Generated object inventory

| GameObject / previous count | Prefab or source | Generation code | Intended purpose | Disposition |
|---|---|---|---|---|
| `West Wall Lamp {z}` / 14 | None; generated Cube using `PlateMaterial` | `Phase4SceneBuilder.BuildEnvironment` old lamp loop | Cyan perimeter accent | Removed from the Builder; no gameplay role |
| `East Wall Lamp {z}` / 14 | None; generated Cube using `PlayerMaterial` | Same old lamp loop | Orange perimeter accent | Removed from the Builder; no gameplay role |
| `External Wall {side}-{z}` / 6 | `P4X_Wall_03.prefab` from `WallBand_Straight.fbx` | Wall-module selection in `BuildEnvironment` | Wall variation / rail | No longer selected for the perimeter |
| `External Wall {side}-{z}` / 6 | `P4X_Wall_04.prefab` from `ShortWall_WhitePlate2_Straight.fbx` | Same selection | Partial wall variation | No longer selected for the perimeter |
| `External Wall {side}-{z}` / 6 | `P4X_Wall_05.prefab` from `ShortWall_AccentStrip_Straight.fbx` | Same selection | Accent strip | No longer selected for the perimeter |
| `External Wall {side}-{z}` / 4 | `P4X_Wall_06.prefab` from `BottomMetal_Straight.fbx` | Same selection | Bottom rail | No longer selected for the perimeter |
| `External Column {side}-{z}` / 6 | `P4X_Column_03.prefab` from `Column_MetalSupport.fbx` | Column-module selection in `BuildEnvironment` | Diagonal structural variation | No longer selected; it needs a larger authored anchor absent here |
| `External Wall {side}-{z}` / 34 current | `P4X_Wall_01` or `P4X_Wall_02` | Bounded two-variant selection | Full-height external wall cladding | Retained; moved onto the floor edge and continuous backing |
| `External Column {side}-{z}` / 20 current | `P4X_Column_01` or `P4X_Column_02` | Bounded two-variant selection | Solid wall support | Retained; floor- and wall-connected |
| `West/East Perimeter Wall Backing` / 3 each | None; generated Cube | `BuildEnvironment` before the wall rows | Continuous structural wall | Added; joins all cladding to the floor and removes black-background isolation |
| `Facility Machine Bank/Status {-1,0,1}` / 20 banks, 60 bars | External Prop wrapper plus project Cubes | `BuildMachineBank` | Local machine status | Retained; each bar intersects its machine bank, not the black background |
| `Floor Wiring` / 4 | LineRenderer, no Prefab | `P3SceneBuilder.CreateWire` | Plate/Door or Socket/Door relationship | Retained; every path starts/ends on authored floor device coordinates |
| `Door Open Edge {-1,1}` / 8 | Generated Cube on `P4 Doorway Frame` | `BuildDoor` | Door circuit/channel readability | Retained; parented and physically adjacent to the external Door frame |
| `Bulkhead Floor Guide` / 6; `Section Identity Strip` / 3; `Goal Floor Guide` / 6 | Generated floor Cubes | `BuildBulkhead`, `BuildEnvironment`, `BuildGoal` | Floor routing / section / Goal guidance | Retained; all lie on the floor or Goal root |

## Builder correction

- Deleted the independent wall-lamp generation loop rather than disabling Scene instances.
- Limited perimeter wall wrappers to `P4X_Wall_01/02`, the two full-height variants.
- Limited perimeter columns to `P4X_Column_01/02`, excluding the diagonal `Column_MetalSupport`.
- Moved wall cladding from `x=±7.08` to `x=±6.42` so its inner bounds overlap the authored floor perimeter.
- Added continuous backings at `x=±6.58`, height 3 m, for every section.
- Moved solid columns to `x=±6.24`, overlapping the floor, wall cladding, and backing.

## Regeneration and capture evidence

`P3SceneBuilder.BuildFromCommandLine` exited `0`. The dedicated regression test executes `P3SceneBuilder.BuildScene` again, then verifies:

- zero `West/East Wall Lamp` names;
- 34 external wall instances, all from `P4X_Wall_01/02`;
- 20 external column instances, all from `P4X_Column_01/02`;
- three west and three east continuous backings;
- every retained wall/column bound reaches the floor and overlaps its perimeter envelope.

The same eight camera states were regenerated at 1920x1080, Debug Overlay OFF:

- Before: `Captures/Phase4_1/FloatingVisualFix/Before` from the detached `phase4-assets-automation-passed` commit.
- After: `Captures/Phase4_1/FloatingVisualFix/After` from the corrected Builder.
- Both sets: 8/8 PNG, all 1920x1080.

## Regression results

| Gate | Result |
|---|---|
| Targeted Builder regeneration test | 1/1 passed |
| EditMode | 94/94 passed; 0 failed; 0 skipped |
| PlayMode | 93/93 passed; 0 failed; 0 skipped |
| P0/P1/P2/P3 maximum Replay Drift | `0 m` for all scenes |
| P3 automatic completion | Completed all 3 sections; 1,091 advances |
| P3 interactions | Success 4; failure 0 |
| Stable IDs / gameplay Collider preservation | Passed retained EditMode and PlayMode coverage |
| Presentation colliders | 0 added; retained tests passed |
| P0/P1/P2 Scene SHA-256 | Unchanged from validated hashes |

The automated capture inspection confirms the listed orphan generators are absent and retained perimeter elements have an authored connection. Final appearance still requires the human Visual Review; this audit does not authorize `phase4-validated`.

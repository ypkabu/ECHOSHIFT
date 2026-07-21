# Phase 4 Visual Polish Automation Validation

**Status: Historical first-pass automation record — human Visual Review rejected the primitive-led presentation**

This document preserves the `phase4-automation-passed` evidence at `e043b160a945939d5f862a4648d3df728a40a49e`. It is not the current acceptance candidate. Corrective external-asset integration and its 93/93 + 93/93 verification, rebuilt Players, captures, performance, and shutdown matrix are recorded separately in `Phase4ExternalAssetIntegrationValidation.md`. Current status is **Phase 4.1: Automation Passed; Human Visual Review Pending**.

Validation date: 2026-07-21 (Asia/Tokyo)  
Branch: `feature/phase4-visual-polish`  
Baseline: Phase 3 `86cdf46635b0de8df3c0adf599a670d903941fe8` / `phase3-validated`  
Unity Editor: `6000.4.6f1`  
URP: `17.4.0`

## Scope delivered

- P3-only modular facility composition with floor/wall panels, columns, corners, overhead rails, device plinths, machine banks, section boundaries, conduits, Door frames, and Goal portals.
- Compound Player/Echo experiment suits with readable front/back silhouette, carry pose, trails, stopped state, and three cyclic non-color generation marks.
- Dedicated Plate, Door, Battery, Socket, and Goal visual adapters driven by existing authoritative gameplay state.
- One global safe URP Volume, one shadowed Directional Light, and at most two unshadowed local lights per active section.
- Bounded pooled VFX and generated Audio feedback, including Echo creation/removal, Loop completion, interaction, Battery, Door, section/game completion, movement, and UI cues.
- HUD hierarchy/icon/progress treatment and cached text updates with three-resolution layout validation.
- Reproducible Scene generation, capture pipeline, build pipeline, and maximum-Echo performance probe.

No new puzzle system, solution, Stable ID, Replay format, fixed-tick rule, collider shape, or P0-P2 Scene content was introduced.

## Japanese font packaging

Noto Sans JP was obtained from the official Google Fonts repository family directory (`ofl/notosansjp`). The upstream `OFL.txt` identifies the SIL Open Font License 1.1. The source font, a generated static Regular instance, and `ThirdPartyNotices/NotoSansJP-OFL.txt` are stored locally; no font binary is attached to reports.

- Runtime OS-font creation: absent.
- Legacy UI/TextMesh path: packaged `NotoSansJP-Regular.ttf`.
- TextMeshPro path: static `NotoSansJP_Phase4` atlas, 194 glyphs, catalog coverage true.
- TMP Essential Resources/fallback infrastructure: packaged project assets.
- Standalone marker: `font=Noto Sans JP (Packaged);glyphs=True`.

Official source references:

- <https://github.com/google/fonts/tree/main/ofl/notosansjp>
- <https://github.com/google/fonts/blob/main/ofl/notosansjp/OFL.txt>

## Scene Builder and immutable regressions

`EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine` completed with process exit code `0`. It rebuilt canonical Phase 4 assets and P3 without duplicate materials, fonts, prefabs, or Build Settings entries. P0-P2 hashes stayed:

| Scene | SHA-256 |
|---|---|
| `P0_ReplayLab.unity` | `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5` |
| `P1_InteractionLab.unity` | `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB` |
| `P2_CoordinationLab.unity` | `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C` |

Build Settings remain P3, P2, P1, P0. Compiler warnings/errors and serialized Missing Script/Reference matches were zero.

## Automated tests

Final XML results from the current source tree:

| Suite | Total | Passed | Failed | Skipped | Duration |
|---|---:|---:|---:|---:|---:|
| EditMode | 84 | 84 | 0 | 0 | 8.059572 s |
| PlayMode | 87 | 87 | 0 | 0 | 19.2221506 s |

Phase 4 contributes 23 EditMode and 26 PlayMode cases; all retained P0-P3 cases remain enabled. Real-Scene integration reported:

- P0 maximum Replay Drift: `0 m`.
- P1 maximum Replay Drift: `0 m`; interaction success `2`, failure `0`.
- P2 maximum Replay Drift: `0 m`; interaction success `2`, failure `0`; Goal true.
- P3 maximum Replay Drift: `0 m`; interaction success `4`, failure `0`; all three sections completed.
- Missing Component/Reference, NullReference, unhandled Exception, compiler warning: `0` matched in the final logs.

## Performance

The final Development Player ran at 1920x1080 on `NVIDIA GeForce RTX 5070 Laptop GPU`, D3D11, with a non-Null graphical device. The probe used the maximum three-Echo condition, a 120fps cap, separate Recorder/steady warmups, three Loop transitions, and 600 steady frames.

| Metric | Result |
|---|---:|
| Average frame time | 8.339 ms |
| p95 frame time | 8.359 ms |
| Maximum steady frame time | 8.581 ms |
| Main Thread average | 8.335 ms |
| Main Thread maximum | 8.596 ms |
| Loop Transition maximum frame | 14.093 ms |
| Loop Transition Main Thread maximum | 14.074 ms |
| Steady GC maximum / total | 0 B / 0 B |
| Non-zero GC frames | 0 / 600 |
| Maximum used memory | 105,811,560 B |
| Final / required Echoes | 3 / 3 |

The Draw Calls recorder was unavailable. The SetPass recorder was valid but returned `0`; this value is recorded without inference. Interactive Frame Debugger/Profiler confirmation is deferred to the human review.

## Screenshot capture

`Phase4CapturePipeline.CaptureAllFromCommandLine` exited `0` and generated eight non-empty PNGs under ignored `Captures/Phase4/`. Every file is 1920x1080. Debug Overlay is off; gameplay HUD is hidden in frames 1-7 and the Japanese HUD/Pause presentation is visible in frame 8. Automated inspection confirmed file count, dimensions, sizes, and hashes; visual judgment remains manual.

## Standalone Build and startup

`Phase4BuildPipeline.BuildWindowsDevelopment` completed with process exit code `0`:

- BuildReport: Success, warnings `0`, errors `0`, reported size `178,650,134 B`.
- Output: `Builds/Phase4/ECHOSHIFT_Phase4.exe`, Data folder, Mono runtime, UnityPlayer, and required libraries.
- Distribution: 291 files, `178,845,266 B` total.
- EXE SHA-256: `098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938` (Unity bootstrap executable; content changes are in Data files).
- Startup batch probe: P3 Playing, `ja-JP`, packaged font, glyphs true, HUD true, telemetry JSON saved, quit requested, cleanup completed, process exit `0`.
- Missing/Null/unhandled/Font Glyph matches: `0`.

A normal visible automated Player exit returned Windows status `0xC0000005` after the log reached clean Unity shutdown. The unchanged validated Phase 3 Development Build reproduces the same host behavior, and D3D11/D3D12 selection does not change it. This is not attributed to Phase 4 code. The supported batch probe exits `0`; ordinary visible Quit must be repeated during the Phase 4 human review and on a second Windows environment before release packaging.

## Problems found and corrected

- New Scene creation invalidated loaded visual asset references: assets are reloaded after Scene creation.
- TMP Essential Resources and shader/settings references were absent: official package resources were imported and the atlas made static.
- Existing TMP atlas data could be cleared during Build: clear-on-build was disabled and glyph coverage is tested.
- URP Volume overrides serialized with invalid subasset references: overrides are added explicitly as profile subassets.
- Capture startup exposure/HUD staging was inconsistent: camera warmup and per-frame HUD visibility were made deterministic.
- Performance preparation measured one Echo instead of the maximum three: the probe now waits for each transition and verifies `3/3` before sampling.
- Door/Echo-removal/Goal feedback paths were incomplete: bounded scene-owned notifications and de-duplication were added.
- Echo generation 4 clamped onto generation 3 visuals after eviction: visual slots now cycle modulo three.
- Feedback runtime buffers were not rebuilt after Scene deserialization: `Awake` reconstructs them.
- HUD formatted unchanged values each frame: values are cached and timer strings precomputed, reducing steady GC from 194 B/frame to 0 B/frame.

## Historical gate decision

The original automation criteria were green, but later human Visual Review found presentation-level High issues. The immutable `phase4-automation-passed` tag remains historical. Phase 4.1 corrected those findings and passed a fresh automation gate; `phase4-validated` still must not be created until `Phase4VisualAcceptance.md` is completed against the new Build and any Critical/High finding is corrected, rebuilt, and fully revalidated.

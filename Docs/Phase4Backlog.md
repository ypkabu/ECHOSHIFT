# Phase 4 Visual Review and Later Backlog

The first primitive-led Phase 4 pass failed Visual Review. Phase 4.1 integrated a curated CC0 external-asset subset and passed full automation; this file now contains human-judgment items and later production work. **Phase 4.1: Automation Passed; Human Visual Review Pending.**

## Visual Polish / Art Direction

### Addressed by automation: playable-greybox presentation looks too simple

Human acceptance found the whole game visually plain and inexpensive-looking compared with a finished release. The observation does not block Phase 3 because the milestone intentionally delivers a Playable Greybox and all gameplay, comprehension, control, Japanese-display, and completion checks passed.

The corrective automated slice now includes:

- curated Quaternius modular facility floors, walls, columns, supports, props, Door frames, and Goal portal;
- a static Quaternius Robot visual wrapper for Player/Echo with visor, carry pose, trail, cyclic generation color, and compact non-color generation marks;
- dedicated Door, PressurePlate, Battery, PowerSocket, and Goal forms with emissive wiring;
- restrained URP lighting, Bloom, Vignette, Color Adjustments, and ACES Tonemapping;
- pooled event feedback and generated bounded Audio cues;
- a panel/icon/progress-based HUD and packaged Japanese font;
- automated 1920x1080 capture and measured performance gates.

Human review must determine whether the result is sufficiently above greybox quality for portfolio footage. A Fail does not invalidate Phase 3 gameplay, but Critical/High findings block `phase4-validated`.

## Source-blind pretest observations

These were not reproduced as blocking human-acceptance issues, but remain useful polish candidates:

- Medium: show delayed Echo replay progress more clearly so it is not mistaken for failed recorded Interaction.
- Resolved in Phase 4.1: persistent world labels were removed instead of merely reducing overlap.
- Resolved in Phase 4.1: Pause presentation is one centered card with the gameplay HUD hidden.

## Later art-production backlog

- Replace or further art-direct the curated external meshes only if the Phase 4.1 human review still finds them below the intended portfolio bar; preserve collider and Stable-ID roots.
- Add UVs, authored textures/decals, normal maps, richer material variation, and a final lightmap pass.
- Add production character locomotion, interaction animation, Echo playback posing, and optional IK.
- Add final BGM, ambience, authored sound design, mixing, accessibility volume controls, and platform device testing.
- Add production UI motion, final iconography, localization layout review, and ultrawide-specific art framing.
- Produce trailer shots only after the Phase 4 human checklist passes and any Critical/High issue is rebuilt and retested.

## Technical follow-up

- Interactive Frame Debugger/Profiler capture is required because Draw Calls was unavailable and GPU Frame Time, SetPass, Triangles, and Vertices returned non-authoritative `0` values.
- Inspect both Robot feet in motion: the vendor FBX importer removes one self-intersecting `Foot.L` polygon, although no automatic visible/Build defect was observed.
- Recheck ordinary visible Quit on the human-review machine. Phase 4.1's nine-scenario shutdown matrix exited `0`, but the historical Phase 3/Phase 4 `UnityPlayer.dll` `0xC0000005` root cause remains unproven.
- Judge black-void balance, external-kit cohesion, audio comfort, non-16:9 HUD/framing, and Player/Echo readability in motion rather than from still captures alone.

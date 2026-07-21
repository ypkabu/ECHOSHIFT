# Phase 4 Visual Review and Later Backlog

Phase 4 automation implemented the first Visual Polish vertical slice. This file now contains work that is intentionally outside that automated slice or requires human visual/audio judgment. **Phase 4: Automation Passed; Human Visual Review Pending.**

## Visual Polish / Art Direction

### Addressed by automation: playable-greybox presentation looks too simple

Human acceptance found the whole game visually plain and inexpensive-looking compared with a finished release. The observation does not block Phase 3 because the milestone intentionally delivers a Playable Greybox and all gameplay, comprehension, control, Japanese-display, and completion checks passed.

The automated slice now includes:

- modular facility panels, trim, columns, overhead structure, device plinths, machine banks, section boundaries, and Goal portal;
- compound low-poly Player/Echo suits with visor, limbs, carry pose, trail, cyclic generation color, and non-color generation marks;
- dedicated Door, PressurePlate, Battery, PowerSocket, and Goal forms with emissive wiring;
- restrained URP lighting, Bloom, Vignette, Color Adjustments, and ACES Tonemapping;
- pooled event feedback and generated bounded Audio cues;
- a panel/icon/progress-based HUD and packaged Japanese font;
- automated 1920x1080 capture and measured performance gates.

Human review must determine whether the result is sufficiently above greybox quality for portfolio footage. A Fail does not invalidate Phase 3 gameplay, but Critical/High findings block `phase4-validated`.

## Source-blind pretest observations

These were not reproduced as blocking human-acceptance issues, but remain useful polish candidates:

- Medium: show delayed Echo replay progress more clearly so it is not mistaken for failed recorded Interaction.
- Low: reduce world-label overlap when Player, Echo, Battery, and Socket cluster.
- Low: remove the faint duplicate Pause title caused by overlapping paused-state presentation.

## Later art-production backlog

- Replace generated primitives with production-authored environment and character meshes while preserving collider and Stable-ID roots.
- Add UVs, authored textures/decals, normal maps, richer material variation, and a final lightmap pass.
- Add production character locomotion, interaction animation, Echo playback posing, and optional IK.
- Add final BGM, ambience, authored sound design, mixing, accessibility volume controls, and platform device testing.
- Add production UI motion, final iconography, localization layout review, and ultrawide-specific art framing.
- Produce trailer shots only after the Phase 4 human checklist passes and any Critical/High issue is rebuilt and retested.

## Technical follow-up

- Interactive Frame Debugger/Profiler capture is required because the runtime Draw Calls counter was unavailable and SetPass returned `0` on the automated configuration.
- On this machine, automated normal-window shutdown returns `0xC0000005` after clean Unity cleanup for both Phase 3 and Phase 4 Development Builds. Batchmode exits `0`. Recheck ordinary visible Quit during human review and on a second Windows machine before release packaging.

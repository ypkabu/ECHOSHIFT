# Phase 4 and Later Backlog

This document records non-blocking observations deferred after **Phase 3: Validated**. It is a backlog only; no Phase 4 implementation or scope commitment is started by this document.

## Visual Polish / Art Direction

### Medium: playable-greybox presentation looks too simple for a finished product

Human acceptance found the whole game visually plain and inexpensive-looking compared with a finished release. The observation does not block Phase 3 because the milestone intentionally delivers a Playable Greybox and all gameplay, comprehension, control, Japanese-display, and completion checks passed.

Minimum future exploration scope:

- improve Environment shapes, modular composition, and materials;
- replace or refine Player and Echo models, silhouettes, and generation readability;
- refine Door, PressurePlate, Battery, and PowerSocket modelling;
- establish a coherent lighting direction;
- add purposeful VFX for interaction, Echo replay, devices, Doors, and completion;
- redesign the HUD hierarchy and finish its visual language;
- improve Loop and Section transition presentation;
- unify color, material response, texture quality, contrast, and the overall screen composition.

Replacement conditions: preserve Player/Echo/device readability, Japanese legibility, deterministic simulation boundaries, collider behavior, Stable IDs, and recorded-interaction tests while replacing greybox presentation. Art work must not move gameplay state into presentation components.

## Source-blind pretest observations

These were not reproduced as blocking human-acceptance issues, but remain useful polish candidates:

- Medium: show delayed Echo replay progress more clearly so it is not mistaken for failed recorded Interaction.
- Low: reduce world-label overlap when Player, Echo, Battery, and Socket cluster.
- Low: remove the faint duplicate Pause title caused by overlapping paused-state presentation.

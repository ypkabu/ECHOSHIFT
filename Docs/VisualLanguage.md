# Phase 4 Visual Language

**Phase 4: Automation Passed; Human Visual Review Pending**

## Color channels

| Meaning | Primary color | Non-color reinforcement |
|---|---|---|
| Environment | warm white / charcoal / black | panel scale, recess, frame, overhead rail |
| Player | white body / amber core | visor direction, circular foot ring, full compound suit |
| Echo slot 1 | cyan | one head/chest mark, `E1`, trail |
| Echo slot 2 | violet | two marks, `E2`, trail |
| Echo slot 3 | blue-green | three marks, `E3`, trail |
| PressurePlate circuit | cyan | stepped pad, single conduit/badge |
| Battery circuit | orange | directional fins, keyed Socket, double conduit/badge |
| Goal / success | pale green-white | tall portal, beam, expanding pulse |
| Failure / blocked | red | cross/flash pulse and failure sound |

After the oldest Replay is evicted, generation IDs continue increasing for deterministic history, while visual slots cycle modulo three. The maximum three visible Echoes therefore retain distinct color/material/mark combinations without changing gameplay generation identity.

## State communication

- Echo playback: trail emits and cool core stays bright; stopped playback removes trail, reduces scale slightly, and dims the core.
- Plate: the pad moves down and emission changes; wiring retains its cyan identity.
- Door: the framed moving panel changes position and badge/emission; Plate and Socket variants also use different channel marks.
- Battery: the core pulses on the floor, the carry pose lifts it at the visible arm socket, and insertion aligns it to the keyed Socket.
- Socket: empty orange ring becomes a bright powered ring; the orange conduit leads to its Door.
- Goal: portal silhouette is always visible; completion uses a bounded pale pulse and cue.
- Interaction: success is a short white/cyan pulse; failure is red plus a separate sound and HUD reason.

## UI language

The HUD uses dark translucent panels, compact reusable icons, stable screen-edge padding, a restrained progress strip, and high-contrast Japanese text. Objectives are subordinate to moment-to-moment Loop/Timer/Echo state. The center and walkable route stay uncovered. Layout validation covers 1280x720, 1920x1080, and 2560x1440; non-16:9 usability remains part of manual acceptance.

## Resource limits

- Shared URP/Lit materials and `MaterialPropertyBlock`; no runtime material instances.
- Fixed feedback pool of 12 ParticleSystems and eight reusable AudioSources.
- One shadowed Directional Light and at most two unshadowed local lights per active section.
- Static packaged TMP atlas for the current Japanese catalog; no OS font lookup or runtime atlas growth.

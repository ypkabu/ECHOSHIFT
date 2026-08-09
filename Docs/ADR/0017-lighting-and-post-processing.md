# ADR 0017: Restrained URP Lighting and Post-processing

## Status

Accepted for Phase 4 automation; visual judgment pending.

## Decision

Use one soft-shadow Directional Light, no more than two unshadowed local lights per active section, Trilight ambient/fog separation, and one global URP Volume. The Volume contains ACES Tonemapping, mild Color Adjustments, thresholded Bloom, and subtle Vignette. Motion Blur, Chromatic Aberration, and gameplay Depth of Field are absent.

## Reasons

- Actor/device contrast needs real lighting; emission alone does not illuminate or define form.
- A fixed light budget prevents modular detail from multiplying realtime-light cost.
- Mild post-processing unifies the palette while retaining Echo generation and Japanese HUD readability.
- Scene Builder ownership makes settings repeatable and testable.

## Alternatives considered

- Fully emissive/unlit presentation: rejected for flat depth and weak silhouettes.
- Many realtime point/spot lights: rejected for GPU and shadow cost.
- Heavy cinematic Bloom, DOF, Motion Blur, or aberration: rejected because they obscure puzzle targets and UI.
- Full baked-lighting pipeline: deferred because the generated slice changes frequently and does not need a production lightmap yet.

## Current limitations

Automated screenshots and safe-range tests cannot judge subjective exposure, black crush, glare, or monitor variation. Runtime Draw Calls were unavailable and SetPass returned zero on the automated recorder.

## Replacement conditions

Human review or target-hardware profiling may tune values within the documented language. Any new Volume override, shadowed light, lightmap, or renderer feature requires three-resolution screenshots, performance capture, and P0-P3 regression.

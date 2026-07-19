# ADR 0013: Runtime UI and Visual Feedback

## Status

Accepted for Phase 3 automation; subject to manual acceptance.

## Decision

Use built-in uGUI, a centralized `Phase3TextCatalog`, configurable damped camera follow, temporary URP materials, line renderers, Echo trails, and MaterialPropertyBlocks. HUD shows section, objective, loop/timer/Echo count, current input-device prompt, carry state, transition, and reason-specific interaction failure. F3 hides the diagnostic overlay. No Cinemachine or third-party presentation package is added.

## Reasons

This gives the greybox a readable, replaceable presentation layer without changing deterministic gameplay or allocating cloned materials during feedback updates.

## Alternatives considered

- Cinemachine: not installed and unnecessary for one top-down follow camera.
- UI Toolkit runtime conversion: rejected to avoid an unrelated migration.
- Hard-coded strings in components: rejected because audit and future localization would be fragmented.

## Current constraints

Visual clarity, camera comfort, control feel, monitor scaling, and tutorial comprehension remain human acceptance items. Audio, final VFX, accessibility settings, the Unity Localization package, multilingual runtime switching, and production localization workflow are outside scope. The Phase 3 Japanese pretest uses a centralized serialized `ja-JP` catalog and an installed OS Japanese font without adding those systems.

## Replacement conditions

Replace temporary assets and layout when an art/UI milestone supplies production style, accessibility, multilingual localization, or camera requirements.

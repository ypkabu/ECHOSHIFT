# ADR 0016: Packaged Japanese Font and Static TMP Atlas

## Status

Accepted.

## Decision

Package Noto Sans JP from the official Google Fonts OFL family source. Retain the upstream license in `ThirdPartyNotices`, generate a static Regular instance, create one canonical static TextMeshPro atlas for the current Japanese catalog, and use the packaged legacy `Font` for existing Unity UI/TextMesh components. Runtime OS font lookup and dynamic atlas growth are prohibited.

## Reasons

- Steam distribution must not depend on an installed system font.
- SIL Open Font License 1.1 permits bundling with software when its notice accompanies the font.
- A static catalog atlas makes missing glyphs and Build-time clearing deterministic.
- The dual TMP/legacy boundary avoids a risky full UI rewrite while removing OS dependence.

## Alternatives considered

- `Font.CreateDynamicFontFromOSFont`: rejected because target-machine availability is not guaranteed.
- Dynamic TMP atlas: rejected for runtime allocation, glyph warning, and Build reproducibility risk.
- Convert every legacy UI element to TMP in Phase 4: deferred to avoid changing validated Pause/navigation/layout behavior.
- Unknown third-party Japanese font: rejected for licensing uncertainty.

## Current limitations

The atlas covers the current catalog, not arbitrary future dialogue or user text. Source plus static instance increases project size. Official TMP Essential Resources are included.

## Replacement conditions

Regenerate the canonical atlas when localized catalog characters change. A later full TMP migration may remove the legacy font path only after all Japanese layouts, Pause navigation, captures, and Standalone glyph tests pass.

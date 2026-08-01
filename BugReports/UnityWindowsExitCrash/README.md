# Unity Windows graphical shutdown crash package

This folder contains a source-only staged Unity project and curated evidence for a Unity Bug Report. Generated `Builds`, `Library`, raw `Logs`, crash archives, symbols, PDBs, and dumps are intentionally excluded from Git.

## Current classification

The product and an earlier simple minimal Player produced a repeatable access violation in Unity's native graphical shutdown path. The staged A-I project in this package produced one access violation in Stage B during its initial five-run set, but no Stage B dump was captured. Stage B is therefore only the smallest observed candidate, not a stable reproducer and not proof that URP is the direct cause.

Confirmed wording for the project record:

> 画面付きWindows PlayerのUnity Native Shutdown Pathで発生する再現性の高いエンジン側不具合。ゲームコードが発火条件となる可能性は完全には否定しないが、Access Violation本体はUnityPlayer.dllのNative Cleanup内で発生する

## Contents

- `MinimalRepro/`: staged source project for Unity 6000.4.12f1.
- `ReproductionSteps.md`: build and run instructions.
- `Evidence/StageMatrix.md`: A-I and Stage B follow-up results.
- `Evidence/VersionAndBuildMatrix.md`: Unity, graphics, Development, and build hashes.
- `Evidence/SymbolicatedStack.md`: confirmed product/earlier-minimal dump analysis and the explicit Stage B limitation.
- `Evidence/DumpHashes.md`: hashes only; dump files are not included.
- `Evidence/PlayerLogExcerpts.md`: curated shutdown excerpts; raw logs are excluded.
- `Evidence/EventViewer.md`: retained event facts and the Stage B limitation.
- `Evidence/SystemInformation.md`: tested host and tool versions.
- `Evidence/ProjectSettingsComparison.md`: staged setting/package deltas.

## Submission caveat

The checked-in Stage B `manifest.json` and `packages-lock.json` are the default submission state because B is the smallest staged configuration in which an access violation was observed. It reproduced `1/5` in the initial unmonitored window-close set, `0/20` under ProcDump, and `0/30` with user-level WER LocalDumps enabled. Its stack has not been confirmed against the product dump. If Unity cannot reproduce Stage B, use the version/build matrices and supplied stack evidence to correlate the native cleanup failure; do not infer a URP regression from Stage B alone.

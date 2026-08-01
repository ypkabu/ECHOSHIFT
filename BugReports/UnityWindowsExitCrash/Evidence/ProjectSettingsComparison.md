# Package and Project Settings comparison

## Package stages

The default tracked state is Stage B. `Packages/manifest.json` contains URP `17.4.0`; `packages-lock.json` was freshly resolved by Unity 6000.4.12f1. The A-I manifest snapshots are retained in `StageDefinitions`.

| Stage | Explicit package delta |
| --- | --- |
| A | none |
| B | `com.unity.render-pipelines.universal` 17.4.0 |
| C | `com.unity.inputsystem` 1.19.0 |
| D | `com.unity.ugui` 2.0.0; minimal TMP Settings supplied before import |
| E | no package delta; adds Volume component |
| F | `com.unity.modules.audio` 1.0.0 |
| G | no package delta; changes Player settings |
| H | no package delta; adds project-style quit component |
| I | `com.unity.modules.physics` 1.0.0; adds one visual/collider |

## Product package baseline

- URP 17.4.0
- Input System 1.19.0
- uGUI/TMP 2.0.0
- Visual Studio Editor 2.0.22 in the original 6000.4.6f1 project; test copies updated it to 2.0.27 without changing runtime packages

## Player Settings

The product values recorded during investigation were company `Echo Shift Prototype`, product `ECHO SHIFT`, 1920x1080, windowed, run-in-background off, resizable window off, flip-model swapchain on, Mono, and Input System handling. Stage G applies the user-visible Windows values through the builder. The serialized active-input-handler value was not independently re-read after Stage G and is therefore not claimed as confirmed-identical.

The Stage A-B comparison changes URP package/pipeline assets only in the builder. Because B reproduced only once and C-I did not increase monotonically, that delta is a candidate boundary, not a causal URP finding.

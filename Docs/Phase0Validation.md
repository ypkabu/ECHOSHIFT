# Phase 0 Validation

## Validation status

Phase 0 final audit passed on 2026-07-19. Phase 1 work was not started.

## Toolchain

- Unity Editor: `6000.4.6f1`
- Editor executable: `C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe`
- Universal Render Pipeline: `17.4.0`
- Unity Test Framework: `1.6.0`
- Target: Windows x86_64 Development Build

The Editor version was read from the installed executable with `Unity.exe -version`. Package versions are direct dependencies in `Unity/Packages/manifest.json` and resolved in `packages-lock.json`.

## Scene Builder

- Method: `EchoShift.Editor.P0SceneBuilder.BuildFromCommandLine`
- Result: exit code `0`
- Scene: `Assets/_Project/Scenes/P0_ReplayLab.unity`
- Build Settings: one enabled scene, `P0_ReplayLab`
- Reproduced layers: Player, Environment, Echo, InteractionTrigger
- Reproduced collision rules: Player/Echo and Echo/Echo ignored; both actors collide with Environment and interact with InteractionTrigger
- Final log: `Logs/scene-builder-final-audit.log`

The final builder log has no matched C# compiler warning/error, unhandled exception, missing script, or missing reference. The log is generated validation evidence and is intentionally ignored by Git.

## Automated tests

| Suite | Total | Passed | Failed | Skipped | Process exit |
| --- | ---: | ---: | ---: | ---: | ---: |
| EditMode | 12 | 12 | 0 | 0 | 0 |
| PlayMode | 12 | 12 | 0 | 0 | 0 |

EditMode covers frame ordering, immutability, the 600-frame capacity, overflow, invalid settings, and finalization of a short recording prefix.

PlayMode covers reset restoration, replay drift, short replay termination, multi-actor and stale-actor plate management, loop transitions, Echo rewinding, the three-Echo cap and oldest-Echo eviction, collision-layer separation, closed-Door and boundary blocking, actor trigger detection, and the generated-scene flow.

## Generated-scene integration

The integration test loaded the generated `P0_ReplayLab` through Build Settings and verified:

- the active scene name and absence of missing components;
- valid required references for LoopDirector, PlayerSimulation, DoorController, ResetRegistry, and the debug overlay;
- Player, PressurePlate, DoorController, GoalVolume, and LoopDirector presence;
- loop completion and Echo creation;
- Player and Echo pressure-plate activation;
- Echo detection by GoalVolume without granting player completion;
- Door opening while the Echo holds the plate;
- current-player GoalVolume completion;
- no unexpected error, assert, or exception log.

Maximum replay drift in the final generated-scene integration run was `0 m`, against the unchanged `0.05 m` tolerance.

## Standalone build

- Method: `EchoShift.Editor.Phase0BuildPipeline.BuildWindowsDevelopment`
- Output: `Builds/Phase0/ECHOSHIFT_Phase0.exe`
- Unity target: `StandaloneWindows64`
- Options: `BuildOptions.Development`
- Process exit: `0`
- BuildReport result: succeeded
- BuildReport warnings: `0`
- Reported total output size: `164,072,932` bytes
- PE machine: `0x8664` (AMD64)
- EXE: present, `667,648` bytes
- Data folder: present, 251 files at audit time
- UnityPlayer.dll and UnityCrashHandler64.exe: present
- Build scene: only `P0_ReplayLab`

The build log has no matched C# compiler error/warning, BuildFailedException, missing script/reference, or unhandled exception.

## Standalone startup

The generated Player was launched with `-batchmode -nographics` and an explicit log file. It initialized Unity `6000.4.6f1`, the Null graphics device, and PhysX, loaded the startup content, and remained running. The audit terminated that exact Player process after ten seconds; therefore there is no natural application exit-code claim.

`Logs/standalone-player-final.log` has no matched unhandled exception, assertion failure, MissingReferenceException, NullReferenceException, missing script, missing reference, or crash marker.

## Git evidence policy

`TestResults/*.xml`, `Logs/`, `Builds/`, and `Unity/Library/` remain ignored. Test XML and logs are reproducible, run-specific evidence containing timing and machine-local paths; committing them would create noisy diffs without becoming a stable project input. The pass counts and material evidence are recorded in this tracked document instead.

## Remaining Phase 0 constraints

- Replay data is in memory only and drift is measured without correction.
- Character motion uses deterministic Transform movement and capsule casts, not general dynamic-physics rewind.
- Reset is limited to explicitly registered Phase 0 components.
- Input is keyboard-only; gamepad and the Input System workflow are deferred.
- The build is an unsigned validation build, not a Steam-ready package or installer.

## Manual validation still required

- Launch the graphical Standalone Player without `-nographics` and inspect camera framing, materials, transparency, overlay legibility, and door motion.
- Exercise WASD and R on physical keyboard input and judge movement feel and manual early-loop termination.
- Complete the intended two-loop solution manually and observe the three-Echo presentation.
- Verify packaging/signing/installer behavior only when a distribution workflow is defined; it is outside Phase 0.

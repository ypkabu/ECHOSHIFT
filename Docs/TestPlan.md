# Test Plan

## EditMode

- Frames remain in increasing tick order.
- Finalized recordings cannot be mutated through the recorder.
- Recorder capacity equals the configured 600 ticks.
- Overflow is rejected without resizing.
- A short recording finalizes only its recorded prefix and remains immutable.
- Invalid tick rate, loop duration, Echo limit, speed, or drift tolerance is rejected.

## PlayMode

- A resettable transform restores its captured position and rotation.
- A finalized player recording is handed to a newly created Echo during a loop transition.
- Echo playback reaches the recorded final pose within `0.05 m` in an unobstructed test.
- A pressure plate remains pressed until its last overlapping actor leaves.
- Existing Echo playback indices return to zero when a new loop starts.
- Player/Echo and Echo/Echo contacts are disabled while both actors remain blocked by a closed door and room boundary.
- Both actor kinds are detected by pressure-plate and goal triggers.
- Disabled, deactivated, destroyed, reset, and evicted actors cannot leave a pressure plate latched.
- A short manual loop replays only recorded frames and holds its final pose without further drift measurements.
- The generated `P0_ReplayLab` scene has no missing component, loads its required references, and completes the Echo/plate/door/goal flow without unexpected exceptions.

## Batch commands

From the repository root in PowerShell:

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P0SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\scene-builder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\Logs\editmode.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\Logs\playmode.log"
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase0BuildPipeline.BuildWindowsDevelopment -logFile "$PWD\Logs\standalone-build.log"
```

Review XML results and logs. A process exit code alone is not sufficient evidence. Do not add `-quit` to the test commands: the Test Framework exits the process after writing results, while an early generic quit can occur before the runner starts.

## Latest verified results

Executed with Unity `6000.4.6f1` on 2026-07-19:

- EditMode: 12 passed, 0 failed, 0 skipped.
- PlayMode: 12 passed, 0 failed, 0 skipped.
- The PlayMode suite includes a complete scripted two-loop solution in the generated scene and measured `0 m` maximum replay drift in the final run.
- Windows x86_64 Development Build: exit code `0`, BuildReport succeeded, zero warnings, and the expected EXE/Data output was generated.
- Headless Standalone smoke: initialized Unity and PhysX, ran for ten seconds, then was terminated by the audit; no matched unhandled exception or missing-reference error.
- `Logs/scene-builder-final-audit.log`, `Logs/editmode-final.log`, `Logs/playmode-final-2.log`, `Logs/standalone-build.log`, and `Logs/standalone-player-final.log` contain no matched compiler error/warning, unhandled exception, missing script/reference, or final test failure.

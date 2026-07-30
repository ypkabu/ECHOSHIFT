using System;
using System.Collections;
using System.IO;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Replay;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoShift.Presentation
{
    public sealed class Phase4PresentationProbe : MonoBehaviour
    {
        private const string CommandLineFlag = "-phase43PresentationProbe";
        private const float MinimumDurationSeconds = 30f;
        private const int RequiredFrames = 9;
        private const int SimulationTicksPerPhysicsFrame = 10;
        [SerializeField] private SectionTransitionCoordinator coordinator;

        private readonly float[] _timestamps = new float[RequiredFrames];
        private readonly bool[] _captured = new bool[RequiredFrames];
        private readonly string[] _fileNames =
        {
            "01_player_idle.png", "02_player_move.png", "03_echo_spawn.png",
            "04_echo_move.png", "05_battery_pickup.png", "06_battery_carry_move.png",
            "07_socket_insert.png", "08_door_open.png", "09_echo_stopped.png"
        };
        private ProfilerRecorder _animatorUpdate;
        private ProfilerRecorder _doorUpdate;
        private float _animatorMaximumMs;
        private float _doorMaximumMs;
        private float _carryMaximumFrameMs;
        private float _doorAnimationMaximumFrameMs;
        private string _directory;
        private bool _captureRequestedThisFrame;
        private Phase4RobotPoseController[] _poses = Array.Empty<Phase4RobotPoseController>();
        private DoorVisualFeedback[] _doors = Array.Empty<DoorVisualFeedback>();
        private CarryableBattery[] _sectionBatteries = Array.Empty<CarryableBattery>();
        private PowerSocket[] _sectionSockets = Array.Empty<PowerSocket>();
        private DoorController[] _sectionDoors = Array.Empty<DoorController>();
        private PressurePlate[] _sectionPlates = Array.Empty<PressurePlate>();

        [Serializable]
        private sealed class Result
        {
            public string unityVersion;
            public string graphicsDevice;
            public string graphicsApi;
            public float durationSeconds;
            public float animatorMaximumMs;
            public float doorVisualMaximumMs;
            public float carryPoseMaximumFrameMs;
            public float doorAnimationMaximumFrameMs;
            public string[] scenes;
            public float[] timestamps;
        }

        public void Configure(SectionTransitionCoordinator sectionCoordinator) =>
            coordinator = sectionCoordinator;

        private void Start()
        {
            if (!HasFlag(CommandLineFlag))
            {
                enabled = false;
                return;
            }
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D11)
            {
                Fail($"GraphicsApi={SystemInfo.graphicsDeviceType}");
                yield break;
            }
            Application.runInBackground = true;
            Screen.SetResolution(1920, 1080, false);
            Cursor.visible = false;
            Phase0DebugOverlay[] overlays = FindObjectsByType<Phase0DebugOverlay>(
                FindObjectsInactive.Include);
            for (int i = 0; i < overlays.Length; i++) overlays[i].SetVisible(false);
            _directory = Path.Combine(Application.persistentDataPath, "Phase4PresentationProbe");
            Directory.CreateDirectory(_directory);
            for (int i = 0; i < _fileNames.Length; i++)
            {
                string old = Path.Combine(_directory, _fileNames[i]);
                if (File.Exists(old)) File.Delete(old);
            }

            _animatorUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, Phase4RobotPoseController.ProfilerMarkerName, 1);
            _doorUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, DoorVisualFeedback.ProfilerMarkerName, 1);
            yield return null;
            yield return null;
            if (coordinator == null)
            {
                Fail("MissingCoordinator");
                yield break;
            }

            coordinator.ConfirmInteractiveStartForTests();
            coordinator.SetTransitionDurationsForTests(0.15f, 0.15f);
            PuzzleSectionController[] sections = FindObjectsByType<PuzzleSectionController>(
                FindObjectsInactive.Include);
            Array.Sort(sections, (left, right) =>
                left.SectionNumber.CompareTo(right.SectionNumber));
            if (sections.Length != 3)
            {
                Fail($"SectionCount={sections.Length}");
                yield break;
            }
            for (int i = 0; i < sections.Length; i++)
            {
                Phase4StandaloneRouteInputSource route =
                    sections[i].Player.gameObject.AddComponent<Phase4StandaloneRouteInputSource>();
                route.Configure(i + 1, sections[i].Director);
                sections[i].Player.Configure(route, sections[i].Player.Motor,
                    sections[i].Player.Interactor);
            }
            // Graphical startup can consume more than the route's first thirty
            // ticks while shaders and the first frame initialize. Install every
            // input source first, then restore the validated game state to tick 0.
            coordinator.RestartGameInPlaceForTests();
            for (int i = 0; i < sections.Length; i++)
            {
                // Own simulation ticks while the command-line probe is active.
                // A hidden graphical window can accumulate several Update ticks
                // between physics frames; disabling only LoopDirector.Update keeps
                // rendering and physics live while preserving deterministic,
                // bounded tick batches for the presentation route.
                sections[i].Director.enabled = false;
            }
            _poses = FindObjectsByType<Phase4RobotPoseController>(
                FindObjectsInactive.Include);
            _doors = FindObjectsByType<DoorVisualFeedback>(
                FindObjectsInactive.Include);
            CacheSectionPresentationReferences(sections);

            float started = Time.realtimeSinceStartup;
            Capture(0, started);
            yield return new WaitForEndOfFrame();
            yield return null;
            _captureRequestedThisFrame = false;
            int advances = 0;
            while ((Time.realtimeSinceStartup - started < MinimumDurationSeconds ||
                    coordinator.State != GameplayState.Completed) && advances++ < 4500)
            {
                if (ShouldWaitForCarryPose())
                {
                    // The recorded route advances faster than presentation time.
                    // Let the bounded Interact pose finish before continuing the
                    // held-battery route so Carry is observed in real rendered frames.
                    yield return new WaitForFixedUpdate();
                    SamplePerformance(true);
                    continue;
                }
                if (coordinator.State == GameplayState.Playing)
                {
                    PuzzleSectionController active = coordinator.ActiveSection;
                    for (int tick = 0; tick < SimulationTicksPerPhysicsFrame &&
                         coordinator.State == GameplayState.Playing; tick++)
                    {
                        active.Director.AdvanceOneTickForTests();
                        coordinator.EvaluateActiveSectionForTests();
                        Observe(active, started);
                        if (_captureRequestedThisFrame) break;
                    }
                    if (advances % 30 == 0) LogRouteState(active, advances);
                }
                else if (coordinator.State != GameplayState.Completed)
                {
                    coordinator.CompleteTransitionNowForTests();
                }
                bool captureInstrumentationFrame = _captureRequestedThisFrame;
                if (captureInstrumentationFrame)
                {
                    // ScreenCapture is asynchronous and multiple requests in one
                    // rendered frame collapse to the same image. Finish this frame
                    // before advancing the next presentation state.
                    yield return new WaitForEndOfFrame();
                    yield return null;
                    _captureRequestedThisFrame = false;
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
                SamplePerformance(!captureInstrumentationFrame);
            }

            float deadline = Time.realtimeSinceStartup + 5f;
            while (!AllFilesExist() && Time.realtimeSinceStartup < deadline) yield return null;
            if (coordinator.State != GameplayState.Completed || !AllCaptured() || !AllFilesExist())
            {
                Fail($"state={coordinator.State};captured={CapturedCount()};files={FileCount()}");
                yield break;
            }
            Result result = new Result
            {
                unityVersion = Application.unityVersion,
                graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                durationSeconds = Time.realtimeSinceStartup - started,
                animatorMaximumMs = _animatorMaximumMs,
                doorVisualMaximumMs = _doorMaximumMs,
                carryPoseMaximumFrameMs = _carryMaximumFrameMs,
                doorAnimationMaximumFrameMs = _doorAnimationMaximumFrameMs,
                scenes = _fileNames,
                timestamps = _timestamps
            };
            string resultPath = Path.Combine(_directory, "presentation-probe.json");
            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));
            Debug.Log($"PHASE4_3_PRESENTATION_PROBE_OK duration={result.durationSeconds:F2};" +
                      $"frames={RequiredFrames};animatorMaxMs={result.animatorMaximumMs:F4};" +
                      $"doorMaxMs={result.doorVisualMaximumMs:F4};" +
                      $"carryFrameMaxMs={result.carryPoseMaximumFrameMs:F3};" +
                      $"doorFrameMaxMs={result.doorAnimationMaximumFrameMs:F3};" +
                      $"path={resultPath}", this);
            DisposeRecorders();
            Application.Quit(0);
        }

        private void Observe(PuzzleSectionController section, float started)
        {
            int loop = section.Director.LoopNumber;
            int tick = section.Director.CurrentTick;
            if (section.SectionNumber == 1)
            {
                if (loop == 1 && tick >= 10) Capture(1, started);
                if (loop >= 2 && section.Director.EchoCount >= 1) Capture(2, started);
                if (loop >= 2 && tick >= 35) Capture(3, started);
            }
            else if (section.SectionNumber == 2)
            {
                CarryableBattery battery = _sectionBatteries[1];
                if (battery != null && battery.IsHeld) Capture(4, started);
                if (battery != null && battery.IsHeld && tick >= 75) Capture(5, started);
                PowerSocket socket = _sectionSockets[1];
                if (socket != null && socket.IsPowered) Capture(6, started);
                DoorController door = _sectionDoors[1];
                if (door != null && door.IsOpen) Capture(7, started);
            }
            LoopDirector director = section.Director;
            for (int i = 0; i < director.EchoCount; i++)
            {
                EchoPlayback echo = director.GetEchoPlayback(i);
                if (echo.gameObject.activeInHierarchy &&
                    echo.PlaybackTick >= echo.RecordingLength)
                {
                    Capture(8, started);
                    break;
                }
            }
        }

        private void CacheSectionPresentationReferences(PuzzleSectionController[] sections)
        {
            _sectionBatteries = new CarryableBattery[sections.Length];
            _sectionSockets = new PowerSocket[sections.Length];
            _sectionDoors = new DoorController[sections.Length];
            _sectionPlates = new PressurePlate[sections.Length];
            for (int i = 0; i < sections.Length; i++)
            {
                _sectionBatteries[i] = sections[i].GetComponentInChildren<CarryableBattery>(true);
                _sectionSockets[i] = sections[i].GetComponentInChildren<PowerSocket>(true);
                _sectionDoors[i] = sections[i].GetComponentInChildren<DoorController>(true);
                _sectionPlates[i] = sections[i].GetComponentInChildren<PressurePlate>(true);
            }
        }

        private void LogRouteState(PuzzleSectionController section, int advances)
        {
            LoopDirector director = section.Director;
            PressurePlate plate = _sectionPlates[section.SectionNumber - 1];
            DoorController door = _sectionDoors[section.SectionNumber - 1];
            Vector3 echoPosition = director.EchoCount > 0
                ? director.GetEchoPlayback(0).transform.position
                : Vector3.zero;
            Debug.Log($"PHASE4_3_PROBE_STATE advances={advances};section={section.SectionNumber};" +
                      $"loop={director.LoopNumber};tick={director.CurrentTick};echoes={director.EchoCount};" +
                      $"player={section.Player.transform.position};echo0={echoPosition};" +
                      $"plate={(plate != null && plate.IsPressed)};door={(door != null && door.IsOpen)};" +
                      $"goal={section.Goal.IsReached}", this);
        }

        private void SamplePerformance(bool includeFrameTiming)
        {
            if (_animatorUpdate.Valid)
                _animatorMaximumMs = Mathf.Max(_animatorMaximumMs,
                    _animatorUpdate.LastValue / 1000000f);
            if (_doorUpdate.Valid)
                _doorMaximumMs = Mathf.Max(_doorMaximumMs, _doorUpdate.LastValue / 1000000f);
            if (!includeFrameTiming) return;
            float frameMs = Time.unscaledDeltaTime * 1000f;
            for (int i = 0; i < _poses.Length; i++)
                if (_poses[i].gameObject.activeInHierarchy &&
                    (_poses[i].CurrentState == Phase4RobotPoseState.CarryIdle ||
                     _poses[i].CurrentState == Phase4RobotPoseState.CarryWalk))
                    _carryMaximumFrameMs = Mathf.Max(_carryMaximumFrameMs, frameMs);
            for (int i = 0; i < _doors.Length; i++)
                if (_doors[i].gameObject.activeInHierarchy &&
                    _doors[i].OpenProgress > 0f && _doors[i].OpenProgress < 1f)
                    _doorAnimationMaximumFrameMs = Mathf.Max(_doorAnimationMaximumFrameMs, frameMs);
        }

        private bool ShouldWaitForCarryPose()
        {
            if (coordinator.State != GameplayState.Playing ||
                coordinator.ActiveSectionNumber != 2 || !_captured[4] || _captured[5])
                return false;
            CarryableBattery battery = _sectionBatteries[1];
            if (battery == null || !battery.IsHeld) return false;
            for (int i = 0; i < _poses.Length; i++)
            {
                if (!_poses[i].gameObject.activeInHierarchy) continue;
                if (_poses[i].CurrentState == Phase4RobotPoseState.CarryIdle ||
                    _poses[i].CurrentState == Phase4RobotPoseState.CarryWalk)
                    return false;
            }
            return true;
        }

        private void Capture(int index, float started)
        {
            if (_captured[index] || _captureRequestedThisFrame) return;
            _captureRequestedThisFrame = true;
            _captured[index] = true;
            _timestamps[index] = Time.realtimeSinceStartup - started;
            string path = Path.Combine(_directory, _fileNames[index]);
            ScreenCapture.CaptureScreenshot(path, 1);
            Debug.Log($"PHASE4_3_PROBE_FRAME index={index + 1};" +
                      $"time={_timestamps[index]:F2};path={path}", this);
        }

        private bool AllCaptured() => CapturedCount() == RequiredFrames;

        private int CapturedCount()
        {
            int count = 0;
            for (int i = 0; i < _captured.Length; i++) if (_captured[i]) count++;
            return count;
        }

        private bool AllFilesExist() => FileCount() == RequiredFrames;

        private int FileCount()
        {
            int count = 0;
            for (int i = 0; i < _fileNames.Length; i++)
                if (File.Exists(Path.Combine(_directory, _fileNames[i]))) count++;
            return count;
        }

        private void Fail(string reason)
        {
            Debug.LogError($"PHASE4_3_PRESENTATION_PROBE_FAILED reason={reason}", this);
            DisposeRecorders();
            Application.Quit(3);
        }

        private void OnDestroy() => DisposeRecorders();

        private void DisposeRecorders()
        {
            if (_animatorUpdate.Valid) _animatorUpdate.Dispose();
            if (_doorUpdate.Valid) _doorUpdate.Dispose();
        }

        private static bool HasFlag(string value)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
                if (string.Equals(arguments[i], value,
                    StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}

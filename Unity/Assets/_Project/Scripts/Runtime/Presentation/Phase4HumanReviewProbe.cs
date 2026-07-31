using System;
using System.Collections;
using System.IO;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoShift.Presentation
{
    public sealed class Phase4HumanReviewProbe : MonoBehaviour
    {
        public const string CommandLineFlag = "-phase43HumanReviewProbe";
        public const float MinimumVideoDurationSeconds = 30f;
        public const float MaximumVideoDurationSeconds = 40f;
        public const float MinimumGameplayDurationSeconds = 25f;
        public const float MaximumCompletedDurationSeconds = 2f;
        public const int RequiredSceneCount = 12;

        private static readonly string[] SceneNames =
        {
            "Idle",
            "Straight Walk",
            "Diagonal Walk",
            "Direction Change",
            "Echo Spawn And Move",
            "Echo Stopped",
            "Battery Pickup",
            "Carry Idle",
            "Carry Walk",
            "Battery Insert",
            "Door Open",
            "Player Passes Door"
        };

        private static readonly float[] SceneMinimumDurations =
        {
            2f, 3f, 3f, 2f, 3f, 2f, 2f, 2f, 4f, 2f, 3f, 2f
        };

        [SerializeField] private SectionTransitionCoordinator coordinator;

        private readonly float[] _sceneStarts = new float[RequiredSceneCount];
        private readonly float[] _sceneDurations = new float[RequiredSceneCount];
        private readonly bool[] _sceneStarted = new bool[RequiredSceneCount];
        private float _captureStarted;
        private float _tickBudget;
        private string _directory;
        private string _captureStartedSignal;
        private string _captureFlushedSignal;

        [Serializable]
        private sealed class Result
        {
            public string unityVersion;
            public string graphicsDevice;
            public string graphicsApi;
            public int width;
            public int height;
            public float timeScale;
            public float durationSeconds;
            public float gameplaySeconds;
            public float completedSeconds;
            public string[] scenes;
            public float[] sceneStarts;
            public float[] sceneDurations;
            public string captureStartedSignal;
            public string captureFlushedSignal;
        }

        public static int SceneCount => SceneNames.Length;
        public static string GetSceneName(int index) => SceneNames[index];
        public static float GetSceneMinimumDuration(int index) =>
            SceneMinimumDurations[index];

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
            Time.timeScale = 1f;
            Screen.SetResolution(1920, 1080, false);
            Cursor.visible = false;
            Phase0DebugOverlay[] overlays = FindObjectsByType<Phase0DebugOverlay>(
                FindObjectsInactive.Include);
            for (int i = 0; i < overlays.Length; i++) overlays[i].SetVisible(false);

            yield return null;
            yield return null;
            if (coordinator == null)
            {
                Fail("MissingCoordinator");
                yield break;
            }

            PuzzleSectionController[] sections = FindObjectsByType<PuzzleSectionController>(
                FindObjectsInactive.Include);
            Array.Sort(sections, (left, right) =>
                left.SectionNumber.CompareTo(right.SectionNumber));
            if (sections.Length != 3)
            {
                Fail($"SectionCount={sections.Length}");
                yield break;
            }

            Phase4StandaloneRouteInputSource[] standardRoutes =
                new Phase4StandaloneRouteInputSource[sections.Length];
            Phase4ManualReviewInputSource[] manualInputs =
                new Phase4ManualReviewInputSource[sections.Length];
            for (int i = 0; i < sections.Length; i++)
            {
                standardRoutes[i] =
                    sections[i].Player.gameObject.AddComponent<Phase4StandaloneRouteInputSource>();
                standardRoutes[i].Configure(i + 1, sections[i].Director);
                manualInputs[i] =
                    sections[i].Player.gameObject.AddComponent<Phase4ManualReviewInputSource>();
                sections[i].Player.Configure(
                    standardRoutes[i], sections[i].Player.Motor, sections[i].Player.Interactor);
            }
            Phase4HumanReviewSectionTwoInputSource sectionTwoRoute =
                sections[1].Player.gameObject.AddComponent<
                    Phase4HumanReviewSectionTwoInputSource>();
            sectionTwoRoute.Configure(sections[1].Director);

            coordinator.ConfirmInteractiveStartForTests();
            coordinator.SetTransitionDurationsForTests(0.5f, 0.5f);
            coordinator.RestartGameInPlaceForTests();
            for (int i = 0; i < sections.Length; i++) sections[i].Director.enabled = false;

            _directory = Path.Combine(
                Application.persistentDataPath, "Phase4HumanReviewProbe");
            Directory.CreateDirectory(_directory);
            _captureStartedSignal = Path.Combine(_directory, "capture-started.signal");
            _captureFlushedSignal = Path.Combine(_directory, "capture-flushed.signal");
            DeleteIfPresent(_captureStartedSignal);
            DeleteIfPresent(_captureFlushedSignal);
            DeleteIfPresent(Path.Combine(_directory, "human-review-probe.json"));
            Debug.Log($"PHASE4_3_HUMAN_REVIEW_READY directory={_directory};" +
                      $"startSignal={_captureStartedSignal};" +
                      $"flushSignal={_captureFlushedSignal}", this);

            // External D3D11 capture must query the final DWM bounds and open the
            // hardware desktop-duplication device before the timeline begins.
            float readyDeadline = Time.realtimeSinceStartup + 60f;
            while (!File.Exists(_captureStartedSignal) &&
                   Time.realtimeSinceStartup < readyDeadline)
                yield return null;
            if (!File.Exists(_captureStartedSignal))
            {
                Fail("CaptureStartSignalTimeout");
                yield break;
            }

            _captureStarted = Time.realtimeSinceStartup;
            sections[0].Player.Configure(
                manualInputs[0], sections[0].Player.Motor, sections[0].Player.Interactor);

            BeginScene(0);
            yield return WaitRealtime(SceneMinimumDurations[0]);
            EndScene(0);

            yield return RunMovementScene(
                1, sections[0], manualInputs[0], MovementPattern.Straight);
            yield return RunMovementScene(
                2, sections[0], manualInputs[0], MovementPattern.Diagonal);
            yield return RunMovementScene(
                3, sections[0], manualInputs[0], MovementPattern.Turn);

            sections[0].Player.Configure(
                standardRoutes[0], sections[0].Player.Motor, sections[0].Player.Interactor);
            coordinator.RestartGameInPlaceForTests();
            sections[0].Director.enabled = false;
            float echoDeadline = Time.realtimeSinceStartup + 5f;
            while (sections[0].Director.EchoCount == 0 &&
                   Time.realtimeSinceStartup < echoDeadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            if (sections[0].Director.EchoCount == 0)
            {
                Fail("EchoSpawnTimeout");
                yield break;
            }
            if (coordinator.State == GameplayState.LoopTransition)
                coordinator.CompleteTransitionNowForTests();
            sections[0].Player.Configure(
                manualInputs[0], sections[0].Player.Motor, sections[0].Player.Interactor);

            BeginScene(4);
            yield return AdvanceForSeconds(SceneMinimumDurations[4]);
            EndScene(4);
            EchoPlayback echo = sections[0].Director.GetEchoPlayback(0);
            if (echo.PlaybackTick < echo.RecordingLength)
            {
                Fail($"EchoStillPlaying={echo.PlaybackTick}/{echo.RecordingLength}");
                yield break;
            }

            BeginScene(5);
            yield return WaitRealtime(SceneMinimumDurations[5]);
            EndScene(5);

            sections[0].Player.Configure(
                standardRoutes[0], sections[0].Player.Motor, sections[0].Player.Interactor);
            float sectionDeadline = Time.realtimeSinceStartup + 8f;
            while (coordinator.ActiveSectionNumber == 1 &&
                   Time.realtimeSinceStartup < sectionDeadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            if (coordinator.ActiveSectionNumber != 2)
            {
                Fail($"SectionOneTransition={coordinator.State}");
                yield break;
            }

            sections[1].Player.Configure(
                sectionTwoRoute, sections[1].Player.Motor, sections[1].Player.Interactor);
            CarryableBattery battery =
                sections[1].GetComponentInChildren<CarryableBattery>(true);
            PowerSocket socket = sections[1].GetComponentInChildren<PowerSocket>(true);
            DoorController door = sections[1].GetComponentInChildren<DoorController>(true);
            float batteryDeadline = Time.realtimeSinceStartup + 5f;
            while ((battery == null || !battery.IsHeld) &&
                   Time.realtimeSinceStartup < batteryDeadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            if (battery == null || !battery.IsHeld)
            {
                Fail("BatteryPickupTimeout");
                yield break;
            }

            BeginScene(6);
            yield return WaitRealtime(SceneMinimumDurations[6]);
            EndScene(6);
            BeginScene(7);
            yield return WaitRealtime(SceneMinimumDurations[7]);
            EndScene(7);

            BeginScene(8);
            float carryDeadline = Time.realtimeSinceStartup + SceneMinimumDurations[8];
            while (Time.realtimeSinceStartup < carryDeadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            EndScene(8);
            if (sections[1].Director.CurrentTick < 301 || !battery.IsHeld)
            {
                Fail($"CarryRouteTick={sections[1].Director.CurrentTick};held={battery.IsHeld}");
                yield break;
            }

            BeginScene(9);
            yield return WaitRealtime(0.6f);
            sectionTwoRoute.EnableInsertion();
            StepOneTick();
            yield return WaitRealtime(SceneMinimumDurations[9] - 0.6f);
            EndScene(9);
            if (socket == null || !socket.IsPowered || door == null)
            {
                Fail($"InsertionPowered={socket != null && socket.IsPowered}");
                yield break;
            }

            sectionTwoRoute.EnableLoopEnd();
            StepOneTick();
            StepOneTick();
            if (coordinator.State == GameplayState.LoopTransition)
            {
                yield return WaitRealtime(0.5f);
                coordinator.CompleteTransitionNowForTests();
            }
            sections[1].Player.Configure(
                manualInputs[1], sections[1].Player.Motor, sections[1].Player.Interactor);
            manualInputs[1].SetMove(Vector2.zero);
            float replayDoorDeadline = Time.realtimeSinceStartup + 8f;
            while ((!socket.RequestsDoorOpen || !door.IsOpen) &&
                   Time.realtimeSinceStartup < replayDoorDeadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            if (!socket.RequestsDoorOpen || !socket.InsertedByReplay || !door.IsOpen)
            {
                Fail($"ReplayInsertion={socket.InsertedByReplay};" +
                     $"doorRequested={socket.RequestsDoorOpen};doorOpen={door.IsOpen}");
                yield break;
            }

            BeginScene(10);
            yield return WaitRealtime(1f);
            manualInputs[1].SetMove(Vector2.up);
            BeginScene(11);
            yield return AdvanceForSeconds(SceneMinimumDurations[11]);
            manualInputs[1].SetMove(Vector2.zero);
            EndScene(11);
            EndScene(10);

            float duration = Time.realtimeSinceStartup - _captureStarted;
            if (!AllScenesMeetDuration() ||
                duration < MinimumGameplayDurationSeconds ||
                duration > MaximumVideoDurationSeconds)
            {
                Fail($"duration={duration:F3};scenes={CompletedSceneCount()}");
                yield break;
            }

            Result result = new Result
            {
                unityVersion = Application.unityVersion,
                graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                width = Screen.width,
                height = Screen.height,
                timeScale = Time.timeScale,
                durationSeconds = duration,
                gameplaySeconds = duration,
                completedSeconds = 0f,
                scenes = (string[])SceneNames.Clone(),
                sceneStarts = (float[])_sceneStarts.Clone(),
                sceneDurations = (float[])_sceneDurations.Clone(),
                captureStartedSignal = _captureStartedSignal,
                captureFlushedSignal = _captureFlushedSignal
            };
            string resultPath = Path.Combine(_directory, "human-review-probe.json");
            File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));
            Debug.Log($"PHASE4_3_HUMAN_REVIEW_CAPTURE_COMPLETE duration={duration:F3};" +
                      $"gameplay={result.gameplaySeconds:F3};completed=0.000;" +
                      $"timeScale={Time.timeScale:F1};scenes={CompletedSceneCount()};" +
                      $"path={resultPath}", this);

            float flushDeadline = Time.realtimeSinceStartup + 30f;
            while (!File.Exists(_captureFlushedSignal) &&
                   Time.realtimeSinceStartup < flushDeadline)
                yield return null;
            if (!File.Exists(_captureFlushedSignal))
            {
                Fail("CaptureFlushSignalTimeout");
                yield break;
            }
            Debug.Log($"PHASE4_3_HUMAN_REVIEW_OK duration={duration:F3};" +
                      $"flushAcknowledged=true;timeScale={Time.timeScale:F1}", this);
            // Leave two rendered frames plus a short real-time drain after the
            // external encoder acknowledges its flush. This keeps the capture
            // timeline unchanged while allowing D3D11 presentation work to
            // retire before the Development Player tears down its device.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.5f);
            Application.Quit(0);
        }

        private IEnumerator RunMovementScene(
            int sceneIndex,
            PuzzleSectionController section,
            Phase4ManualReviewInputSource input,
            MovementPattern pattern)
        {
            BeginScene(sceneIndex);
            float started = Time.realtimeSinceStartup;
            float duration = SceneMinimumDurations[sceneIndex];
            while (Time.realtimeSinceStartup - started < duration)
            {
                float elapsed = Time.realtimeSinceStartup - started;
                input.SetMove(GetMovement(pattern, elapsed));
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
            input.SetMove(Vector2.zero);
            EndScene(sceneIndex);
            AssertSectionStillActive(section);
        }

        private IEnumerator AdvanceForSeconds(float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForFixedUpdate();
                StepActiveSimulation();
            }
        }

        private static IEnumerator WaitRealtime(float seconds)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < deadline) yield return null;
        }

        private void StepActiveSimulation()
        {
            if (coordinator.State != GameplayState.Playing) return;
            LoopDirector director = coordinator.ActiveSection.Director;
            _tickBudget += Time.fixedDeltaTime * director.TickRate;
            int ticks = Mathf.Min(2, Mathf.FloorToInt(_tickBudget));
            _tickBudget -= ticks;
            for (int i = 0; i < ticks && coordinator.State == GameplayState.Playing; i++)
                StepOneTick();
        }

        private void StepOneTick()
        {
            if (coordinator.State != GameplayState.Playing) return;
            coordinator.ActiveSection.Director.AdvanceOneTickForTests();
            coordinator.EvaluateActiveSectionForTests();
        }

        private void BeginScene(int index)
        {
            _sceneStarted[index] = true;
            _sceneStarts[index] = Time.realtimeSinceStartup - _captureStarted;
            Debug.Log($"PHASE4_3_HUMAN_REVIEW_SCENE_BEGIN index={index + 1};" +
                      $"name={SceneNames[index]};time={_sceneStarts[index]:F3}", this);
        }

        private void EndScene(int index)
        {
            _sceneDurations[index] =
                Time.realtimeSinceStartup - _captureStarted - _sceneStarts[index];
            Debug.Log($"PHASE4_3_HUMAN_REVIEW_SCENE_END index={index + 1};" +
                      $"name={SceneNames[index]};duration={_sceneDurations[index]:F3}", this);
        }

        private bool AllScenesMeetDuration()
        {
            for (int i = 0; i < RequiredSceneCount; i++)
                if (!_sceneStarted[i] ||
                    _sceneDurations[i] + 0.02f < SceneMinimumDurations[i])
                    return false;
            return true;
        }

        private int CompletedSceneCount()
        {
            int count = 0;
            for (int i = 0; i < RequiredSceneCount; i++)
                if (_sceneStarted[i] && _sceneDurations[i] > 0f) count++;
            return count;
        }

        private void AssertSectionStillActive(PuzzleSectionController section)
        {
            if (coordinator.ActiveSection != section)
                Fail($"UnexpectedSection={coordinator.ActiveSectionNumber}");
        }

        private static Vector2 GetMovement(MovementPattern pattern, float elapsed)
        {
            int phase = Mathf.FloorToInt(elapsed / 0.5f);
            if (pattern == MovementPattern.Straight)
                return (phase & 1) == 0 ? Vector2.up : Vector2.down;
            if (pattern == MovementPattern.Diagonal)
            {
                Vector2 diagonal = new Vector2(1f, 1f).normalized;
                return (phase & 1) == 0 ? diagonal : -diagonal;
            }
            switch (phase & 3)
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                case 2: return Vector2.down;
                default: return Vector2.left;
            }
        }

        private void Fail(string reason)
        {
            Debug.LogError($"PHASE4_3_HUMAN_REVIEW_FAILED reason={reason}", this);
            Application.Quit(4);
        }

        private static void DeleteIfPresent(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static bool HasFlag(string value)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
                if (string.Equals(arguments[i], value,
                    StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private enum MovementPattern
        {
            Straight,
            Diagonal,
            Turn
        }
    }

    public sealed class Phase4ManualReviewInputSource : MonoBehaviour, IInputSource
    {
        private Vector2 _move;

        public void SetMove(Vector2 move) => _move = Vector2.ClampMagnitude(move, 1f);

        public InputCommand Sample(int tick) =>
            new InputCommand(tick, _move, InputButtonFlags.None);
    }

    public sealed class Phase4HumanReviewSectionTwoInputSource :
        MonoBehaviour, IInputSource
    {
        private LoopDirector _director;
        private bool _insertionEnabled;
        private bool _insertionIssued;
        private bool _loopEndEnabled;
        private bool _loopEndIssued;

        public void Configure(LoopDirector director) => _director = director;
        public void EnableInsertion() => _insertionEnabled = true;
        public void EnableLoopEnd() => _loopEndEnabled = true;

        public InputCommand Sample(int tick)
        {
            Vector2 move = Vector2.zero;
            InputButtonFlags buttons = InputButtonFlags.None;
            if (_director != null && _director.LoopNumber == 1)
            {
                if (_loopEndEnabled && !_loopEndIssued)
                {
                    buttons = InputButtonFlags.EndLoop;
                    _loopEndIssued = true;
                }
                else if (tick < 45) move = Vector2.up;
                else if (tick < 60) move = Vector2.right;
                else if (tick == 61) buttons = InputButtonFlags.Interact;
                else if (tick < 302) move = Vector2.up * 0.1875f;
                else if (_insertionEnabled && !_insertionIssued)
                {
                    buttons = InputButtonFlags.Interact;
                    _insertionIssued = true;
                }
            }
            return new InputCommand(tick, move, buttons);
        }
    }
}

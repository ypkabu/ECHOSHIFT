using System;
using System.Collections.Generic;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Core
{
    public sealed class LoopDirector : MonoBehaviour
    {
        public event Action<LoopHistorySummary> LoopCompleted;
        public event Action<InteractionExecution, LoopActor> InteractionResolved;
        public event Action<Vector3, int> EchoRemoved;

        [SerializeField] private LoopSettings settings;
        [SerializeField] private PlayerSimulation playerSimulation;
        [SerializeField] private TransformResettable playerReset;
        [SerializeField] private ResetRegistry resetRegistry;
        [SerializeField] private EchoPlayback echoPrefab;
        [SerializeField] private Transform echoContainer;
        [SerializeField] private InteractionRegistry interactionRegistry;
        [SerializeField] private PressurePlate[] coordinatedPressurePlates =
            Array.Empty<PressurePlate>();
        [SerializeField] private DoorController[] coordinatedDoors =
            Array.Empty<DoorController>();
        [SerializeField] private GoalVolume coordinatedGoal;

        private readonly List<EchoPlayback> _echoes = new List<EchoPlayback>(3);
        private readonly ReplayFrame[] _echoFrames = new ReplayFrame[3];
        private readonly bool[] _echoHasFrame = new bool[3];
        private readonly InteractionRequest[] _requests =
            new InteractionRequest[InteractionConflictResolver.MaximumRequestsPerTick];
        private readonly InteractionResolution[] _resolutions =
            new InteractionResolution[InteractionConflictResolver.MaximumRequestsPerTick];
        private readonly InteractionConflictResolver _conflictResolver =
            new InteractionConflictResolver();

        private SimulationClock _clock;
        private ReplayRecorder _recorder;
        private Vector3 _actorStartPosition;
        private Quaternion _actorStartRotation;
        private bool _isInitialized;
        private bool _isTransitioning;
        private bool _loopEndRequested;
        private LoopEndReason _pendingEndReason = LoopEndReason.Manual;
        private int _nextReplayGeneration = 1;
        private bool _externallyPaused;

        public int CurrentTick { get; private set; }
        public int LoopNumber { get; private set; } = 1;
        public int EchoCount => _echoes.Count;
        public int MaxTicks => settings != null ? settings.MaxTicks : 0;
        public int TickRate => settings != null ? settings.TickRate : 0;
        public int LoopDurationSeconds => settings != null ? settings.LoopDurationSeconds : 0;
        public int MaxEchoes => settings != null ? settings.MaxEchoes : 0;
        public float DriftTolerance => settings != null ? settings.DriftTolerance : 0f;
        public ReplayRecording LastCompletedRecording { get; private set; }
        public bool LastCompletedPressurePlateWasPressed { get; private set; }
        public int CurrentInteractionCount => _recorder?.InteractionCount ?? 0;
        public LoopEndReason LastLoopEndReason { get; private set; } = LoopEndReason.Manual;
        public LoopHistory History { get; private set; } = new LoopHistory();
        public bool IsInitialized => _isInitialized;
        public bool IsSimulationPaused => _externallyPaused;
        public bool HasValidReferences =>
            settings != null && playerSimulation != null && playerReset != null &&
            resetRegistry != null && echoPrefab != null && echoContainer != null &&
            playerSimulation.HasValidReferences;

        public float MaximumReplayDrift
        {
            get
            {
                float maximum = 0f;
                for (int i = 0; i < _echoes.Count; i++)
                {
                    maximum = Mathf.Max(maximum, _echoes[i].MaximumDrift);
                }

                return maximum;
            }
        }

        public bool AnyEchoExceededTolerance
        {
            get
            {
                for (int i = 0; i < _echoes.Count; i++)
                {
                    if (_echoes[i].ExceededTolerance)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Configure(
            LoopSettings loopSettings,
            PlayerSimulation player,
            TransformResettable playerTransformReset,
            ResetRegistry registry,
            EchoPlayback prefab,
            Transform container)
        {
            Configure(loopSettings, player, playerTransformReset, registry, prefab,
                container, null, null, null, null);
        }

        public void Configure(
            LoopSettings loopSettings,
            PlayerSimulation player,
            TransformResettable playerTransformReset,
            ResetRegistry registry,
            EchoPlayback prefab,
            Transform container,
            InteractionRegistry recordedInteractionRegistry)
        {
            Configure(loopSettings, player, playerTransformReset, registry, prefab,
                container, recordedInteractionRegistry, null, null, null);
        }

        public void Configure(
            LoopSettings loopSettings,
            PlayerSimulation player,
            TransformResettable playerTransformReset,
            ResetRegistry registry,
            EchoPlayback prefab,
            Transform container,
            InteractionRegistry recordedInteractionRegistry,
            PressurePlate[] pressurePlates,
            DoorController[] doors,
            GoalVolume goal)
        {
            settings = loopSettings;
            playerSimulation = player;
            playerReset = playerTransformReset;
            resetRegistry = registry;
            echoPrefab = prefab;
            echoContainer = container;
            interactionRegistry = recordedInteractionRegistry;
            coordinatedPressurePlates = pressurePlates ?? Array.Empty<PressurePlate>();
            coordinatedDoors = doors ?? Array.Empty<DoorController>();
            coordinatedGoal = goal;
        }

        private void Start()
        {
            if (!_isInitialized && !TryInitialize())
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_isInitialized || _externallyPaused)
            {
                return;
            }

            if (_loopEndRequested)
            {
                TransitionLoop();
                return;
            }

            _clock.AddTime(Time.unscaledDeltaTime);
            while (_clock.TryConsumeTick())
            {
                SimulateTick();
                if (_loopEndRequested)
                {
                    break;
                }
            }
        }

        public void RequestLoopEnd()
        {
            RequestLoopEnd(LoopEndReason.Manual);
        }

        public void RequestLoopEnd(LoopEndReason reason)
        {
            if (_isInitialized && !_isTransitioning && !_externallyPaused)
            {
                _pendingEndReason = reason;
                _loopEndRequested = true;
            }
        }

        public EchoPlayback GetEchoPlayback(int index)
        {
            return _echoes[index];
        }

        public void SetSimulationPaused(bool paused)
        {
            _externallyPaused = paused;
            if (_clock == null)
            {
                return;
            }

            if (paused)
            {
                _clock.Pause();
            }
            else if (_isInitialized && !_isTransitioning)
            {
                _clock.Resume();
            }
        }

        public void RestartSectionLifecycle()
        {
            if (!_isInitialized)
            {
                return;
            }

            _clock.Pause();
            ReleaseActorHeldObjects();
            ClearEchoes();
            resetRegistry.RestoreInitialStates();
            playerReset.RestoreInitialState();
            playerSimulation.Motor.ResetPose(_actorStartPosition, _actorStartRotation);
            playerSimulation.GetComponent<LoopActor>()?.Configure(LoopActorKind.Player, 1);
            playerSimulation.ResetInteractionStatistics();
            Physics.SyncTransforms();
            _recorder = new ReplayRecorder(settings.MaxTicks);
            History = new LoopHistory();
            LastCompletedRecording = null;
            LastCompletedPressurePlateWasPressed = false;
            CurrentTick = 0;
            LoopNumber = 1;
            _nextReplayGeneration = 1;
            _loopEndRequested = false;
            _pendingEndReason = LoopEndReason.Manual;
            _clock.Reset();
            if (!_externallyPaused)
            {
                _clock.Resume();
            }
        }

        public void ShutdownSectionLifecycle()
        {
            if (!_isInitialized)
            {
                return;
            }

            _externallyPaused = true;
            _clock.Pause();
            ReleaseActorHeldObjects();
            ClearEchoes();
            resetRegistry.RestoreInitialStates();
            _loopEndRequested = false;
        }

        public bool InitializeForTests()
        {
            return _isInitialized || TryInitialize();
        }

        public bool EnsureInitialized()
        {
            return _isInitialized || TryInitialize();
        }

        public void AdvanceOneTickForTests()
        {
            if (!InitializeForTests())
            {
                throw new InvalidOperationException("LoopDirector test initialization failed.");
            }

            if (_externallyPaused)
            {
                return;
            }

            if (_loopEndRequested)
            {
                TransitionLoop();
                return;
            }

            SimulateTick();
        }

        private bool TryInitialize()
        {
            if (!HasValidReferences)
            {
                Debug.LogError("LoopDirector has missing required references.", this);
                return false;
            }

            if (!settings.TryValidate(out string error))
            {
                Debug.LogError(error, settings);
                return false;
            }

            resetRegistry.CaptureInitialStates();
            playerReset.CaptureInitialState();
            _actorStartPosition = playerReset.InitialPosition;
            _actorStartRotation = playerReset.InitialRotation;
            LoopActor playerActor = playerSimulation.GetComponent<LoopActor>();
            playerActor?.Configure(LoopActorKind.Player, _nextReplayGeneration);
            for (int i = 0; i < coordinatedDoors.Length; i++)
            {
                coordinatedDoors[i]?.UseCoordinatedTicks();
            }

            _clock = new SimulationClock(settings.TickRate, settings.MaxCatchUpTicksPerFrame);
            _recorder = new ReplayRecorder(settings.MaxTicks);
            History = new LoopHistory();
            _clock.Resume();
            _isInitialized = true;
            return true;
        }

        private void SimulateTick()
        {
            BeginDevicesForTick();

            int echoCount = _echoes.Count;
            for (int i = 0; i < echoCount; i++)
            {
                _echoHasFrame[i] = _echoes[i].TryGetCurrentFrame(out _echoFrames[i]);
            }

            InputCommand playerCommand = playerSimulation.CollectCommand(CurrentTick);
            for (int i = 0; i < echoCount; i++)
            {
                if (_echoHasFrame[i])
                {
                    _echoes[i].SimulateCurrentMovement(_echoFrames[i], settings.TickDuration);
                }
            }

            playerSimulation.SimulateMovement(playerCommand, settings.TickDuration);
            Physics.SyncTransforms();
            RefreshSensors();

            int requestCount = 0;
            for (int i = 0; i < echoCount; i++)
            {
                if (_echoHasFrame[i] &&
                    _echoes[i].TryCreateInteractionRequest(
                        _echoFrames[i].Command.Tick,
                        out InteractionRequest request))
                {
                    _requests[requestCount++] = request;
                }
            }

            LoopActor playerActor = playerSimulation.GetComponent<LoopActor>();
            ActorSimulationOrder playerOrder = playerActor != null
                ? playerActor.SimulationOrder
                : new ActorSimulationOrder(LoopActorKind.Player, _nextReplayGeneration);
            bool playerRequestedInteraction =
                playerCommand.HasButton(InputButtonFlags.Interact);
            if (playerSimulation.TryCreateInteractionRequest(
                    playerCommand,
                    playerOrder,
                    out InteractionRequest playerRequest))
            {
                _requests[requestCount++] = playerRequest;
            }
            else if (playerRequestedInteraction &&
                     playerSimulation.LastInteractionExecution.FailureReason !=
                     InteractionFailureReason.None)
            {
                InteractionResolved?.Invoke(
                    playerSimulation.LastInteractionExecution, playerActor);
            }

            int resolutionCount = _conflictResolver.Resolve(
                _requests, requestCount, _resolutions);
            for (int i = 0; i < resolutionCount; i++)
            {
                ApplyInteractionResolution(_resolutions[i]);
                _requests[i] = default;
                _resolutions[i] = default;
            }

            CommitDevicesForNextTick();
            for (int i = 0; i < echoCount; i++)
            {
                if (_echoHasFrame[i])
                {
                    _echoes[i].CompleteCurrentFrame(_echoFrames[i]);
                    _echoHasFrame[i] = false;
                }
            }

            ReplayFrame playerFrame = playerSimulation.CaptureFrame(playerCommand);
            ReplayRecordResult recordResult = _recorder.TryRecord(playerFrame);
            if (recordResult != ReplayRecordResult.Recorded)
            {
                Debug.LogError($"Replay recorder rejected tick {CurrentTick}: {recordResult}.", this);
                RequestLoopEnd(LoopEndReason.Test);
                return;
            }

            InteractionExecution interaction = playerSimulation.LastInteractionExecution;
            if (interaction.Succeeded)
            {
                InteractionRecordResult interactionResult =
                    _recorder.TryRecordInteraction(interaction.Command);
                if (interactionResult != InteractionRecordResult.Recorded)
                {
                    Debug.LogError(
                        $"Interaction recorder rejected tick {CurrentTick}: {interactionResult}.",
                        this);
                    RequestLoopEnd(LoopEndReason.Test);
                    return;
                }
            }

            coordinatedGoal?.RefreshFromPhysics();
            CurrentTick++;
            if (coordinatedGoal != null && coordinatedGoal.IsReached)
            {
                RequestLoopEnd(LoopEndReason.Goal);
            }
            else if (playerCommand.HasButton(InputButtonFlags.EndLoop))
            {
                RequestLoopEnd(LoopEndReason.Manual);
            }
            else if (CurrentTick >= settings.MaxTicks)
            {
                RequestLoopEnd(LoopEndReason.Timer);
            }
        }

        private void BeginDevicesForTick()
        {
            for (int i = 0; i < coordinatedDoors.Length; i++)
            {
                coordinatedDoors[i]?.BeginSimulationTick();
            }

            Physics.SyncTransforms();
        }

        private void RefreshSensors()
        {
            for (int i = 0; i < coordinatedPressurePlates.Length; i++)
            {
                coordinatedPressurePlates[i]?.RefreshFromPhysics();
            }

            playerSimulation.RefreshInteractionCandidate(CurrentTick);
        }

        private void CommitDevicesForNextTick()
        {
            for (int i = 0; i < coordinatedDoors.Length; i++)
            {
                coordinatedDoors[i]?.CommitDeviceState();
            }
        }

        private void ApplyInteractionResolution(InteractionResolution resolution)
        {
            LoopActor resolvedActor = resolution.Request.Interactor != null
                ? resolution.Request.Interactor.Actor
                : null;
            if (resolution.Request.Interactor == playerSimulation.Interactor)
            {
                playerSimulation.CompleteInteraction(resolution.Execution);
                InteractionResolved?.Invoke(resolution.Execution, resolvedActor);
                return;
            }

            for (int i = 0; i < _echoes.Count; i++)
            {
                if (resolution.Request.Interactor == _echoes[i].Interactor)
                {
                    _echoes[i].CompleteInteraction(resolution.Execution);
                    InteractionResolved?.Invoke(resolution.Execution, resolvedActor);
                    return;
                }
            }
        }

        private void TransitionLoop()
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            _clock.Pause();
            UpdateHistoryRuntimeResults();
            LastCompletedRecording = _recorder.FinalizeRecording();
            LastCompletedPressurePlateWasPressed =
                IsAnyCoordinatedPressurePlatePressed();
            LastLoopEndReason = _pendingEndReason;
            int generation = _nextReplayGeneration;

            ReleaseActorHeldObjects();
            resetRegistry.RestoreInitialStates();
            Physics.SyncTransforms();
            for (int i = 0; i < _echoes.Count; i++)
            {
                _echoes[i].Rewind(_actorStartPosition, _actorStartRotation);
            }

            AddEcho(LastCompletedRecording, generation);
            LoopHistorySummary completedSummary = new LoopHistorySummary(
                LoopNumber,
                LastCompletedRecording.Count,
                LastCompletedRecording.Interactions.Count,
                generation,
                0f,
                playerSimulation.InteractionSuccessCount,
                playerSimulation.InteractionFailureCount,
                ReplayHistoryState.Active,
                LastLoopEndReason);
            History.Add(completedSummary);
            _nextReplayGeneration++;

            playerReset.RestoreInitialState();
            playerSimulation.Motor.ResetPose(_actorStartPosition, _actorStartRotation);
            playerSimulation.GetComponent<LoopActor>()?.Configure(
                LoopActorKind.Player, _nextReplayGeneration);
            playerSimulation.ResetInteractionStatistics();
            Physics.SyncTransforms();

            _recorder = new ReplayRecorder(settings.MaxTicks);
            CurrentTick = 0;
            LoopNumber++;
            _loopEndRequested = false;
            _pendingEndReason = LoopEndReason.Manual;
            _clock.Reset();
            _clock.Resume();
            _isTransitioning = false;
            LoopCompleted?.Invoke(completedSummary);
        }

        private bool IsAnyCoordinatedPressurePlatePressed()
        {
            for (int i = 0; i < coordinatedPressurePlates.Length; i++)
            {
                if (coordinatedPressurePlates[i] != null &&
                    coordinatedPressurePlates[i].IsPressed)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddEcho(ReplayRecording recording, int generation)
        {
            if (_echoes.Count >= settings.MaxEchoes)
            {
                EchoPlayback oldest = _echoes[0];
                _echoes.RemoveAt(0);
                History.MarkEvicted(oldest.ReplayGeneration);
                oldest.ReleaseCarriedForReset();
                EchoRemoved?.Invoke(oldest.transform.position, oldest.ReplayGeneration);
                oldest.gameObject.SetActive(false);
                Destroy(oldest.gameObject);
            }

            EchoPlayback echo = Instantiate(
                echoPrefab, _actorStartPosition, _actorStartRotation, echoContainer);
            CharacterMotor motor = echo.GetComponent<CharacterMotor>();
            LoopActor actor = echo.GetComponent<LoopActor>();
            Interactor actorInteractor = echo.GetComponent<Interactor>();
            if (actorInteractor != null)
            {
                actorInteractor.Configure(actor, actorInteractor.Sensor,
                    actorInteractor.CarrySocket, interactionRegistry);
            }

            echo.Initialize(recording, motor, actor, settings.DriftTolerance,
                actorInteractor, generation);
            echo.gameObject.SetActive(true);
            _echoes.Add(echo);
        }

        private void UpdateHistoryRuntimeResults()
        {
            for (int i = 0; i < _echoes.Count; i++)
            {
                EchoPlayback echo = _echoes[i];
                History.UpdateRuntimeResults(
                    echo.ReplayGeneration,
                    echo.MaximumDrift,
                    echo.InteractionSuccessCount,
                    echo.InteractionFailureCount);
            }
        }

        private void ReleaseActorHeldObjects()
        {
            playerSimulation.ReleaseCarriedForReset();
            for (int i = 0; i < _echoes.Count; i++)
            {
                _echoes[i].ReleaseCarriedForReset();
            }
        }

        private void ClearEchoes()
        {
            for (int i = 0; i < _echoes.Count; i++)
            {
                EchoPlayback echo = _echoes[i];
                if (echo == null)
                {
                    continue;
                }

                echo.ReleaseCarriedForReset();
                EchoRemoved?.Invoke(echo.transform.position, echo.ReplayGeneration);
                echo.gameObject.SetActive(false);
                Destroy(echo.gameObject);
            }

            _echoes.Clear();
        }
    }
}

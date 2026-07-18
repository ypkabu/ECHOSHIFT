using System.Collections.Generic;
using EchoShift.Player;
using EchoShift.Interaction.Recorded;
using EchoShift.Replay;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Core
{
    public sealed class LoopDirector : MonoBehaviour
    {
        [SerializeField] private LoopSettings settings;
        [SerializeField] private PlayerSimulation playerSimulation;
        [SerializeField] private TransformResettable playerReset;
        [SerializeField] private ResetRegistry resetRegistry;
        [SerializeField] private EchoPlayback echoPrefab;
        [SerializeField] private Transform echoContainer;

        private readonly List<EchoPlayback> _echoes = new List<EchoPlayback>(3);
        private SimulationClock _clock;
        private ReplayRecorder _recorder;
        private Vector3 _actorStartPosition;
        private Quaternion _actorStartRotation;
        private bool _isInitialized;
        private bool _isTransitioning;
        private bool _loopEndRequested;

        public int CurrentTick { get; private set; }
        public int LoopNumber { get; private set; } = 1;
        public int EchoCount => _echoes.Count;
        public int MaxTicks => settings != null ? settings.MaxTicks : 0;
        public ReplayRecording LastCompletedRecording { get; private set; }
        public int CurrentInteractionCount => _recorder?.InteractionCount ?? 0;
        public bool HasValidReferences =>
            settings != null &&
            playerSimulation != null &&
            playerReset != null &&
            resetRegistry != null &&
            echoPrefab != null &&
            echoContainer != null &&
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
            settings = loopSettings;
            playerSimulation = player;
            playerReset = playerTransformReset;
            resetRegistry = registry;
            echoPrefab = prefab;
            echoContainer = container;
        }

        private void Start()
        {
            if (!TryInitialize())
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_isInitialized)
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
                    TransitionLoop();
                    break;
                }
            }
        }

        public void RequestLoopEnd()
        {
            if (_isInitialized && !_isTransitioning)
            {
                _loopEndRequested = true;
            }
        }

        public EchoPlayback GetEchoPlayback(int index)
        {
            return _echoes[index];
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
            _clock = new SimulationClock(settings.TickRate, settings.MaxCatchUpTicksPerFrame);
            _recorder = new ReplayRecorder(settings.MaxTicks);
            _clock.Resume();
            _isInitialized = true;
            return true;
        }

        private void SimulateTick()
        {
            ReplayFrame playerFrame = playerSimulation.SimulateTick(
                CurrentTick,
                settings.TickDuration);
            ReplayRecordResult recordResult = _recorder.TryRecord(playerFrame);
            if (recordResult != ReplayRecordResult.Recorded)
            {
                Debug.LogError($"Replay recorder rejected tick {CurrentTick}: {recordResult}.", this);
                _loopEndRequested = true;
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
                    _loopEndRequested = true;
                    return;
                }
            }

            for (int i = 0; i < _echoes.Count; i++)
            {
                _echoes[i].SimulateTick(settings.TickDuration);
            }

            CurrentTick++;
            if (playerFrame.Command.HasButton(InputButtonFlags.EndLoop) ||
                CurrentTick >= settings.MaxTicks)
            {
                _loopEndRequested = true;
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

            LastCompletedRecording = _recorder.FinalizeRecording();
            ReleaseActorHeldObjects();
            resetRegistry.RestoreInitialStates();
            Physics.SyncTransforms();

            for (int i = 0; i < _echoes.Count; i++)
            {
                _echoes[i].Rewind(_actorStartPosition, _actorStartRotation);
            }

            AddEcho(LastCompletedRecording);
            playerReset.RestoreInitialState();
            playerSimulation.Motor.ResetPose(_actorStartPosition, _actorStartRotation);
            Physics.SyncTransforms();

            _recorder = new ReplayRecorder(settings.MaxTicks);
            CurrentTick = 0;
            LoopNumber++;
            _loopEndRequested = false;
            _clock.Reset();
            _clock.Resume();
            _isTransitioning = false;
        }

        private void AddEcho(ReplayRecording recording)
        {
            if (_echoes.Count >= settings.MaxEchoes)
            {
                EchoPlayback oldest = _echoes[0];
                _echoes.RemoveAt(0);
                oldest.ReleaseCarriedForReset();
                oldest.gameObject.SetActive(false);
                Destroy(oldest.gameObject);
            }

            EchoPlayback echo = Instantiate(
                echoPrefab,
                _actorStartPosition,
                _actorStartRotation,
                echoContainer);
            CharacterMotor motor = echo.GetComponent<CharacterMotor>();
            LoopActor actor = echo.GetComponent<LoopActor>();
            echo.Initialize(recording, motor, actor, settings.DriftTolerance);
            echo.gameObject.SetActive(true);
            _echoes.Add(echo);
        }

        private void ReleaseActorHeldObjects()
        {
            playerSimulation.ReleaseCarriedForReset();
            for (int i = 0; i < _echoes.Count; i++)
            {
                _echoes[i].ReleaseCarriedForReset();
            }
        }
    }
}

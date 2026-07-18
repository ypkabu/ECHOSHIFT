using EchoShift.Player;
using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Replay
{
    [RequireComponent(typeof(CharacterMotor), typeof(LoopActor))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private LoopActor loopActor;
        [SerializeField] private Interactor interactor;

        private ReplayRecording _recording;
        private ReplayDriftMonitor _driftMonitor;
        private Vector3[] _actualPath;
        private int _nextFrame;
        private int _nextInteraction;

        public int PlaybackTick => _nextFrame;
        public int RecordingLength => _recording?.Count ?? 0;
        public int ReplayGeneration => loopActor != null ? loopActor.ReplayGeneration : 0;
        public Color GenerationColor { get; private set; } = Color.cyan;
        public LoopActor Actor => loopActor;
        public CharacterMotor Motor => motor;
        public Interactor Interactor => interactor;
        public float CurrentDrift => _driftMonitor?.CurrentDrift ?? 0f;
        public float MaximumDrift => _driftMonitor?.MaximumDrift ?? 0f;
        public bool ExceededTolerance => _driftMonitor?.ExceededTolerance ?? false;
        public int InteractionSuccessCount { get; private set; }
        public int InteractionFailureCount { get; private set; }
        public InteractionFailureReason LastInteractionFailure { get; private set; }
        public int NextInteractionTick =>
            _recording != null && _nextInteraction < _recording.Interactions.Count
                ? _recording.Interactions[_nextInteraction].Tick
                : -1;

        public void Initialize(
            ReplayRecording recording,
            CharacterMotor characterMotor,
            LoopActor actor,
            float driftTolerance,
            Interactor actorInteractor = null,
            int replayGeneration = 0)
        {
            _recording = recording;
            motor = characterMotor;
            loopActor = actor;
            interactor = actorInteractor != null
                ? actorInteractor
                : GetComponent<Interactor>();
            loopActor.Configure(LoopActorKind.Echo, replayGeneration);
            ApplyGenerationVisual(replayGeneration);
            _driftMonitor = new ReplayDriftMonitor(driftTolerance);
            _actualPath = new Vector3[recording.Count];
            _nextFrame = 0;
            ResetInteractionPlayback();
        }

        public void SimulateTick(float tickDuration)
        {
            if (!TryGetCurrentFrame(out ReplayFrame frame))
            {
                return;
            }

            SimulateCurrentMovement(frame, tickDuration);
            if (TryCreateInteractionRequest(frame.Command.Tick, out InteractionRequest request))
            {
                CompleteInteraction(request.Interactor.ExecuteRecorded(request.Command));
            }

            CompleteCurrentFrame(frame);
        }

        public bool TryGetCurrentFrame(out ReplayFrame frame)
        {
            if (_recording == null || _nextFrame >= _recording.Count)
            {
                frame = default;
                return false;
            }

            frame = _recording[_nextFrame];
            return true;
        }

        public void SimulateCurrentMovement(ReplayFrame frame, float tickDuration)
        {
            motor.Simulate(frame.Command.Move, tickDuration);
        }

        public bool TryCreateInteractionRequest(
            int tick,
            out InteractionRequest request)
        {
            request = default;
            if (_recording == null)
            {
                return false;
            }

            InteractionRecording interactions = _recording.Interactions;
            while (_nextInteraction < interactions.Count &&
                   interactions[_nextInteraction].Tick < tick)
            {
                _nextInteraction++;
            }

            if (_nextInteraction >= interactions.Count ||
                interactions[_nextInteraction].Tick != tick)
            {
                return false;
            }

            InteractionCommand command = interactions[_nextInteraction];
            _nextInteraction++;
            request = interactor != null
                ? interactor.CreateRecordedRequest(command, loopActor.SimulationOrder)
                : new InteractionRequest(loopActor.SimulationOrder, command, null);
            return true;
        }

        public void CompleteInteraction(InteractionExecution execution)
        {
            if (execution.Succeeded)
            {
                InteractionSuccessCount++;
            }
            else
            {
                InteractionFailureCount++;
                LastInteractionFailure = execution.FailureReason;
            }
        }

        public void CompleteCurrentFrame(ReplayFrame frame)
        {
            if (_recording == null || _nextFrame >= _recording.Count)
            {
                return;
            }

            _driftMonitor.Measure(frame.ExpectedPosition, motor.Position);
            _actualPath[_nextFrame] = motor.Position;
            _nextFrame++;
        }

        public void Rewind(Vector3 position, Quaternion rotation)
        {
            motor.ResetPose(position, rotation);
            _nextFrame = 0;
            _driftMonitor?.Reset();
            ResetInteractionPlayback();
        }

        public void ReleaseCarriedForReset()
        {
            interactor?.ReleaseCarriedForReset();
        }

        private void ResetInteractionPlayback()
        {
            _nextInteraction = 0;
            InteractionSuccessCount = 0;
            InteractionFailureCount = 0;
            LastInteractionFailure = InteractionFailureReason.None;
        }

        private void ApplyGenerationVisual(int generation)
        {
            GenerationColor = (generation % 3) switch
            {
                1 => new Color(0.15f, 0.9f, 1f, 0.58f),
                2 => new Color(1f, 0.25f, 0.8f, 0.58f),
                _ => new Color(0.45f, 1f, 0.35f, 0.58f)
            };
            Renderer actorRenderer = GetComponentInChildren<Renderer>();
            if (actorRenderer == null)
            {
                return;
            }

            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            actorRenderer.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor", GenerationColor);
            actorRenderer.SetPropertyBlock(properties);
        }

        private void OnDrawGizmos()
        {
            if (_recording == null || _recording.Count < 2)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            for (int i = 1; i < _recording.Count; i++)
            {
                Gizmos.DrawLine(
                    _recording[i - 1].ExpectedPosition,
                    _recording[i].ExpectedPosition);
            }

            Gizmos.color = new Color(1f, 0.3f, 0.8f, 0.9f);
            for (int i = 1; i < _nextFrame; i++)
            {
                Gizmos.DrawLine(_actualPath[i - 1], _actualPath[i]);
            }

            Gizmos.color = new Color(1f, 0.65f, 0.05f, 0.95f);
            for (int i = 0; i < _recording.Interactions.Count; i++)
            {
                Gizmos.DrawWireSphere(
                    _recording.Interactions[i].ExpectedActorPosition,
                    0.2f);
            }
        }
    }
}

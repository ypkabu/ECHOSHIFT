using EchoShift.Input;
using EchoShift.Interaction.Recorded;
using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Player
{
    public sealed class PlayerSimulation : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private Interactor interactor;

        private IInputSource _inputSource;

        public CharacterMotor Motor => motor;
        public Interactor Interactor => interactor;
        public InteractionExecution LastInteractionExecution { get; private set; }
        public int InteractionSuccessCount { get; private set; }
        public int InteractionFailureCount { get; private set; }

        public void Configure(MonoBehaviour source, CharacterMotor characterMotor)
        {
            inputSourceComponent = source;
            motor = characterMotor;
            ResolveInputSource();
        }

        public void Configure(
            MonoBehaviour source,
            CharacterMotor characterMotor,
            Interactor actorInteractor)
        {
            inputSourceComponent = source;
            motor = characterMotor;
            interactor = actorInteractor;
            ResolveInputSource();
        }

        public bool HasValidReferences => inputSourceComponent is IInputSource && motor != null;

        private void Awake()
        {
            ResolveInputSource();
        }

        public ReplayFrame SimulateTick(int tick, float tickDuration)
        {
            InputCommand command = CollectCommand(tick);
            SimulateMovement(command, tickDuration);
            LastInteractionExecution = default;
            if (interactor != null)
            {
                RefreshInteractionCandidate(tick);
                if (command.HasButton(InputButtonFlags.Interact))
                {
                    LastInteractionExecution = interactor.TryLiveInteraction(tick);
                    CompleteInteraction(LastInteractionExecution);
                }
            }

            return CaptureFrame(command);
        }

        public InputCommand CollectCommand(int tick)
        {
            return _inputSource.Sample(tick);
        }

        public void SimulateMovement(InputCommand command, float tickDuration)
        {
            motor.Simulate(command.Move, tickDuration);
        }

        public void RefreshInteractionCandidate(int tick)
        {
            interactor?.RefreshCandidate(tick);
        }

        public bool TryCreateInteractionRequest(
            InputCommand command,
            ActorSimulationOrder actorOrder,
            out InteractionRequest request)
        {
            request = default;
            LastInteractionExecution = default;
            if (!command.HasButton(InputButtonFlags.Interact))
            {
                return false;
            }

            if (interactor == null)
            {
                LastInteractionExecution = InteractionExecution.Failure(
                    new InteractionCommand(
                        command.Tick,
                        InteractionKind.None,
                        string.Empty,
                        motor.Position),
                    InteractionFailureReason.MissingInteractor);
                CompleteInteraction(LastInteractionExecution);
                return false;
            }

            if (!interactor.TryCreateLiveRequest(
                    command.Tick,
                    actorOrder,
                    out request,
                    out InteractionExecution rejected))
            {
                LastInteractionExecution = rejected;
                CompleteInteraction(rejected);
                return false;
            }

            return true;
        }

        public void CompleteInteraction(InteractionExecution execution)
        {
            LastInteractionExecution = execution;
            if (execution.Succeeded)
            {
                InteractionSuccessCount++;
            }
            else if (execution.Command.Kind != InteractionKind.None ||
                     execution.FailureReason != InteractionFailureReason.None)
            {
                InteractionFailureCount++;
            }
        }

        public ReplayFrame CaptureFrame(InputCommand command)
        {
            return new ReplayFrame(command, motor.Position, motor.Rotation);
        }

        public void ResetInteractionStatistics()
        {
            LastInteractionExecution = default;
            InteractionSuccessCount = 0;
            InteractionFailureCount = 0;
        }

        public void ReleaseCarriedForReset()
        {
            interactor?.ReleaseCarriedForReset();
        }

        private void ResolveInputSource()
        {
            _inputSource = inputSourceComponent as IInputSource;
        }
    }
}

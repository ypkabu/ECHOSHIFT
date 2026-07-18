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
            InputCommand command = _inputSource.Sample(tick);
            motor.Simulate(command.Move, tickDuration);
            LastInteractionExecution = default;
            if (interactor != null)
            {
                interactor.RefreshCandidate(tick);
                if (command.HasButton(InputButtonFlags.Interact))
                {
                    LastInteractionExecution = interactor.TryLiveInteraction(tick);
                }
            }

            return new ReplayFrame(command, motor.Position, motor.Rotation);
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

using EchoShift.Input;
using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Player
{
    public sealed class PlayerSimulation : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour inputSourceComponent;
        [SerializeField] private CharacterMotor motor;

        private IInputSource _inputSource;

        public CharacterMotor Motor => motor;

        public void Configure(MonoBehaviour source, CharacterMotor characterMotor)
        {
            inputSourceComponent = source;
            motor = characterMotor;
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
            return new ReplayFrame(command, motor.Position, motor.Rotation);
        }

        private void ResolveInputSource()
        {
            _inputSource = inputSourceComponent as IInputSource;
        }
    }
}

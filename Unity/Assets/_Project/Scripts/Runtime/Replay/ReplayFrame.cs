using UnityEngine;

namespace EchoShift.Replay
{
    public readonly struct ReplayFrame
    {
        public ReplayFrame(
            InputCommand command,
            Vector3 expectedPosition,
            Quaternion expectedRotation)
        {
            Command = command;
            ExpectedPosition = expectedPosition;
            ExpectedRotation = expectedRotation;
        }

        public InputCommand Command { get; }
        public Vector3 ExpectedPosition { get; }
        public Quaternion ExpectedRotation { get; }
    }
}

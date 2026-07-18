using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Replay
{
    [RequireComponent(typeof(CharacterMotor), typeof(LoopActor))]
    public sealed class EchoPlayback : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private LoopActor loopActor;

        private ReplayRecording _recording;
        private ReplayDriftMonitor _driftMonitor;
        private Vector3[] _actualPath;
        private int _nextFrame;

        public int PlaybackTick => _nextFrame;
        public int RecordingLength => _recording?.Count ?? 0;
        public LoopActor Actor => loopActor;
        public CharacterMotor Motor => motor;
        public float CurrentDrift => _driftMonitor?.CurrentDrift ?? 0f;
        public float MaximumDrift => _driftMonitor?.MaximumDrift ?? 0f;
        public bool ExceededTolerance => _driftMonitor?.ExceededTolerance ?? false;

        public void Initialize(
            ReplayRecording recording,
            CharacterMotor characterMotor,
            LoopActor actor,
            float driftTolerance)
        {
            _recording = recording;
            motor = characterMotor;
            loopActor = actor;
            _driftMonitor = new ReplayDriftMonitor(driftTolerance);
            _actualPath = new Vector3[recording.Count];
            _nextFrame = 0;
        }

        public void SimulateTick(float tickDuration)
        {
            if (_recording == null || _nextFrame >= _recording.Count)
            {
                return;
            }

            ReplayFrame frame = _recording[_nextFrame];
            motor.Simulate(frame.Command.Move, tickDuration);
            _driftMonitor.Measure(frame.ExpectedPosition, motor.Position);
            _actualPath[_nextFrame] = motor.Position;
            _nextFrame++;
        }

        public void Rewind(Vector3 position, Quaternion rotation)
        {
            motor.ResetPose(position, rotation);
            _nextFrame = 0;
            _driftMonitor?.Reset();
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
        }
    }
}

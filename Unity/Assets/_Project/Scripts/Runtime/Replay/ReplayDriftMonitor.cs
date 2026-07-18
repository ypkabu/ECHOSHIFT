using UnityEngine;

namespace EchoShift.Replay
{
    public sealed class ReplayDriftMonitor
    {
        private readonly float _tolerance;

        public ReplayDriftMonitor(float tolerance)
        {
            _tolerance = tolerance;
        }

        public float CurrentDrift { get; private set; }
        public float MaximumDrift { get; private set; }
        public bool ExceededTolerance { get; private set; }

        public void Measure(Vector3 expectedPosition, Vector3 actualPosition)
        {
            CurrentDrift = Vector3.Distance(expectedPosition, actualPosition);
            if (CurrentDrift > MaximumDrift)
            {
                MaximumDrift = CurrentDrift;
            }

            ExceededTolerance |= CurrentDrift > _tolerance;
        }

        public void Reset()
        {
            CurrentDrift = 0f;
            MaximumDrift = 0f;
            ExceededTolerance = false;
        }
    }
}

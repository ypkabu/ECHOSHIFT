using UnityEngine;

namespace EchoShift.Core
{
    [CreateAssetMenu(fileName = "LoopSettings", menuName = "ECHO SHIFT/Loop Settings")]
    public sealed class LoopSettings : ScriptableObject
    {
        [SerializeField, Min(1)] private int tickRate = 60;
        [SerializeField, Min(1)] private int loopDurationSeconds = 10;
        [SerializeField, Range(1, 3)] private int maxEchoes = 3;
        [SerializeField, Min(0.01f)] private float moveSpeed = 4f;
        [SerializeField, Min(0.001f)] private float driftTolerance = 0.05f;
        [SerializeField, Range(1, 32)] private int maxCatchUpTicksPerFrame = 8;

        public int TickRate => tickRate;
        public int LoopDurationSeconds => loopDurationSeconds;
        public int MaxEchoes => maxEchoes;
        public float MoveSpeed => moveSpeed;
        public float DriftTolerance => driftTolerance;
        public int MaxCatchUpTicksPerFrame => maxCatchUpTicksPerFrame;
        public float TickDuration => 1f / tickRate;
        public int MaxTicks => tickRate * loopDurationSeconds;

        public bool TryValidate(out string error)
        {
            if (tickRate <= 0)
            {
                error = "Tick rate must be greater than zero.";
                return false;
            }

            if (loopDurationSeconds <= 0)
            {
                error = "Loop duration must be greater than zero.";
                return false;
            }

            long maxTicks = (long)tickRate * loopDurationSeconds;
            if (maxTicks > int.MaxValue)
            {
                error = "The configured loop contains too many ticks.";
                return false;
            }

            if (maxEchoes < 1 || maxEchoes > 3)
            {
                error = "Phase 0 supports between one and three Echoes.";
                return false;
            }

            if (moveSpeed <= 0f)
            {
                error = "Move speed must be greater than zero.";
                return false;
            }

            if (driftTolerance <= 0f)
            {
                error = "Drift tolerance must be greater than zero.";
                return false;
            }

            if (maxCatchUpTicksPerFrame <= 0)
            {
                error = "Catch-up tick count must be greater than zero.";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}

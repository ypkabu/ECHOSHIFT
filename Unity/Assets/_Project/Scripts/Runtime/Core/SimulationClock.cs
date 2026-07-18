using System;

namespace EchoShift.Core
{
    public sealed class SimulationClock
    {
        private readonly double _tickDuration;
        private readonly double _maximumAccumulatedTime;
        private double _accumulatedTime;

        public SimulationClock(int tickRate, int maxCatchUpTicksPerFrame)
        {
            if (tickRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tickRate));
            }

            if (maxCatchUpTicksPerFrame <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCatchUpTicksPerFrame));
            }

            _tickDuration = 1d / tickRate;
            _maximumAccumulatedTime = _tickDuration * maxCatchUpTicksPerFrame;
        }

        public bool IsPaused { get; private set; }

        public void AddTime(float unscaledDeltaTime)
        {
            if (IsPaused || unscaledDeltaTime <= 0f)
            {
                return;
            }

            _accumulatedTime = Math.Min(
                _accumulatedTime + unscaledDeltaTime,
                _maximumAccumulatedTime);
        }

        public bool TryConsumeTick()
        {
            if (IsPaused || _accumulatedTime + 1e-9d < _tickDuration)
            {
                return false;
            }

            _accumulatedTime -= _tickDuration;
            return true;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Reset()
        {
            _accumulatedTime = 0d;
        }
    }
}

using System;

namespace EchoShift.Core
{
    public enum LoopEndReason : byte
    {
        Timer,
        Manual,
        Goal,
        Test
    }

    public enum ReplayHistoryState : byte
    {
        Active,
        Evicted
    }

    public readonly struct LoopHistorySummary
    {
        public LoopHistorySummary(
            int loopNumber,
            int recordedTicks,
            int interactionEventCount,
            int replayGeneration,
            float maximumDrift,
            int interactionSuccessCount,
            int interactionFailureCount,
            ReplayHistoryState state,
            LoopEndReason endReason)
        {
            LoopNumber = loopNumber;
            RecordedTicks = recordedTicks;
            InteractionEventCount = interactionEventCount;
            ReplayGeneration = replayGeneration;
            MaximumDrift = maximumDrift;
            InteractionSuccessCount = interactionSuccessCount;
            InteractionFailureCount = interactionFailureCount;
            State = state;
            EndReason = endReason;
        }

        public int LoopNumber { get; }
        public int RecordedTicks { get; }
        public int InteractionEventCount { get; }
        public int ReplayGeneration { get; }
        public float MaximumDrift { get; }
        public int InteractionSuccessCount { get; }
        public int InteractionFailureCount { get; }
        public ReplayHistoryState State { get; }
        public LoopEndReason EndReason { get; }

        public LoopHistorySummary WithRuntimeResults(
            float maximumDrift,
            int successCount,
            int failureCount)
        {
            return new LoopHistorySummary(
                LoopNumber,
                RecordedTicks,
                InteractionEventCount,
                ReplayGeneration,
                maximumDrift,
                successCount,
                failureCount,
                State,
                EndReason);
        }

        public LoopHistorySummary WithState(ReplayHistoryState state)
        {
            return new LoopHistorySummary(
                LoopNumber,
                RecordedTicks,
                InteractionEventCount,
                ReplayGeneration,
                MaximumDrift,
                InteractionSuccessCount,
                InteractionFailureCount,
                state,
                EndReason);
        }
    }

    public sealed class LoopHistory
    {
        public const int DefaultCapacity = 16;

        private readonly LoopHistorySummary[] _summaries;
        private int _count;

        public LoopHistory(int capacity = DefaultCapacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _summaries = new LoopHistorySummary[capacity];
        }

        public int Count => _count;
        public int Capacity => _summaries.Length;
        public LoopHistorySummary this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _summaries[index];
            }
        }

        public void Add(LoopHistorySummary summary)
        {
            if (_count == _summaries.Length)
            {
                Array.Copy(_summaries, 1, _summaries, 0, _summaries.Length - 1);
                _count--;
            }

            _summaries[_count] = summary;
            _count++;
        }

        public bool UpdateRuntimeResults(
            int replayGeneration,
            float maximumDrift,
            int successCount,
            int failureCount)
        {
            int index = FindGeneration(replayGeneration);
            if (index < 0)
            {
                return false;
            }

            _summaries[index] = _summaries[index].WithRuntimeResults(
                maximumDrift,
                successCount,
                failureCount);
            return true;
        }

        public bool MarkEvicted(int replayGeneration)
        {
            int index = FindGeneration(replayGeneration);
            if (index < 0)
            {
                return false;
            }

            _summaries[index] = _summaries[index].WithState(ReplayHistoryState.Evicted);
            return true;
        }

        private int FindGeneration(int replayGeneration)
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                if (_summaries[i].ReplayGeneration == replayGeneration)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

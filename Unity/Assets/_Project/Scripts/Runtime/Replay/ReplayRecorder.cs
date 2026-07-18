using System;

namespace EchoShift.Replay
{
    public enum ReplayRecordResult
    {
        Recorded,
        Full,
        Finalized,
        OutOfOrder
    }

    public sealed class ReplayRecorder
    {
        private readonly ReplayFrame[] _buffer;
        private ReplayRecording _finalizedRecording;
        private int _count;

        public ReplayRecorder(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _buffer = new ReplayFrame[capacity];
        }

        public int Capacity => _buffer.Length;
        public int Count => _count;
        public bool IsFinalized => _finalizedRecording != null;

        public ReplayRecordResult TryRecord(ReplayFrame frame)
        {
            if (IsFinalized)
            {
                return ReplayRecordResult.Finalized;
            }

            if (_count >= _buffer.Length)
            {
                return ReplayRecordResult.Full;
            }

            if (frame.Command.Tick != _count)
            {
                return ReplayRecordResult.OutOfOrder;
            }

            _buffer[_count] = frame;
            _count++;
            return ReplayRecordResult.Recorded;
        }

        public ReplayRecording FinalizeRecording()
        {
            if (_finalizedRecording != null)
            {
                return _finalizedRecording;
            }

            ReplayFrame[] immutableFrames = new ReplayFrame[_count];
            Array.Copy(_buffer, immutableFrames, _count);
            _finalizedRecording = new ReplayRecording(immutableFrames);
            return _finalizedRecording;
        }
    }
}

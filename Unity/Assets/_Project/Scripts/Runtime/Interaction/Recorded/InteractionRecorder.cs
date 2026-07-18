using System;

namespace EchoShift.Interaction.Recorded
{
    public enum InteractionRecordResult
    {
        Recorded,
        Full,
        Finalized,
        OutOfOrder,
        OutsideRecordedFrames
    }

    public sealed class InteractionRecorder
    {
        private readonly InteractionCommand[] _buffer;
        private InteractionRecording _finalized;
        private int _count;

        public InteractionRecorder(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _buffer = new InteractionCommand[capacity];
        }

        public int Count => _count;
        public bool IsFinalized => _finalized != null;

        public InteractionRecordResult TryRecord(
            InteractionCommand command,
            int recordedFrameCount)
        {
            if (IsFinalized)
            {
                return InteractionRecordResult.Finalized;
            }

            if (_count >= _buffer.Length)
            {
                return InteractionRecordResult.Full;
            }

            if (command.Tick < 0 || command.Tick >= recordedFrameCount)
            {
                return InteractionRecordResult.OutsideRecordedFrames;
            }

            if (_count > 0 && command.Tick <= _buffer[_count - 1].Tick)
            {
                return InteractionRecordResult.OutOfOrder;
            }

            _buffer[_count] = command;
            _count++;
            return InteractionRecordResult.Recorded;
        }

        public InteractionRecording FinalizeRecording()
        {
            if (_finalized != null)
            {
                return _finalized;
            }

            InteractionCommand[] immutableCommands = new InteractionCommand[_count];
            Array.Copy(_buffer, immutableCommands, _count);
            _finalized = new InteractionRecording(immutableCommands);
            return _finalized;
        }
    }
}

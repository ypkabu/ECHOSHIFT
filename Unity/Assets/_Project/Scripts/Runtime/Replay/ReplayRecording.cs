using System;

namespace EchoShift.Replay
{
    public sealed class ReplayRecording
    {
        private readonly ReplayFrame[] _frames;

        internal ReplayRecording(ReplayFrame[] frames)
        {
            _frames = frames ?? throw new ArgumentNullException(nameof(frames));
        }

        public int Count => _frames.Length;
        public ReplayFrame this[int index] => _frames[index];
    }
}

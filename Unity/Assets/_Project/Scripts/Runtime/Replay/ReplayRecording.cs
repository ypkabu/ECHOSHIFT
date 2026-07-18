using System;
using EchoShift.Interaction.Recorded;

namespace EchoShift.Replay
{
    public sealed class ReplayRecording
    {
        private readonly ReplayFrame[] _frames;

        internal ReplayRecording(
            ReplayFrame[] frames,
            InteractionRecording interactions)
        {
            _frames = frames ?? throw new ArgumentNullException(nameof(frames));
            Interactions = interactions ?? throw new ArgumentNullException(nameof(interactions));
        }

        public int Count => _frames.Length;
        public InteractionRecording Interactions { get; }
        public ReplayFrame this[int index] => _frames[index];
    }
}

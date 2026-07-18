using System;

namespace EchoShift.Interaction.Recorded
{
    public sealed class InteractionRecording
    {
        private readonly InteractionCommand[] _commands;

        internal InteractionRecording(InteractionCommand[] commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        public int Count => _commands.Length;
        public InteractionCommand this[int index] => _commands[index];
    }
}

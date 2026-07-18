using System;
using UnityEngine;

namespace EchoShift.Replay
{
    [Flags]
    public enum InputButtonFlags : byte
    {
        None = 0,
        EndLoop = 1 << 0,
        Interact = 1 << 1
    }

    public readonly struct InputCommand
    {
        public InputCommand(int tick, Vector2 move, InputButtonFlags buttons)
        {
            Tick = tick;
            Move = move;
            Buttons = buttons;
        }

        public int Tick { get; }
        public Vector2 Move { get; }
        public InputButtonFlags Buttons { get; }

        public bool HasButton(InputButtonFlags button)
        {
            return (Buttons & button) != 0;
        }
    }
}

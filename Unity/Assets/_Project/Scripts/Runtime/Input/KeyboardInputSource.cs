using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Input
{
    public sealed class KeyboardInputSource : MonoBehaviour, IInputSource
    {
        private bool _endLoopLatched;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                _endLoopLatched = true;
            }
        }

        public InputCommand Sample(int tick)
        {
            float x = 0f;
            float y = 0f;

            if (UnityEngine.Input.GetKey(KeyCode.A))
            {
                x -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.D))
            {
                x += 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.S))
            {
                y -= 1f;
            }

            if (UnityEngine.Input.GetKey(KeyCode.W))
            {
                y += 1f;
            }

            Vector2 move = new Vector2(x, y);
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            InputButtonFlags buttons = InputButtonFlags.None;
            if (_endLoopLatched)
            {
                buttons |= InputButtonFlags.EndLoop;
                _endLoopLatched = false;
            }

            return new InputCommand(tick, move, buttons);
        }
    }
}

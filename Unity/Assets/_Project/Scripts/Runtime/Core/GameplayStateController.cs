using System;

namespace EchoShift.Core
{
    public enum GameplayState : byte
    {
        Booting,
        Playing,
        LoopTransition,
        SectionTransition,
        Paused,
        Completed
    }

    public sealed class GameplayStateController
    {
        public GameplayState State { get; private set; } = GameplayState.Booting;
        public string LastReason { get; private set; } = "Boot";
        public int Revision { get; private set; }

        public bool TryTransition(GameplayState next, string reason)
        {
            if (next == State || !IsAllowed(State, next))
            {
                return false;
            }

            State = next;
            LastReason = string.IsNullOrEmpty(reason) ? next.ToString() : reason;
            Revision++;
            return true;
        }

        public void Reset()
        {
            State = GameplayState.Booting;
            LastReason = "RestartGame";
            Revision++;
        }

        public static bool IsAllowed(GameplayState current, GameplayState next)
        {
            return current switch
            {
                GameplayState.Booting => next == GameplayState.Playing,
                GameplayState.Playing => next == GameplayState.LoopTransition ||
                                         next == GameplayState.SectionTransition ||
                                         next == GameplayState.Paused ||
                                         next == GameplayState.Completed,
                GameplayState.LoopTransition => next == GameplayState.Playing ||
                                                next == GameplayState.SectionTransition,
                GameplayState.SectionTransition => next == GameplayState.Playing ||
                                                   next == GameplayState.Completed,
                GameplayState.Paused => next == GameplayState.Playing ||
                                        next == GameplayState.SectionTransition ||
                                        next == GameplayState.Completed,
                GameplayState.Completed => next == GameplayState.Paused,
                _ => throw new ArgumentOutOfRangeException(nameof(current))
            };
        }
    }
}

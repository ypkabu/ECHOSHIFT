using System;

namespace EchoShift.Player
{
    public readonly struct ActorSimulationOrder :
        IComparable<ActorSimulationOrder>,
        IEquatable<ActorSimulationOrder>
    {
        public ActorSimulationOrder(LoopActorKind kind, int replayGeneration)
        {
            Kind = kind;
            ReplayGeneration = replayGeneration < 0 ? 0 : replayGeneration;
        }

        public LoopActorKind Kind { get; }
        public int ReplayGeneration { get; }

        public int CompareTo(ActorSimulationOrder other)
        {
            if (Kind != other.Kind)
            {
                return Kind == LoopActorKind.Echo ? -1 : 1;
            }

            return ReplayGeneration.CompareTo(other.ReplayGeneration);
        }

        public bool Equals(ActorSimulationOrder other)
        {
            return Kind == other.Kind && ReplayGeneration == other.ReplayGeneration;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorSimulationOrder other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Kind * 397) ^ ReplayGeneration;
        }

        public static bool operator ==(
            ActorSimulationOrder left,
            ActorSimulationOrder right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            ActorSimulationOrder left,
            ActorSimulationOrder right)
        {
            return !left.Equals(right);
        }
    }
}

using UnityEngine;

namespace EchoShift.Player
{
    public enum LoopActorKind
    {
        Player,
        Echo
    }

    public sealed class LoopActor : MonoBehaviour
    {
        [SerializeField] private LoopActorKind kind;
        [SerializeField, Min(0)] private int replayGeneration;

        public LoopActorKind Kind => kind;
        public int ReplayGeneration => replayGeneration;
        public ActorSimulationOrder SimulationOrder =>
            new ActorSimulationOrder(kind, replayGeneration);

        public void Configure(LoopActorKind actorKind)
        {
            Configure(actorKind, 0);
        }

        public void Configure(LoopActorKind actorKind, int generation)
        {
            kind = actorKind;
            replayGeneration = Mathf.Max(0, generation);
        }
    }
}

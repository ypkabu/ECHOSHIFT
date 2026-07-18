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

        public LoopActorKind Kind => kind;

        public void Configure(LoopActorKind actorKind)
        {
            kind = actorKind;
        }
    }
}

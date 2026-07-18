using System;
using EchoShift.Player;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class GoalVolume : MonoBehaviour, IResettable
    {
        public event Action<LoopActor> ActorEntered;

        public bool IsReached { get; private set; }

        public void CaptureInitialState()
        {
            IsReached = false;
        }

        public void RestoreInitialState()
        {
            IsReached = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            LoopActor actor = other.GetComponentInParent<LoopActor>();
            if (actor == null)
            {
                return;
            }

            ActorEntered?.Invoke(actor);
            if (actor.Kind == LoopActorKind.Player)
            {
                IsReached = true;
            }
        }
    }
}

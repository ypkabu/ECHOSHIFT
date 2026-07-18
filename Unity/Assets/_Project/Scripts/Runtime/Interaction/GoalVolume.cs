using System;
using EchoShift.Player;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class GoalVolume : MonoBehaviour, IResettable
    {
        private readonly Collider[] _overlapBuffer = new Collider[16];
        private BoxCollider _boxCollider;
        private bool _playerInside;

        public event Action<LoopActor> ActorEntered;

        public bool IsReached { get; private set; }

        public void CaptureInitialState()
        {
            IsReached = false;
            _playerInside = false;
        }

        public void RestoreInitialState()
        {
            IsReached = false;
            _playerInside = false;
        }

        public void RefreshFromPhysics()
        {
            _boxCollider ??= GetComponent<BoxCollider>();
            if (_boxCollider == null)
            {
                return;
            }

            Vector3 scale = transform.lossyScale;
            Vector3 halfExtents = Vector3.Scale(
                _boxCollider.size,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * 0.5f;
            int count = Physics.OverlapBoxNonAlloc(
                transform.TransformPoint(_boxCollider.center),
                halfExtents,
                _overlapBuffer,
                transform.rotation,
                ~0,
                QueryTriggerInteraction.Collide);
            LoopActor player = null;
            for (int i = 0; i < count; i++)
            {
                LoopActor actor = _overlapBuffer[i].GetComponentInParent<LoopActor>();
                if (actor != null && actor.Kind == LoopActorKind.Player &&
                    actor.isActiveAndEnabled && actor.gameObject.activeInHierarchy)
                {
                    player = actor;
                }

                _overlapBuffer[i] = null;
            }

            bool isInside = player != null;
            if (isInside && !_playerInside)
            {
                ActorEntered?.Invoke(player);
            }

            _playerInside = isInside;
            IsReached |= isInside;
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
                _playerInside = true;
            }
        }
    }
}

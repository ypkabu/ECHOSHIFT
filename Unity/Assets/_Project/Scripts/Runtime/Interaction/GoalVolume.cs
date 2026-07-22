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
        private Collider _triggerCollider;
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
            _triggerCollider ??= GetComponent<Collider>();
            if (_triggerCollider == null)
            {
                return;
            }

            int count;
            if (_triggerCollider is BoxCollider box)
            {
                Vector3 scale = transform.lossyScale;
                Vector3 halfExtents = Vector3.Scale(
                    box.size,
                    new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z))) * 0.5f;
                count = Physics.OverlapBoxNonAlloc(
                    transform.TransformPoint(box.center), halfExtents,
                    _overlapBuffer, transform.rotation, ~0,
                    QueryTriggerInteraction.Collide);
            }
            else if (_triggerCollider is CapsuleCollider capsule)
            {
                Vector3 scale = transform.lossyScale;
                Vector3 axis;
                float axisScale;
                float radiusScale;
                if (capsule.direction == 0)
                {
                    axis = transform.right;
                    axisScale = Mathf.Abs(scale.x);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                }
                else if (capsule.direction == 2)
                {
                    axis = transform.forward;
                    axisScale = Mathf.Abs(scale.z);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                }
                else
                {
                    axis = transform.up;
                    axisScale = Mathf.Abs(scale.y);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                }

                Vector3 center = transform.TransformPoint(capsule.center);
                float radius = capsule.radius * radiusScale;
                float halfLine = Mathf.Max(capsule.height * axisScale * 0.5f - radius, 0f);
                count = Physics.OverlapCapsuleNonAlloc(
                    center - axis * halfLine, center + axis * halfLine, radius,
                    _overlapBuffer, ~0, QueryTriggerInteraction.Collide);
            }
            else
            {
                Bounds bounds = _triggerCollider.bounds;
                count = Physics.OverlapBoxNonAlloc(
                    bounds.center, bounds.extents, _overlapBuffer,
                    Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            }
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

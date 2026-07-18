using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    [RequireComponent(typeof(StableId), typeof(Collider), typeof(Rigidbody))]
    public sealed class CarryableBattery : MonoBehaviour, IInteractable, IResettable
    {
        [SerializeField] private StableId stableId;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private Rigidbody body;

        private Transform _initialParent;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private bool _hasCapturedState;
        private Transform _carrySocket;

        public StableId StableId => stableId;
        public string InteractionName => name;
        public Transform InteractionTransform => transform;
        public Interactor Holder { get; private set; }
        public PowerSocket InsertedSocket { get; private set; }
        public bool IsHeld => Holder != null;
        public bool IsInserted => InsertedSocket != null;

        public void Configure(
            StableId identity,
            Collider batteryCollider,
            Rigidbody rigidbody)
        {
            stableId = identity;
            interactionCollider = batteryCollider;
            body = rigidbody;
        }

        public InteractionKind GetDefaultInteraction(in InteractionContext context)
        {
            return Holder == context.Interactor
                ? InteractionKind.DropBattery
                : InteractionKind.PickupBattery;
        }

        public bool CanInteract(
            in InteractionContext context,
            InteractionKind kind,
            out InteractionFailureReason failureReason)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                failureReason = InteractionFailureReason.TargetInactive;
                return false;
            }

            switch (kind)
            {
                case InteractionKind.PickupBattery:
                    if (Holder != null && Holder != context.Interactor)
                    {
                        failureReason = InteractionFailureReason.HeldByAnotherActor;
                        return false;
                    }

                    if (InsertedSocket != null || Holder == context.Interactor)
                    {
                        failureReason = InteractionFailureReason.TargetUnavailable;
                        return false;
                    }

                    if (!IsWithinRange(context))
                    {
                        failureReason = InteractionFailureReason.OutOfRange;
                        return false;
                    }

                    failureReason = InteractionFailureReason.None;
                    return true;

                case InteractionKind.DropBattery:
                    if (Holder != context.Interactor)
                    {
                        failureReason = InteractionFailureReason.ActorNotCarrying;
                        return false;
                    }

                    failureReason = InteractionFailureReason.None;
                    return true;

                default:
                    failureReason = InteractionFailureReason.UnsupportedInteraction;
                    return false;
            }
        }

        public bool TryInteract(
            in InteractionContext context,
            InteractionKind kind,
            out InteractionFailureReason failureReason)
        {
            if (!CanInteract(context, kind, out failureReason))
            {
                return false;
            }

            if (kind == InteractionKind.PickupBattery)
            {
                return context.Interactor.TryAcquireBattery(this, out failureReason);
            }

            if (kind == InteractionKind.DropBattery)
            {
                return context.Interactor.TryDropBattery(this, out failureReason);
            }

            failureReason = InteractionFailureReason.UnsupportedInteraction;
            return false;
        }

        public void CaptureInitialState()
        {
            _initialParent = transform.parent;
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            _hasCapturedState = true;
        }

        public void RestoreInitialState()
        {
            if (!_hasCapturedState)
            {
                return;
            }

            SeverRelationships();
            transform.SetParent(_initialParent, true);
            transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            SetCollisionEnabled(true);
        }

        internal bool AttachToInteractor(
            Interactor interactor,
            Transform carrySocket,
            out InteractionFailureReason failureReason)
        {
            if (Holder != null)
            {
                failureReason = InteractionFailureReason.HeldByAnotherActor;
                return false;
            }

            if (InsertedSocket != null)
            {
                failureReason = InteractionFailureReason.TargetUnavailable;
                return false;
            }

            Holder = interactor;
            _carrySocket = carrySocket;
            transform.SetParent(_initialParent, true);
            FollowCarrySocket();
            SetCollisionEnabled(false);
            failureReason = InteractionFailureReason.None;
            return true;
        }

        internal void DetachAndPlace(Vector3 contextPosition, Vector3 contextForward)
        {
            Holder = null;
            _carrySocket = null;
            transform.SetParent(_initialParent, true);
            Vector3 position = contextPosition + (contextForward.normalized * 0.9f);
            position.y = _hasCapturedState ? _initialPosition.y : transform.position.y;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            SetCollisionEnabled(true);
        }

        internal bool InsertInto(
            Interactor interactor,
            PowerSocket socket,
            Transform insertionTransform,
            out InteractionFailureReason failureReason)
        {
            if (Holder != interactor || !interactor.TryDetachForInsertion(this))
            {
                failureReason = InteractionFailureReason.ActorNotCarrying;
                return false;
            }

            Holder = null;
            _carrySocket = null;
            InsertedSocket = socket;
            transform.SetParent(insertionTransform, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            SetCollisionEnabled(false);
            failureReason = InteractionFailureReason.None;
            return true;
        }

        internal void ReleaseFromInteractorForReset(Interactor interactor)
        {
            if (Holder != interactor)
            {
                return;
            }

            Holder = null;
            _carrySocket = null;
            transform.SetParent(_initialParent, true);
            SetCollisionEnabled(true);
        }

        internal void ReleaseFromSocketForReset(PowerSocket socket)
        {
            if (InsertedSocket == socket)
            {
                InsertedSocket = null;
            }
        }

        private bool IsWithinRange(in InteractionContext context)
        {
            return Vector3.SqrMagnitude(transform.position - context.ActorPosition) <=
                   context.Range * context.Range;
        }

        private void SetCollisionEnabled(bool isEnabled)
        {
            if (interactionCollider != null)
            {
                interactionCollider.enabled = isEnabled;
            }

            if (body != null)
            {
                body.detectCollisions = isEnabled;
                body.isKinematic = true;
                body.useGravity = false;
            }
        }

        private void SeverRelationships()
        {
            if (Holder != null)
            {
                Holder.NotifyBatteryDetached(this);
                Holder = null;
                _carrySocket = null;
            }

            if (InsertedSocket != null)
            {
                PowerSocket socket = InsertedSocket;
                InsertedSocket = null;
                socket.NotifyBatteryDetached(this);
            }
        }

        private void OnDisable()
        {
            stableId?.UnregisterTarget();
            SeverRelationships();
        }

        private void OnEnable()
        {
            stableId?.RegisterTarget();
        }

        private void LateUpdate()
        {
            FollowCarrySocket();
        }

        private void FollowCarrySocket()
        {
            if (Holder != null && _carrySocket != null)
            {
                transform.SetPositionAndRotation(_carrySocket.position, _carrySocket.rotation);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.05f, 0.8f);
            Vector3 position = _hasCapturedState ? _initialPosition : transform.position;
            Gizmos.DrawWireCube(position, new Vector3(0.7f, 0.4f, 1f));
        }
    }
}

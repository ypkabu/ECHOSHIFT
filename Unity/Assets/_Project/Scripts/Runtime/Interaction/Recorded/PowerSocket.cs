using System;
using EchoShift.Reset;
using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    [RequireComponent(typeof(StableId), typeof(Collider))]
    public sealed class PowerSocket : MonoBehaviour, IInteractable, IResettable, IDoorOpenSource
    {
        [SerializeField] private StableId stableId;
        [SerializeField] private Transform insertionTransform;
        [SerializeField] private bool requireReplayForDoor;

        private bool _insertedByReplay;

        public event Action<bool> PoweredChanged;

        public StableId StableId => stableId;
        public string InteractionName => name;
        public Transform InteractionTransform => transform;
        public CarryableBattery InsertedBattery { get; private set; }
        public bool IsPowered => InsertedBattery != null;
        public bool RequireReplayForDoor => requireReplayForDoor;
        public bool InsertedByReplay => _insertedByReplay;
        public bool RequestsDoorOpen => IsPowered &&
            (!requireReplayForDoor || _insertedByReplay);

        public void Configure(
            StableId identity,
            Transform socketTransform,
            bool replayRequiredForDoor = false)
        {
            stableId = identity;
            insertionTransform = socketTransform;
            requireReplayForDoor = replayRequiredForDoor;
        }

        public InteractionKind GetDefaultInteraction(in InteractionContext context)
        {
            return InteractionKind.InsertBattery;
        }

        public bool CanInteract(
            in InteractionContext context,
            InteractionKind kind,
            out InteractionFailureReason failureReason)
        {
            if (kind != InteractionKind.InsertBattery)
            {
                failureReason = InteractionFailureReason.UnsupportedInteraction;
                return false;
            }

            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                failureReason = InteractionFailureReason.TargetInactive;
                return false;
            }

            if (InsertedBattery != null)
            {
                failureReason = InteractionFailureReason.SocketOccupied;
                return false;
            }

            if (context.Interactor == null || context.Interactor.CarriedBattery == null)
            {
                failureReason = InteractionFailureReason.ActorNotCarrying;
                return false;
            }

            if (Vector3.SqrMagnitude(transform.position - context.ActorPosition) >
                context.Range * context.Range)
            {
                failureReason = InteractionFailureReason.OutOfRange;
                return false;
            }

            failureReason = InteractionFailureReason.None;
            return true;
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

            CarryableBattery battery = context.Interactor.CarriedBattery;
            if (!battery.InsertInto(
                    context.Interactor,
                    this,
                    insertionTransform,
                    out failureReason))
            {
                return false;
            }

            InsertedBattery = battery;
            _insertedByReplay = context.IsReplay;
            PoweredChanged?.Invoke(true);
            return true;
        }

        public void CaptureInitialState()
        {
            ClearInsertedBattery();
        }

        public void RestoreInitialState()
        {
            ClearInsertedBattery();
        }

        internal void NotifyBatteryDetached(CarryableBattery battery)
        {
            if (InsertedBattery != battery)
            {
                return;
            }

            InsertedBattery = null;
            _insertedByReplay = false;
            PoweredChanged?.Invoke(false);
        }

        private void ClearInsertedBattery()
        {
            if (InsertedBattery == null)
            {
                _insertedByReplay = false;
                return;
            }

            CarryableBattery battery = InsertedBattery;
            InsertedBattery = null;
            _insertedByReplay = false;
            battery.ReleaseFromSocketForReset(this);
            PoweredChanged?.Invoke(false);
        }

        private void OnDisable()
        {
            stableId?.UnregisterTarget();
            ClearInsertedBattery();
        }

        private void OnEnable()
        {
            stableId?.RegisterTarget();
        }
    }
}

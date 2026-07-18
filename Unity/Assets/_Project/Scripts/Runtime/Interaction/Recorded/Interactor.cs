using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    [RequireComponent(typeof(LoopActor))]
    public sealed class Interactor : MonoBehaviour
    {
        [SerializeField] private LoopActor actor;
        [SerializeField] private InteractionSensor sensor;
        [SerializeField] private Transform carrySocket;
        [SerializeField] private InteractionRegistry registry;

        private CarryableBattery _carriedBattery;

        public LoopActor Actor => actor;
        public InteractionSensor Sensor => sensor;
        public Transform CarrySocket => carrySocket;
        public CarryableBattery CarriedBattery => _carriedBattery;
        public bool HasValidReferences =>
            actor != null && sensor != null && carrySocket != null && registry != null;

        public void Configure(
            LoopActor loopActor,
            InteractionSensor interactionSensor,
            Transform actorCarrySocket,
            InteractionRegistry interactionRegistry)
        {
            actor = loopActor;
            sensor = interactionSensor;
            carrySocket = actorCarrySocket;
            registry = interactionRegistry;
        }

        public void RefreshCandidate(int tick)
        {
            if (!HasValidReferences)
            {
                return;
            }

            InteractionContext context = CreateContext(tick, false);
            sensor.RefreshCandidate(context);
        }

        public InteractionExecution TryLiveInteraction(int tick)
        {
            if (!HasValidReferences)
            {
                return InteractionExecution.Failure(
                    new InteractionCommand(tick, InteractionKind.None, string.Empty, transform.position),
                    InteractionFailureReason.MissingInteractor);
            }

            InteractionContext context = CreateContext(tick, false);
            sensor.RefreshCandidate(context);
            IInteractable candidate = sensor.CurrentTarget;
            if (candidate != null && sensor.CurrentCanInteract)
            {
                return ExecuteTarget(candidate, sensor.CurrentKind, context);
            }

            if (_carriedBattery != null)
            {
                return ExecuteTarget(_carriedBattery, InteractionKind.DropBattery, context);
            }

            string targetId = candidate?.StableId?.Value ?? string.Empty;
            InteractionCommand failedCommand = new InteractionCommand(
                tick,
                sensor.CurrentKind,
                targetId,
                context.ActorPosition);
            return InteractionExecution.Failure(
                failedCommand,
                candidate == null
                    ? InteractionFailureReason.NoCandidate
                    : sensor.CurrentFailureReason);
        }

        public InteractionExecution ExecuteRecorded(InteractionCommand command)
        {
            if (!HasValidReferences)
            {
                return InteractionExecution.Failure(
                    command,
                    InteractionFailureReason.MissingInteractor);
            }

            if (!registry.TryResolve(command.TargetStableId, out IInteractable target))
            {
                return InteractionExecution.Failure(
                    command,
                    InteractionFailureReason.TargetNotFound);
            }

            InteractionContext context = CreateContext(command.Tick, true);
            if (!target.CanInteract(context, command.Kind, out InteractionFailureReason reason))
            {
                return InteractionExecution.Failure(command, reason);
            }

            if (!target.TryInteract(context, command.Kind, out reason))
            {
                return InteractionExecution.Failure(command, reason);
            }

            return InteractionExecution.Success(command);
        }

        public void ReleaseCarriedForReset()
        {
            if (_carriedBattery == null)
            {
                return;
            }

            CarryableBattery battery = _carriedBattery;
            _carriedBattery = null;
            battery.ReleaseFromInteractorForReset(this);
        }

        internal bool TryAcquireBattery(
            CarryableBattery battery,
            out InteractionFailureReason failureReason)
        {
            if (_carriedBattery != null)
            {
                failureReason = InteractionFailureReason.ActorNotCarrying;
                return false;
            }

            if (!battery.AttachToInteractor(this, carrySocket, out failureReason))
            {
                return false;
            }

            _carriedBattery = battery;
            return true;
        }

        internal bool TryDropBattery(
            CarryableBattery battery,
            out InteractionFailureReason failureReason)
        {
            if (_carriedBattery != battery)
            {
                failureReason = InteractionFailureReason.ActorNotCarrying;
                return false;
            }

            _carriedBattery = null;
            battery.DetachAndPlace(contextPosition: transform.position, contextForward: transform.forward);
            failureReason = InteractionFailureReason.None;
            return true;
        }

        internal bool TryDetachForInsertion(CarryableBattery battery)
        {
            if (_carriedBattery != battery)
            {
                return false;
            }

            _carriedBattery = null;
            return true;
        }

        internal void NotifyBatteryDetached(CarryableBattery battery)
        {
            if (_carriedBattery == battery)
            {
                _carriedBattery = null;
            }
        }

        private InteractionExecution ExecuteTarget(
            IInteractable target,
            InteractionKind kind,
            in InteractionContext context)
        {
            string targetId = target.StableId != null
                ? target.StableId.Value
                : string.Empty;
            InteractionCommand command = new InteractionCommand(
                context.Tick,
                kind,
                targetId,
                context.ActorPosition);
            if (!target.CanInteract(context, kind, out InteractionFailureReason reason) ||
                !target.TryInteract(context, kind, out reason))
            {
                return InteractionExecution.Failure(command, reason);
            }

            return InteractionExecution.Success(command);
        }

        private InteractionContext CreateContext(int tick, bool isReplay)
        {
            return new InteractionContext(
                actor,
                this,
                tick,
                isReplay,
                sensor.InteractionRange);
        }

        private void OnDisable()
        {
            ReleaseCarriedForReset();
        }
    }
}

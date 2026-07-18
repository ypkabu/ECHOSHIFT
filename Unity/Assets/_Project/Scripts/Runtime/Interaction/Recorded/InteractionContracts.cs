using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    public enum InteractionKind : byte
    {
        None,
        PickupBattery,
        DropBattery,
        InsertBattery
    }

    public enum InteractionFailureReason : byte
    {
        None,
        NoCandidate,
        TargetNotFound,
        TargetInactive,
        OutOfRange,
        TargetUnavailable,
        HeldByAnotherActor,
        ActorNotCarrying,
        SocketOccupied,
        UnsupportedInteraction,
        MissingInteractor
    }

    public readonly struct InteractionContext
    {
        public InteractionContext(
            LoopActor actor,
            Interactor interactor,
            int tick,
            bool isReplay,
            float range)
        {
            Actor = actor;
            Interactor = interactor;
            Tick = tick;
            IsReplay = isReplay;
            Range = range;
            ActorPosition = actor != null ? actor.transform.position : Vector3.zero;
            ActorForward = actor != null ? actor.transform.forward : Vector3.forward;
        }

        public LoopActor Actor { get; }
        public Interactor Interactor { get; }
        public int Tick { get; }
        public bool IsReplay { get; }
        public float Range { get; }
        public Vector3 ActorPosition { get; }
        public Vector3 ActorForward { get; }
    }

    public readonly struct InteractionCommand
    {
        public InteractionCommand(
            int tick,
            InteractionKind kind,
            string targetStableId,
            Vector3 expectedActorPosition)
        {
            Tick = tick;
            Kind = kind;
            TargetStableId = targetStableId ?? string.Empty;
            ExpectedActorPosition = expectedActorPosition;
        }

        public int Tick { get; }
        public InteractionKind Kind { get; }
        public string TargetStableId { get; }
        public Vector3 ExpectedActorPosition { get; }
    }

    public readonly struct InteractionExecution
    {
        private InteractionExecution(
            bool succeeded,
            InteractionCommand command,
            InteractionFailureReason failureReason)
        {
            Succeeded = succeeded;
            Command = command;
            FailureReason = failureReason;
        }

        public bool Succeeded { get; }
        public InteractionCommand Command { get; }
        public InteractionFailureReason FailureReason { get; }

        public static InteractionExecution Success(InteractionCommand command)
        {
            return new InteractionExecution(true, command, InteractionFailureReason.None);
        }

        public static InteractionExecution Failure(
            InteractionCommand command,
            InteractionFailureReason reason)
        {
            return new InteractionExecution(false, command, reason);
        }
    }

    public interface IInteractable
    {
        StableId StableId { get; }
        string InteractionName { get; }
        Transform InteractionTransform { get; }
        InteractionKind GetDefaultInteraction(in InteractionContext context);
        bool CanInteract(
            in InteractionContext context,
            InteractionKind kind,
            out InteractionFailureReason failureReason);
        bool TryInteract(
            in InteractionContext context,
            InteractionKind kind,
            out InteractionFailureReason failureReason);
    }
}

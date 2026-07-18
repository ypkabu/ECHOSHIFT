using System;
using EchoShift.Player;

namespace EchoShift.Interaction.Recorded
{
    public readonly struct InteractionRequest
    {
        public InteractionRequest(
            ActorSimulationOrder actorOrder,
            InteractionCommand command,
            Interactor interactor)
        {
            ActorOrder = actorOrder;
            Command = command;
            Interactor = interactor;
        }

        public int Tick => Command.Tick;
        public ActorSimulationOrder ActorOrder { get; }
        public InteractionCommand Command { get; }
        public Interactor Interactor { get; }
    }

    public readonly struct InteractionResolution
    {
        public InteractionResolution(
            InteractionRequest request,
            InteractionExecution execution)
        {
            Request = request;
            Execution = execution;
        }

        public InteractionRequest Request { get; }
        public InteractionExecution Execution { get; }
    }

    public sealed class InteractionConflictResolver
    {
        public const int MaximumRequestsPerTick = 4;

        private readonly int[] _sortedIndices = new int[MaximumRequestsPerTick];
        private readonly ActorSimulationOrder[] _resolvedActors =
            new ActorSimulationOrder[MaximumRequestsPerTick];
        private readonly string[] _reservedTargets =
            new string[MaximumRequestsPerTick];

        public int Resolve(
            InteractionRequest[] requests,
            int requestCount,
            InteractionResolution[] results)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (requestCount < 0 ||
                requestCount > MaximumRequestsPerTick ||
                requestCount > requests.Length ||
                requestCount > results.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(requestCount));
            }

            BuildSortedIndices(requests, requestCount);
            int resolvedActorCount = 0;
            int reservedTargetCount = 0;
            for (int resultIndex = 0; resultIndex < requestCount; resultIndex++)
            {
                InteractionRequest request = requests[_sortedIndices[resultIndex]];
                bool duplicateActor = ContainsActor(
                    request.ActorOrder,
                    resolvedActorCount);
                bool targetBusy = ContainsTarget(
                    request.Command.TargetStableId,
                    reservedTargetCount);

                InteractionExecution execution;
                if (duplicateActor || targetBusy)
                {
                    execution = InteractionExecution.Failure(
                        request.Command,
                        InteractionFailureReason.TargetBusy);
                }
                else
                {
                    _resolvedActors[resolvedActorCount] = request.ActorOrder;
                    resolvedActorCount++;
                    _reservedTargets[reservedTargetCount] =
                        request.Command.TargetStableId;
                    reservedTargetCount++;
                    execution = request.Interactor != null
                        ? request.Interactor.ExecuteRecorded(request.Command)
                        : InteractionExecution.Failure(
                            request.Command,
                            InteractionFailureReason.MissingInteractor);
                }

                results[resultIndex] = new InteractionResolution(request, execution);
            }

            for (int i = 0; i < reservedTargetCount; i++)
            {
                _reservedTargets[i] = null;
            }

            return requestCount;
        }

        public static int Compare(
            in InteractionRequest left,
            in InteractionRequest right)
        {
            int tickComparison = left.Tick.CompareTo(right.Tick);
            if (tickComparison != 0)
            {
                return tickComparison;
            }

            int actorComparison = left.ActorOrder.CompareTo(right.ActorOrder);
            if (actorComparison != 0)
            {
                return actorComparison;
            }

            int targetComparison = string.CompareOrdinal(
                left.Command.TargetStableId,
                right.Command.TargetStableId);
            if (targetComparison != 0)
            {
                return targetComparison;
            }

            return left.Command.Kind.CompareTo(right.Command.Kind);
        }

        private void BuildSortedIndices(
            InteractionRequest[] requests,
            int requestCount)
        {
            for (int i = 0; i < requestCount; i++)
            {
                _sortedIndices[i] = i;
                int cursor = i;
                while (cursor > 0 &&
                       Compare(
                           requests[_sortedIndices[cursor]],
                           requests[_sortedIndices[cursor - 1]]) < 0)
                {
                    int previous = _sortedIndices[cursor - 1];
                    _sortedIndices[cursor - 1] = _sortedIndices[cursor];
                    _sortedIndices[cursor] = previous;
                    cursor--;
                }
            }
        }

        private bool ContainsActor(
            ActorSimulationOrder actorOrder,
            int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_resolvedActors[i] == actorOrder)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsTarget(string stableId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (string.Equals(
                        _reservedTargets[i],
                        stableId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

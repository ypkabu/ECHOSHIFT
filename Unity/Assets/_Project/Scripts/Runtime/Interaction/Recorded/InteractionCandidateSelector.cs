using System;

namespace EchoShift.Interaction.Recorded
{
    public readonly struct InteractionCandidateScore
    {
        public InteractionCandidateScore(
            bool canInteract,
            float distanceSquared,
            float alignment,
            string stableId)
        {
            CanInteract = canInteract;
            DistanceSquared = distanceSquared;
            Alignment = alignment;
            StableId = stableId ?? string.Empty;
        }

        public bool CanInteract { get; }
        public float DistanceSquared { get; }
        public float Alignment { get; }
        public string StableId { get; }
    }

    public static class InteractionCandidateSelector
    {
        private const float ComparisonEpsilon = 0.000001f;

        public static bool IsBetter(
            in InteractionCandidateScore candidate,
            in InteractionCandidateScore current)
        {
            if (candidate.CanInteract != current.CanInteract)
            {
                return candidate.CanInteract;
            }

            float distanceDifference = candidate.DistanceSquared - current.DistanceSquared;
            if (Math.Abs(distanceDifference) > ComparisonEpsilon)
            {
                return distanceDifference < 0f;
            }

            float alignmentDifference = candidate.Alignment - current.Alignment;
            if (Math.Abs(alignmentDifference) > ComparisonEpsilon)
            {
                return alignmentDifference > 0f;
            }

            return string.CompareOrdinal(candidate.StableId, current.StableId) < 0;
        }
    }
}

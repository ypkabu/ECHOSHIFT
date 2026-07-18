using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    public sealed class InteractionSensor : MonoBehaviour
    {
        private const int MaximumColliderHits = 16;

        [SerializeField, Min(0.1f)] private float interactionRange = 1.5f;
        [SerializeField] private LayerMask targetLayers;

        private readonly Collider[] _colliderHits = new Collider[MaximumColliderHits];
        private StableId _currentIdentity;
        private InteractionKind _currentKind;
        private bool _currentCanInteract;
        private InteractionFailureReason _currentFailureReason;

        public float InteractionRange => interactionRange;
        public IInteractable CurrentTarget => _currentIdentity != null
            ? _currentIdentity.Target
            : null;
        public string CurrentTargetName => CurrentTarget?.InteractionName ?? string.Empty;
        public string CurrentStableId => _currentIdentity != null
            ? _currentIdentity.Value
            : string.Empty;
        public InteractionKind CurrentKind => _currentKind;
        public bool CurrentCanInteract => _currentCanInteract;
        public InteractionFailureReason CurrentFailureReason => _currentFailureReason;

        public void Configure(float range, LayerMask layers)
        {
            interactionRange = range;
            targetLayers = layers;
        }

        public void RefreshCandidate(in InteractionContext context)
        {
            _currentIdentity = null;
            _currentKind = InteractionKind.None;
            _currentCanInteract = false;
            _currentFailureReason = InteractionFailureReason.NoCandidate;

            int hitCount = Physics.OverlapSphereNonAlloc(
                context.ActorPosition,
                interactionRange,
                _colliderHits,
                targetLayers,
                QueryTriggerInteraction.Collide);

            bool hasBest = false;
            InteractionCandidateScore bestScore = default;
            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = _colliderHits[i];
                if (hit == null)
                {
                    continue;
                }

                StableId identity = hit.GetComponentInParent<StableId>();
                if (identity == null || identity.Target == null)
                {
                    continue;
                }

                IInteractable target = identity.Target;
                InteractionKind kind = target.GetDefaultInteraction(context);
                bool canInteract;
                InteractionFailureReason reason;
                if (kind == InteractionKind.None)
                {
                    canInteract = false;
                    reason = InteractionFailureReason.UnsupportedInteraction;
                }
                else
                {
                    canInteract = target.CanInteract(context, kind, out reason);
                }

                Vector3 offset = target.InteractionTransform.position - context.ActorPosition;
                float distanceSquared = offset.sqrMagnitude;
                float alignment = distanceSquared > 0.000001f
                    ? Vector3.Dot(context.ActorForward, offset.normalized)
                    : 1f;
                InteractionCandidateScore score = new InteractionCandidateScore(
                    canInteract,
                    distanceSquared,
                    alignment,
                    identity.Value);

                if (hasBest && !InteractionCandidateSelector.IsBetter(score, bestScore))
                {
                    continue;
                }

                hasBest = true;
                bestScore = score;
                _currentIdentity = identity;
                _currentKind = kind;
                _currentCanInteract = canInteract;
                _currentFailureReason = reason;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.45f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            if (_currentIdentity == null || _currentIdentity.Target == null)
            {
                return;
            }

            Gizmos.color = _currentCanInteract ? Color.green : Color.red;
            Gizmos.DrawLine(
                transform.position,
                _currentIdentity.Target.InteractionTransform.position);
            Gizmos.DrawWireSphere(
                _currentIdentity.Target.InteractionTransform.position,
                0.18f);
        }
    }
}

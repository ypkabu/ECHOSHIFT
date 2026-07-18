using UnityEngine;

namespace EchoShift.Interaction.Recorded
{
    public sealed class StableId : MonoBehaviour
    {
        [SerializeField] private string value;
        [SerializeField] private InteractionRegistry registry;
        [SerializeField] private MonoBehaviour targetComponent;

        private IInteractable _target;

        public string Value => value;
        public IInteractable Target => _target;
        public bool HasValidConfiguration =>
            !string.IsNullOrWhiteSpace(value) &&
            registry != null &&
            _target != null;

        public void Configure(
            string stableValue,
            InteractionRegistry interactionRegistry,
            MonoBehaviour interactableComponent)
        {
            registry?.Unregister(this);
            value = stableValue;
            registry = interactionRegistry;
            targetComponent = interactableComponent;
            ResolveTarget();
            RegisterIfPossible();
        }

        private void Awake()
        {
            ResolveTarget();
        }

        private void OnEnable()
        {
            ResolveTarget();
            RegisterIfPossible();
        }

        private void OnDisable()
        {
            registry?.Unregister(this);
        }

        private void ResolveTarget()
        {
            _target = targetComponent as IInteractable;
        }

        private void RegisterIfPossible()
        {
            if (!isActiveAndEnabled || registry == null || _target == null)
            {
                return;
            }

            if (!registry.Register(this) && !string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError($"Stable ID registration failed for '{value}'.", this);
            }
        }
    }
}

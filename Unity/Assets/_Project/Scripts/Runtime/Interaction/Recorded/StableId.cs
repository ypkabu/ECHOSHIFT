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
        internal bool CanRegister =>
            targetComponent != null &&
            _target != null;
        public bool IsTargetAvailable =>
            CanRegister &&
            targetComponent.isActiveAndEnabled &&
            isActiveAndEnabled;
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
            RegisterIfPossible(false);
        }

        private void Awake()
        {
            ResolveTarget();
        }

        private void OnEnable()
        {
            ResolveTarget();
            RegisterIfPossible(true);
        }

        private void OnDisable()
        {
            registry?.Unregister(this);
        }

        private void ResolveTarget()
        {
            _target = targetComponent as IInteractable;
        }

        private void RegisterIfPossible(bool isEnabling)
        {
            if (registry == null || !CanRegister)
            {
                return;
            }

            if (!isEnabling && !IsTargetAvailable)
            {
                return;
            }

            if (!registry.Register(this) && !string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError($"Stable ID registration failed for '{value}'.", this);
            }
        }

        internal void RegisterTarget()
        {
            RegisterIfPossible(true);
        }

        internal void UnregisterTarget()
        {
            registry?.Unregister(this);
        }
    }
}

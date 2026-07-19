using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class InteractionHighlight : MonoBehaviour
    {
        [SerializeField] private PlayerSimulation player;
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.15f, 1f);
        private MaterialPropertyBlock _properties;
        private Renderer _current;

        public void Configure(PlayerSimulation playerSimulation) => player = playerSimulation;

        private void Awake() => _properties = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            Renderer next = null;
            IInteractable target = player?.Interactor?.Sensor?.CurrentTarget;
            if (target != null) next = target.InteractionTransform.GetComponentInChildren<Renderer>();
            if (next == _current) return;
            Apply(_current, Color.black, 0f);
            _current = next;
            Apply(_current, highlightColor, 1f);
        }

        private void Apply(Renderer target, Color color, float emission)
        {
            if (target == null) return;
            target.GetPropertyBlock(_properties);
            _properties.SetColor("_EmissionColor", color * emission);
            target.SetPropertyBlock(_properties);
        }
    }
}

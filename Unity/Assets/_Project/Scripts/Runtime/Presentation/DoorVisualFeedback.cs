using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class DoorVisualFeedback : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Phase4VisualSettings settings;
        [SerializeField] private Color closedColor = new Color(1f, 0.2f, 0.12f, 1f);
        [SerializeField] private Color openColor = new Color(0.15f, 1f, 0.55f, 1f);
        private MaterialPropertyBlock _properties;
        private bool _lastOpen;

        public void Configure(DoorController controller, Renderer visualRenderer)
        {
            door = controller;
            targetRenderer = visualRenderer;
        }

        public void Configure(
            DoorController controller,
            Renderer visualRenderer,
            Renderer badgeRenderer,
            Phase4VisualSettings visualSettings)
        {
            Configure(controller, visualRenderer);
            statusRenderer = badgeRenderer;
            settings = visualSettings;
        }

        public bool HasPhase4References => door != null && targetRenderer != null &&
            statusRenderer != null && settings != null;
        public bool IsOpenVisual => _lastOpen;

        private void Awake() => _properties = new MaterialPropertyBlock();

        private void LateUpdate() => RefreshNowForTests();

        public void RefreshNowForTests()
        {
            _properties ??= new MaterialPropertyBlock();
            bool open = door != null && door.IsOpen;
            if (open == _lastOpen && Time.frameCount > 1) return;
            _lastOpen = open;
            Color color = settings != null
                ? open ? settings.GoalColor : settings.DangerColor
                : open ? openColor : closedColor;
            if (targetRenderer == null) return;
            targetRenderer.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", color);
            _properties.SetColor("_EmissionColor", color * (open ? 1.2f : 0.2f));
            targetRenderer.SetPropertyBlock(_properties);
            if (statusRenderer != null)
            {
                statusRenderer.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", color);
                _properties.SetColor("_EmissionColor", color * (open ? 2f : 0.55f));
                statusRenderer.SetPropertyBlock(_properties);
            }
        }
    }
}

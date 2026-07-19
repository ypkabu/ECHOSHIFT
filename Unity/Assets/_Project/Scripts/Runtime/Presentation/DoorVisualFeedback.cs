using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class DoorVisualFeedback : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Color closedColor = new Color(1f, 0.2f, 0.12f, 1f);
        [SerializeField] private Color openColor = new Color(0.15f, 1f, 0.55f, 1f);
        private MaterialPropertyBlock _properties;
        private bool _lastOpen;

        public void Configure(DoorController controller, Renderer visualRenderer)
        {
            door = controller;
            targetRenderer = visualRenderer;
        }

        private void Awake() => _properties = new MaterialPropertyBlock();

        private void LateUpdate()
        {
            bool open = door != null && door.IsOpen;
            if (open == _lastOpen && Time.frameCount > 1) return;
            _lastOpen = open;
            Color color = open ? openColor : closedColor;
            targetRenderer.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", color);
            _properties.SetColor("_EmissionColor", color * (open ? 1.2f : 0.2f));
            targetRenderer.SetPropertyBlock(_properties);
        }
    }
}

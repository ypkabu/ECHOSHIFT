using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4SocketVisual : MonoBehaviour
    {
        [SerializeField] private PowerSocket socket;
        [SerializeField] private Renderer ring;
        [SerializeField] private Phase4VisualSettings settings;
        private MaterialPropertyBlock _properties;
        public bool IsPoweredVisual { get; private set; }
        public bool HasRequiredReferences => socket != null && ring != null && settings != null;

        public void Configure(PowerSocket source, Renderer statusRing,
            Phase4VisualSettings visualSettings)
        {
            socket = source;
            ring = statusRing;
            settings = visualSettings;
        }

        private void Awake() => _properties = new MaterialPropertyBlock();
        private void OnEnable()
        {
            if (socket != null) socket.PoweredChanged += OnPoweredChanged;
            RefreshNowForTests();
        }
        private void OnDisable()
        {
            if (socket != null) socket.PoweredChanged -= OnPoweredChanged;
        }

        public void RefreshNowForTests() => OnPoweredChanged(socket != null && socket.IsPowered);

        private void OnPoweredChanged(bool powered)
        {
            IsPoweredVisual = powered;
            if (ring == null || settings == null) return;
            Color color = powered ? settings.GoalColor : settings.BatteryColor;
            _properties ??= new MaterialPropertyBlock();
            ring.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", color);
            _properties.SetColor("_EmissionColor", color * (powered ? 2.4f : 0.65f));
            ring.SetPropertyBlock(_properties);
        }
    }
}

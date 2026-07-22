using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4BatteryVisual : MonoBehaviour
    {
        [SerializeField] private CarryableBattery battery;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer core;
        [SerializeField] private Phase4VisualSettings settings;
        private MaterialPropertyBlock _properties;
        private Vector3 _baseScale;
        private bool _lastHeld;
        private bool _lastInserted;

        public bool HasRequiredReferences => battery != null && visualRoot != null &&
            core != null && settings != null;
        public bool IsHeldVisual => _lastHeld;
        public bool IsInsertedVisual => _lastInserted;

        public void Configure(CarryableBattery source, Transform root,
            Renderer coreRenderer, Phase4VisualSettings visualSettings)
        {
            battery = source;
            visualRoot = root;
            core = coreRenderer;
            settings = visualSettings;
        }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            if (visualRoot != null) _baseScale = visualRoot.localScale;
        }

        private void LateUpdate() => RefreshNowForTests();

        public void RefreshNowForTests()
        {
            if (!HasRequiredReferences) return;
            if (_properties == null)
            {
                _properties = new MaterialPropertyBlock();
                _baseScale = visualRoot.localScale;
            }
            _lastHeld = battery.IsHeld;
            _lastInserted = battery.IsInserted;
            float pulse = _lastInserted ? 1f : 0.5f + Mathf.PingPong(Time.unscaledTime * 0.4f, 0.5f);
            visualRoot.localScale = _baseScale * (_lastHeld ? 1.08f : 1f);
            core.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", settings.BatteryColor);
            _properties.SetColor("_EmissionColor", settings.BatteryColor * (1.1f + pulse));
            core.SetPropertyBlock(_properties);
        }
    }
}

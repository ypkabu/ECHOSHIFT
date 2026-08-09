using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    [DisallowMultipleComponent]
    public sealed class Phase4FloorCircuitVisual : MonoBehaviour
    {
        public const float DefaultHeight = 0.012f;
        public const float DefaultWidth = 0.06f;

        [SerializeField] private MonoBehaviour sourceComponent;
        [SerializeField] private Renderer[] segments = System.Array.Empty<Renderer>();
        [SerializeField] private Color circuitColor = Color.cyan;
        [SerializeField] private float floorHeight = DefaultHeight;
        [SerializeField] private float circuitWidth = DefaultWidth;
        private IDoorOpenSource _source;
        private MaterialPropertyBlock _properties;
        private bool _lastPowered;

        public bool HasRequiredReferences => sourceComponent is IDoorOpenSource &&
            segments != null && segments.Length >= 3;
        public bool IsPoweredVisual => _lastPowered;
        public float FloorHeight => floorHeight;
        public float CircuitWidth => circuitWidth;
        public int SegmentCount => segments?.Length ?? 0;
        public MonoBehaviour SourceComponent => sourceComponent;

        public void Configure(
            MonoBehaviour source, Renderer[] circuitSegments, Color color,
            float height = DefaultHeight, float width = DefaultWidth)
        {
            sourceComponent = source;
            segments = circuitSegments ?? System.Array.Empty<Renderer>();
            circuitColor = color;
            floorHeight = height;
            circuitWidth = width;
            _source = source as IDoorOpenSource;
        }

        private void Awake()
        {
            _source = sourceComponent as IDoorOpenSource;
            _properties = new MaterialPropertyBlock();
        }

        private void OnEnable() => RefreshNowForTests();

        private void LateUpdate()
        {
            bool powered = _source != null && _source.RequestsDoorOpen;
            if (powered != _lastPowered) Apply(powered);
        }

        public void RefreshNowForTests()
        {
            _source = sourceComponent as IDoorOpenSource;
            Apply(_source != null && _source.RequestsDoorOpen);
        }

        private void Apply(bool powered)
        {
            _lastPowered = powered;
            _properties ??= new MaterialPropertyBlock();
            float emission = powered ? 1.8f : 0.22f;
            for (int i = 0; i < segments.Length; i++)
            {
                Renderer segment = segments[i];
                if (segment == null) continue;
                segment.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", circuitColor * (powered ? 1f : 0.42f));
                _properties.SetColor("_EmissionColor", circuitColor * emission);
                segment.SetPropertyBlock(_properties);
            }
        }
    }
}

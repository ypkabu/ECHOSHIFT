using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4PressurePlateVisual : MonoBehaviour
    {
        [SerializeField] private PressurePlate plate;
        [SerializeField] private Transform pad;
        [SerializeField] private Renderer[] accents = System.Array.Empty<Renderer>();
        [SerializeField] private Phase4VisualSettings settings;
        [SerializeField, Min(0f)] private float travel = 0.09f;
        private MaterialPropertyBlock _properties;
        private Vector3 _releasedPosition;
        private bool _pressed;

        public bool HasRequiredReferences => plate != null && pad != null && settings != null &&
            accents != null && accents.Length >= 1;
        public bool IsPressedVisual => _pressed;

        public void Configure(PressurePlate source, Transform movingPad,
            Renderer[] accentRenderers, Phase4VisualSettings visualSettings)
        {
            plate = source;
            pad = movingPad;
            accents = accentRenderers ?? System.Array.Empty<Renderer>();
            settings = visualSettings;
        }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            if (pad != null) _releasedPosition = pad.localPosition;
        }

        private void OnEnable()
        {
            if (plate != null) plate.PressedChanged += OnPressedChanged;
            RefreshNowForTests();
        }

        private void OnDisable()
        {
            if (plate != null) plate.PressedChanged -= OnPressedChanged;
        }

        private void LateUpdate()
        {
            if (pad == null) return;
            Vector3 target = _releasedPosition + Vector3.down * (_pressed ? travel : 0f);
            pad.localPosition = Vector3.Lerp(pad.localPosition, target,
                Mathf.Clamp01(Time.unscaledDeltaTime * 14f));
        }

        public void RefreshNowForTests() => OnPressedChanged(plate != null && plate.IsPressed);

        private void OnPressedChanged(bool pressed)
        {
            _pressed = pressed;
            if (settings == null) return;
            Color color = pressed ? settings.GoalColor : settings.PlateColor;
            _properties ??= new MaterialPropertyBlock();
            for (int i = 0; i < accents.Length; i++)
            {
                Renderer target = accents[i];
                if (target == null) continue;
                target.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", color);
                _properties.SetColor("_EmissionColor", color * (pressed ? 2.2f : 0.8f));
                target.SetPropertyBlock(_properties);
            }
        }
    }
}

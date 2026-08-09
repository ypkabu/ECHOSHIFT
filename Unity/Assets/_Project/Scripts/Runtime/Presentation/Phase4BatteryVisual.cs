using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4BatteryVisual : MonoBehaviour
    {
        public const float VisualLengthMeters = 0.78f;
        public const float VisualDiameterMeters = 0.44f;
        public const float InsertionVisualDuration = 0.25f;

        [SerializeField] private CarryableBattery battery;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer core;
        [SerializeField] private Phase4VisualSettings settings;
        private MaterialPropertyBlock _properties;
        private Vector3 _baseScale;
        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private Vector3 _insertionStartLocalPosition;
        private Quaternion _insertionStartLocalRotation;
        private Vector3 _lastWorldPosition;
        private Quaternion _lastWorldRotation;
        private float _insertionProgress = 1f;
        private bool _hasWorldSample;
        private bool _lastHeld;
        private bool _lastInserted;

        public bool HasRequiredReferences => battery != null && visualRoot != null &&
            core != null && settings != null;
        public bool IsHeldVisual => _lastHeld;
        public bool IsInsertedVisual => _lastInserted;
        public float InsertionProgress => _insertionProgress;
        public Transform VisualRoot => visualRoot;

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
            CaptureBaseTransform();
        }

        private void LateUpdate() => UpdateVisual(Time.unscaledDeltaTime, false);

        public void RefreshNowForTests()
        {
            // Editor capture advances the fixed-tick simulation synchronously and
            // therefore has no LateUpdate between ticks. Mirror the normal held
            // Battery follow once before rendering the resulting legitimate state.
            if (battery != null && battery.IsHeld && battery.Holder.CarrySocket != null)
                battery.transform.SetPositionAndRotation(
                    battery.Holder.CarrySocket.position,
                    battery.Holder.CarrySocket.rotation);
            UpdateVisual(InsertionVisualDuration, true);
        }

        public void AdvanceForTests(float deltaTime) => UpdateVisual(deltaTime, false);

        private void UpdateVisual(float deltaTime, bool completeTransition)
        {
            if (!HasRequiredReferences) return;
            if (_properties == null)
            {
                _properties = new MaterialPropertyBlock();
                CaptureBaseTransform();
            }
            bool held = battery.IsHeld;
            bool inserted = battery.IsInserted;
            if (inserted && !_lastInserted)
            {
                _insertionProgress = 0f;
                if (_hasWorldSample)
                {
                    Transform parent = visualRoot.parent;
                    _insertionStartLocalPosition = parent.InverseTransformPoint(_lastWorldPosition);
                    _insertionStartLocalRotation = Quaternion.Inverse(parent.rotation) * _lastWorldRotation;
                }
                else
                {
                    _insertionStartLocalPosition = _baseLocalPosition;
                    _insertionStartLocalRotation = _baseLocalRotation;
                }
            }
            if (!inserted) _insertionProgress = 1f;
            else
            {
                float step = completeTransition ? 1f : deltaTime / InsertionVisualDuration;
                _insertionProgress = Mathf.Clamp01(_insertionProgress + step);
                float eased = _insertionProgress * _insertionProgress *
                    (3f - 2f * _insertionProgress);
                visualRoot.localPosition = Vector3.Lerp(
                    _insertionStartLocalPosition, _baseLocalPosition, eased);
                visualRoot.localRotation = Quaternion.Slerp(
                    _insertionStartLocalRotation, _baseLocalRotation, eased);
            }
            if (!inserted)
            {
                visualRoot.localPosition = _baseLocalPosition;
                visualRoot.localRotation = _baseLocalRotation;
            }
            _lastHeld = held;
            _lastInserted = inserted;
            float pulse = _lastInserted ? 1f : 0.5f + Mathf.PingPong(Time.unscaledTime * 0.4f, 0.5f);
            visualRoot.localScale = _baseScale;
            core.GetPropertyBlock(_properties);
            _properties.SetColor("_BaseColor", settings.BatteryColor);
            _properties.SetColor("_EmissionColor", settings.BatteryColor * (1.1f + pulse));
            core.SetPropertyBlock(_properties);
            _lastWorldPosition = visualRoot.position;
            _lastWorldRotation = visualRoot.rotation;
            _hasWorldSample = true;
        }

        private void CaptureBaseTransform()
        {
            if (visualRoot == null) return;
            _baseScale = visualRoot.localScale;
            _baseLocalPosition = visualRoot.localPosition;
            _baseLocalRotation = visualRoot.localRotation;
            _lastWorldPosition = visualRoot.position;
            _lastWorldRotation = visualRoot.rotation;
            _hasWorldSample = true;
        }
    }
}

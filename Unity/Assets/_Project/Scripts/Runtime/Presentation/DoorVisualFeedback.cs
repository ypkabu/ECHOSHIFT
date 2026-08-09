using EchoShift.Interaction;
using Unity.Profiling;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class DoorVisualFeedback : MonoBehaviour
    {
        public const string ProfilerMarkerName = "EchoShift.DoorVisual.Update";
        public const float PreparationDuration = 0.10f;
        public const float SlideDuration = 0.35f;
        public const float PanelTravel = 1.42f;

        private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker(ProfilerMarkerName);

        [SerializeField] private DoorController door;
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Phase4VisualSettings settings;
        [SerializeField] private Transform leftPanel;
        [SerializeField] private Transform rightPanel;
        [SerializeField] private Renderer[] panelRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Renderer[] frameRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private bool plateDoor;
        [SerializeField] private Color closedColor = new Color(1f, 0.2f, 0.12f, 1f);
        [SerializeField] private Color openColor = new Color(0.15f, 1f, 0.55f, 1f);

        private MaterialPropertyBlock _properties;
        private Vector3 _leftClosedPosition;
        private Vector3 _rightClosedPosition;
        private Vector3 _leftClosedScale;
        private Vector3 _rightClosedScale;
        private float _openProgress;
        private float _transitionElapsed;
        private bool _targetOpen;
        private bool _capturedTransforms;

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

        public void Configure(
            DoorController controller, Transform left, Transform right,
            Renderer[] movingRenderers, Renderer seamRenderer,
            Renderer[] doorwayRenderers, Phase4VisualSettings visualSettings,
            bool isPlateDoor)
        {
            door = controller;
            leftPanel = left;
            rightPanel = right;
            panelRenderers = movingRenderers ?? System.Array.Empty<Renderer>();
            targetRenderer = panelRenderers.Length > 0 ? panelRenderers[0] : null;
            statusRenderer = seamRenderer;
            frameRenderers = doorwayRenderers ?? System.Array.Empty<Renderer>();
            settings = visualSettings;
            plateDoor = isPlateDoor;
            _capturedTransforms = false;
            CapturePanelTransforms();
        }

        public bool HasPhase4References => door != null && targetRenderer != null &&
            statusRenderer != null && settings != null && leftPanel != null && rightPanel != null &&
            panelRenderers != null && panelRenderers.Length >= 2 &&
            frameRenderers != null && frameRenderers.Length >= 2;
        public bool IsOpenVisual => _openProgress >= 0.99f;
        public float OpenProgress => _openProgress;
        public bool UsesSplitPanels => leftPanel != null && rightPanel != null && leftPanel != rightPanel;
        public bool VisualsAreSeparatedFromGameplayRoot => UsesSplitPanels &&
            !leftPanel.IsChildOf(transform) && !rightPanel.IsChildOf(transform);
        public bool IsPlateDoorVisual => plateDoor;
        public Transform LeftPanel => leftPanel;
        public Transform RightPanel => rightPanel;

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            CapturePanelTransforms();
        }

        private void OnEnable()
        {
            CapturePanelTransforms();
            _targetOpen = door != null && door.IsOpen;
            _openProgress = _targetOpen ? 1f : 0f;
            _transitionElapsed = PreparationDuration + SlideDuration;
            ApplyVisuals();
        }

        private void LateUpdate()
        {
            using (UpdateMarker.Auto())
            {
                Advance(Time.unscaledDeltaTime, false);
            }
        }

        public void RefreshNowForTests() => Advance(PreparationDuration + SlideDuration, true);

        public void AdvanceForTests(float deltaTime) => Advance(deltaTime, false);

        private void Advance(float deltaTime, bool complete)
        {
            _properties ??= new MaterialPropertyBlock();
            CapturePanelTransforms();
            bool open = door != null && door.IsOpen;
            if (open != _targetOpen)
            {
                _targetOpen = open;
                _transitionElapsed = 0f;
            }
            _transitionElapsed += deltaTime;
            if (complete) _transitionElapsed = PreparationDuration + SlideDuration;
            if (_transitionElapsed >= PreparationDuration)
            {
                float step = complete ? 1f : deltaTime / SlideDuration;
                _openProgress = Mathf.MoveTowards(_openProgress, _targetOpen ? 1f : 0f, step);
            }
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            float eased = _openProgress * _openProgress * (3f - 2f * _openProgress);
            if (leftPanel != null)
            {
                leftPanel.localPosition = _leftClosedPosition + Vector3.left * (PanelTravel * eased);
                Vector3 scale = _leftClosedScale;
                scale.x *= Mathf.Lerp(1f, 0.16f, eased);
                leftPanel.localScale = scale;
            }
            if (rightPanel != null)
            {
                rightPanel.localPosition = _rightClosedPosition + Vector3.right * (PanelTravel * eased);
                Vector3 scale = _rightClosedScale;
                scale.x *= Mathf.Lerp(1f, 0.16f, eased);
                rightPanel.localScale = scale;
            }

            Color stateColor = settings != null
                ? _targetOpen ? settings.GoalColor : settings.DangerColor
                : _targetOpen ? openColor : closedColor;
            ApplyColor(panelRenderers, stateColor, Mathf.Lerp(0.16f, 0.72f, eased));
            for (int i = 0; i < panelRenderers.Length; i++)
                if (panelRenderers[i] != null)
                    panelRenderers[i].forceRenderingOff = _openProgress >= 0.98f;
            bool panelsVisible = _openProgress < 0.98f;
            if (leftPanel != null && leftPanel.gameObject.activeSelf != panelsVisible)
                leftPanel.gameObject.SetActive(panelsVisible);
            if (rightPanel != null && rightPanel.gameObject.activeSelf != panelsVisible)
                rightPanel.gameObject.SetActive(panelsVisible);
            if (statusRenderer != null)
            {
                statusRenderer.GetPropertyBlock(_properties);
                Color circuit = settings != null
                    ? plateDoor ? settings.PlateColor : settings.BatteryColor
                    : stateColor;
                statusRenderer.enabled = _openProgress < 0.98f;
                statusRenderer.SetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", circuit);
                _properties.SetColor("_EmissionColor", circuit *
                    (_transitionElapsed < PreparationDuration ? 2.8f : 1.25f));
                statusRenderer.SetPropertyBlock(_properties);
            }
            Color frameColor = settings != null
                ? plateDoor ? settings.PlateColor : settings.BatteryColor
                : stateColor;
            ApplyColor(frameRenderers, frameColor, Mathf.Lerp(0.35f, 2.1f, eased));
        }

        private void ApplyColor(Renderer[] renderers, Color color, float emission)
        {
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null) continue;
                renderer.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", color);
                _properties.SetColor("_EmissionColor", color * emission);
                renderer.SetPropertyBlock(_properties);
            }
        }

        private void CapturePanelTransforms()
        {
            if (_capturedTransforms || leftPanel == null || rightPanel == null) return;
            _leftClosedPosition = leftPanel.localPosition;
            _rightClosedPosition = rightPanel.localPosition;
            _leftClosedScale = leftPanel.localScale;
            _rightClosedScale = rightPanel.localScale;
            _capturedTransforms = true;
        }
    }
}

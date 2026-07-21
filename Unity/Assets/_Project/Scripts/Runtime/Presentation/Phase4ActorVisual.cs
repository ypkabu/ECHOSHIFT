using EchoShift.Player;
using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4ActorVisual : MonoBehaviour
    {
        [SerializeField] private Phase4VisualSettings settings;
        [SerializeField] private LoopActor actor;
        [SerializeField] private EchoPlayback playback;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Renderer[] bodyRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Renderer[] accentRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private GameObject[] generationMarks = System.Array.Empty<GameObject>();

        private MaterialPropertyBlock _properties;
        private Vector3 _baseScale;
        private Quaternion _armRestRotation;
        private int _appliedGeneration = -1;
        private int _lastInteractionSuccess;
        private int _lastInteractionFailure;
        private float _spawnProgress;
        private float _interactionPulse;
        private bool _lastStopped;

        public bool HasRequiredReferences =>
            settings != null && actor != null && modelRoot != null && rightArm != null &&
            bodyRenderers != null && bodyRenderers.Length >= 1 &&
            accentRenderers != null && accentRenderers.Length >= 2 &&
            generationMarks != null && generationMarks.Length == 3;
        public int VisibleGenerationMarkCount { get; private set; }
        public bool IsStoppedVisual { get; private set; }
        public bool UsesPropertyBlocks => true;

        public void Configure(
            Phase4VisualSettings visualSettings,
            LoopActor loopActor,
            EchoPlayback echoPlayback,
            Transform visualRoot,
            Transform carryArm,
            Renderer[] bodies,
            Renderer[] accents,
            GameObject[] marks)
        {
            settings = visualSettings;
            actor = loopActor;
            playback = echoPlayback;
            modelRoot = visualRoot;
            rightArm = carryArm;
            bodyRenderers = bodies ?? System.Array.Empty<Renderer>();
            accentRenderers = accents ?? System.Array.Empty<Renderer>();
            generationMarks = marks ?? System.Array.Empty<GameObject>();
        }

        private void Awake()
        {
            actor ??= GetComponent<LoopActor>();
            playback ??= GetComponent<EchoPlayback>();
            _properties = new MaterialPropertyBlock();
            if (modelRoot != null) _baseScale = modelRoot.localScale;
            if (rightArm != null) _armRestRotation = rightArm.localRotation;
        }

        private void OnEnable()
        {
            _spawnProgress = 0f;
            _appliedGeneration = -1;
            ApplyIdentity();
        }

        private void LateUpdate()
        {
            if (!HasRequiredReferences) return;

            int generation = actor.Kind == LoopActorKind.Player ? 0 : actor.ReplayGeneration;
            if (_appliedGeneration != generation) ApplyIdentity();

            bool stopped = playback != null && playback.PlaybackTick >= playback.RecordingLength;
            if (stopped != _lastStopped)
            {
                _lastStopped = stopped;
                ApplyState(stopped);
            }

            if (playback != null)
            {
                if (playback.InteractionSuccessCount != _lastInteractionSuccess)
                {
                    _lastInteractionSuccess = playback.InteractionSuccessCount;
                    PulseInteraction(true);
                }
                if (playback.InteractionFailureCount != _lastInteractionFailure)
                {
                    _lastInteractionFailure = playback.InteractionFailureCount;
                    PulseInteraction(false);
                }
            }

            _spawnProgress = Mathf.MoveTowards(_spawnProgress, 1f, Time.unscaledDeltaTime * 5f);
            _interactionPulse = Mathf.MoveTowards(
                _interactionPulse, 0f, Time.unscaledDeltaTime * 4f);
            float easedSpawn = 1f - ((1f - _spawnProgress) * (1f - _spawnProgress));
            float pulseScale = 1f + Mathf.Abs(_interactionPulse) * 0.08f;
            modelRoot.localScale = _baseScale * (easedSpawn * pulseScale);

            bool carrying = false;
            PlayerSimulation simulation = GetComponent<PlayerSimulation>();
            if (simulation != null) carrying = simulation.Interactor?.CarriedBattery != null;
            else if (playback != null) carrying = playback.Interactor?.CarriedBattery != null;
            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation,
                carrying ? Quaternion.Euler(-62f, 0f, 0f) : _armRestRotation,
                Mathf.Clamp01(Time.unscaledDeltaTime * 12f));
        }

        public void PulseInteraction(bool success)
        {
            _interactionPulse = success ? 1f : -1f;
            Color color = success ? Color.white : settings.DangerColor;
            ApplyRenderers(accentRenderers, color, success ? 2.4f : 3f);
        }

        public void RefreshNowForTests()
        {
            if (_properties == null) _properties = new MaterialPropertyBlock();
            ApplyIdentity();
            bool stopped = playback != null && playback.PlaybackTick >= playback.RecordingLength;
            ApplyState(stopped);
        }

        private void ApplyIdentity()
        {
            if (settings == null || actor == null) return;
            int generation = actor.Kind == LoopActorKind.Player ? 0 : actor.ReplayGeneration;
            _appliedGeneration = generation;
            Color bodyColor = actor.Kind == LoopActorKind.Player
                ? new Color(0.86f, 0.89f, 0.93f, 1f)
                : new Color(0.42f, 0.48f, 0.56f, 1f);
            Color accent = actor.Kind == LoopActorKind.Player
                ? settings.PlayerColor
                : settings.GetEchoColor(generation);
            ApplyRenderers(bodyRenderers, bodyColor, actor.Kind == LoopActorKind.Player ? 0.08f : 0.03f);
            ApplyRenderers(accentRenderers, accent, actor.Kind == LoopActorKind.Player ? 1.8f : 1.45f);

            VisibleGenerationMarkCount = 0;
            int visualGeneration = generation > 0
                ? ((generation - 1) % generationMarks.Length) + 1
                : 0;
            for (int i = 0; i < generationMarks.Length; i++)
            {
                bool visible = actor.Kind == LoopActorKind.Echo && i < visualGeneration;
                generationMarks[i].SetActive(visible);
                if (visible) VisibleGenerationMarkCount++;
            }
        }

        private void ApplyState(bool stopped)
        {
            IsStoppedVisual = stopped;
            if (actor == null || settings == null) return;
            Color accent = actor.Kind == LoopActorKind.Player
                ? settings.PlayerColor
                : settings.GetEchoColor(Mathf.Max(1, actor.ReplayGeneration));
            ApplyRenderers(accentRenderers, accent, stopped ? 0.35f : 1.45f);
        }

        private void ApplyRenderers(Renderer[] renderers, Color color, float emission)
        {
            if (_properties == null) _properties = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer target = renderers[i];
                if (target == null) continue;
                target.GetPropertyBlock(_properties);
                _properties.SetColor("_BaseColor", color);
                _properties.SetColor("_EmissionColor", color * emission);
                target.SetPropertyBlock(_properties);
            }
        }
    }
}

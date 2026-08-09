using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Presentation
{
    [RequireComponent(typeof(EchoPlayback))]
    public sealed class EchoVisualFeedback : MonoBehaviour
    {
        [SerializeField] private EchoPlayback playback;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private Renderer identityRing;
        [SerializeField] private TextMesh identityLabel;
        [SerializeField] private Phase4ActorVisual phase4Visual;
        private MaterialPropertyBlock _identityProperties;
        private Vector3 _baseScale;
        private int _lastSuccess;
        private int _lastFailure;
        private float _pulse;

        public bool IsStopped => playback != null && playback.PlaybackTick >= playback.RecordingLength;
        public string IdentityText => identityLabel != null ? identityLabel.text : string.Empty;
        public bool HasIdentityRing => identityRing != null;

        public void ConfigureIdentity(Renderer ring, TextMesh label)
        {
            identityRing = ring;
            identityLabel = label;
        }

        private void Awake()
        {
            playback ??= GetComponent<EchoPlayback>();
            trail ??= GetComponent<TrailRenderer>();
            phase4Visual ??= GetComponent<Phase4ActorVisual>();
            _identityProperties = new MaterialPropertyBlock();
            if (identityRing == null)
            {
                Transform ring = transform.Find("Echo Identity Ring");
                identityRing = ring != null ? ring.GetComponent<Renderer>() : null;
            }
            if (identityLabel == null)
            {
                Transform label = transform.Find("Echo Identity Label");
                identityLabel = label != null ? label.GetComponent<TextMesh>() : null;
            }
            _baseScale = transform.localScale;
        }

        private void Start()
        {
            if (trail != null)
            {
                trail.startColor = playback.GenerationColor;
                trail.endColor = new Color(playback.GenerationColor.r, playback.GenerationColor.g,
                    playback.GenerationColor.b, 0f);
            }
            ApplyIdentity();
        }

        private void LateUpdate()
        {
            if (playback.InteractionSuccessCount != _lastSuccess)
            {
                _lastSuccess = playback.InteractionSuccessCount;
                _pulse = 1f;
                phase4Visual?.PulseInteraction(true);
            }
            if (playback.InteractionFailureCount != _lastFailure)
            {
                _lastFailure = playback.InteractionFailureCount;
                _pulse = -1f;
                phase4Visual?.PulseInteraction(false);
            }
            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.unscaledDeltaTime * 4f);
            float scale = IsStopped ? 0.92f : 1f + Mathf.Abs(_pulse) * 0.18f;
            transform.localScale = _baseScale * scale;
            if (trail != null) trail.emitting = !IsStopped;
        }

        private void ApplyIdentity()
        {
            if (playback == null) return;
            Color color = playback.GenerationColor;
            if (identityLabel != null)
            {
                identityLabel.text = $"E{playback.ReplayGeneration}";
                identityLabel.color = new Color(color.r, color.g, color.b, 1f);
            }
            if (identityRing == null) return;
            _identityProperties ??= new MaterialPropertyBlock();
            identityRing.GetPropertyBlock(_identityProperties);
            _identityProperties.SetColor("_BaseColor", new Color(color.r, color.g, color.b, 1f));
            _identityProperties.SetColor("_EmissionColor", color * 1.6f);
            identityRing.SetPropertyBlock(_identityProperties);
        }
    }
}

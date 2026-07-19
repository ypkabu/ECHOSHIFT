using EchoShift.Replay;
using UnityEngine;

namespace EchoShift.Presentation
{
    [RequireComponent(typeof(EchoPlayback))]
    public sealed class EchoVisualFeedback : MonoBehaviour
    {
        [SerializeField] private EchoPlayback playback;
        [SerializeField] private TrailRenderer trail;
        private Vector3 _baseScale;
        private int _lastSuccess;
        private int _lastFailure;
        private float _pulse;

        public bool IsStopped => playback != null && playback.PlaybackTick >= playback.RecordingLength;

        private void Awake()
        {
            playback ??= GetComponent<EchoPlayback>();
            trail ??= GetComponent<TrailRenderer>();
            _baseScale = transform.localScale;
        }

        private void Start()
        {
            if (trail == null) return;
            trail.startColor = playback.GenerationColor;
            trail.endColor = new Color(playback.GenerationColor.r, playback.GenerationColor.g,
                playback.GenerationColor.b, 0f);
        }

        private void LateUpdate()
        {
            if (playback.InteractionSuccessCount != _lastSuccess)
            {
                _lastSuccess = playback.InteractionSuccessCount;
                _pulse = 1f;
            }
            if (playback.InteractionFailureCount != _lastFailure)
            {
                _lastFailure = playback.InteractionFailureCount;
                _pulse = -1f;
            }
            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.unscaledDeltaTime * 4f);
            float scale = IsStopped ? 0.92f : 1f + Mathf.Abs(_pulse) * 0.18f;
            transform.localScale = _baseScale * scale;
            if (trail != null) trail.emitting = !IsStopped;
        }
    }
}

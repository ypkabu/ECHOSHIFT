using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4MovementAudio : MonoBehaviour
    {
        [SerializeField] private Phase4AudioController audioController;
        [SerializeField, Min(0.1f)] private float stepInterval = 0.32f;
        [SerializeField, Min(0f)] private float movementThreshold = 0.001f;

        private Vector3 _lastPosition;
        private float _nextStep;

        public void Configure(Phase4AudioController audio) => audioController = audio;

        private void OnEnable()
        {
            _lastPosition = transform.position;
            _nextStep = Time.unscaledTime + stepInterval;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            float movement = (position - _lastPosition).sqrMagnitude;
            _lastPosition = position;
            if (movement < movementThreshold || Time.unscaledTime < _nextStep) return;
            audioController?.Play(Phase4AudioCue.Footstep, 0.18f, 0.96f);
            _nextStep = Time.unscaledTime + stepInterval;
        }
    }
}

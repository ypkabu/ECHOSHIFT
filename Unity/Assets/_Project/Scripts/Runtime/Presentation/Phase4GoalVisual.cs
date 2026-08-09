using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4GoalVisual : MonoBehaviour
    {
        [SerializeField] private GoalVolume goal;
        [SerializeField] private Transform beam;
        [SerializeField] private Phase4VisualSettings settings;
        private Vector3 _baseScale;
        public bool HasRequiredReferences => goal != null && beam != null && settings != null;

        public void Configure(GoalVolume source, Transform portalBeam,
            Phase4VisualSettings visualSettings)
        {
            goal = source;
            beam = portalBeam;
            settings = visualSettings;
        }

        private void Awake()
        {
            if (beam != null) _baseScale = beam.localScale;
        }

        private void LateUpdate()
        {
            if (!HasRequiredReferences) return;
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 2.4f) * 0.04f;
            beam.localScale = _baseScale * pulse;
            beam.Rotate(0f, Time.unscaledDeltaTime * 18f, 0f, Space.Self);
        }
    }
}

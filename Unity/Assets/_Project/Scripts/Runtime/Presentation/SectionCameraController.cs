using UnityEngine;

namespace EchoShift.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class SectionCameraController : MonoBehaviour
    {
        [SerializeField] private Phase3CameraSettings settings;
        [SerializeField] private Transform target;
        private Vector3 _velocity;

        public Transform Target => target;
        public bool HasValidReferences => settings != null && target != null;

        public void Configure(Phase3CameraSettings cameraSettings)
        {
            settings = cameraSettings;
        }

        public void SetTarget(Transform followTarget, bool snap)
        {
            target = followTarget;
            _velocity = Vector3.zero;
            if (snap && target != null)
            {
                ApplyPose(target.position + settings.Offset);
            }
        }

        private void LateUpdate()
        {
            if (!HasValidReferences) return;
            Vector3 desired = target.position + settings.Offset;
            Vector3 position = Vector3.SmoothDamp(
                transform.position, desired, ref _velocity,
                settings.SmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            ApplyPose(position);
        }

        private void ApplyPose(Vector3 position)
        {
            transform.position = position;
            Vector3 fixedLookDirection = settings.LookOffset - settings.Offset;
            if (fixedLookDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(fixedLookDirection, Vector3.up);
            }
        }
    }
}

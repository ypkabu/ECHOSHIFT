using EchoShift.Core;
using EchoShift.Gameplay;
using Unity.Profiling;
using UnityEngine;

namespace EchoShift.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class SectionCameraController : MonoBehaviour
    {
        public const string ProfilerMarkerName = "EchoShift.Camera.Update";
        private static readonly ProfilerMarker CameraUpdateMarker =
            new ProfilerMarker(ProfilerMarkerName);
        [SerializeField] private Phase3CameraSettings settings;
        [SerializeField] private Transform target;
        [SerializeField] private PuzzleSectionController activeSection;
        private Camera _camera;
        private Vector3 _velocity;
        private Vector3 _focus;
        private float _fieldOfViewVelocity;
        private bool _hasFocus;

        public Transform Target => target;
        public bool HasValidReferences => settings != null && target != null;
        public int ActiveSectionNumber => activeSection != null ? activeSection.SectionNumber : 0;
        public Vector3 CurrentFocus => _focus;
        public float CurrentFieldOfView => _camera != null ? _camera.fieldOfView : 0f;

        public void Configure(Phase3CameraSettings cameraSettings)
        {
            settings = cameraSettings;
            _camera = GetComponent<Camera>();
        }

        public void SetTarget(Transform followTarget, bool snap)
        {
            target = followTarget;
            activeSection = null;
            ResetMotion();
            if (snap && target != null && settings != null)
                ApplyFraming(true, 0f);
        }

        public void SetSection(PuzzleSectionController section, bool snap)
        {
            activeSection = section;
            target = section != null ? section.Player.transform : null;
            ResetMotion();
            if (snap && target != null && settings != null)
                ApplyFraming(true, 0f);
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void ResetMotion()
        {
            _velocity = Vector3.zero;
            _fieldOfViewVelocity = 0f;
            _hasFocus = false;
        }

        private void LateUpdate()
        {
            if (!HasValidReferences) return;
            ApplyFraming(false, Time.unscaledDeltaTime);
        }

        public void RefreshNowForTests(bool snap)
        {
            if (!HasValidReferences) return;
            ApplyFraming(snap, snap ? 0f : 1f / 60f);
        }

        private void ApplyFraming(bool snap, float deltaTime)
        {
            using ProfilerMarker.AutoScope scope = CameraUpdateMarker.Auto();
            _camera ??= GetComponent<Camera>();
            Vector3 desiredFocus = CalculateDesiredFocus(out float desiredFieldOfView);
            if (snap || !_hasFocus)
            {
                _focus = desiredFocus;
                _camera.fieldOfView = desiredFieldOfView;
                _hasFocus = true;
            }
            else
            {
                _focus = Vector3.SmoothDamp(
                    _focus, desiredFocus, ref _velocity,
                    settings.SmoothTime, Mathf.Infinity, deltaTime);
                _camera.fieldOfView = Mathf.SmoothDamp(
                    _camera.fieldOfView, desiredFieldOfView, ref _fieldOfViewVelocity,
                    settings.ZoomSmoothTime, Mathf.Infinity, deltaTime);
            }

            Vector3 offset = settings.Offset;
            transform.position = _focus + offset;
            if (offset.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(-offset, Vector3.up);
        }

        private Vector3 CalculateDesiredFocus(out float desiredFieldOfView)
        {
            Vector3 forward = target.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            else forward.Normalize();

            Vector3 focus = target.position + forward * settings.LookAheadDistance;
            int echoCount = 0;
            float groupRadius = 0f;
            LoopDirector director = activeSection != null ? activeSection.Director : null;
            if (director != null)
            {
                echoCount = director.EchoCount;
                if (echoCount >= 2)
                {
                    Vector3 center = target.position;
                    for (int i = 0; i < echoCount; i++)
                        center += director.GetEchoPlayback(i).transform.position;
                    center /= echoCount + 1f;
                    Vector3 groupOffset = center - target.position;
                    groupOffset.y = 0f;
                    groupOffset = Vector3.ClampMagnitude(
                        groupOffset, settings.MaximumGroupFocusOffset);
                    focus += groupOffset * settings.EchoGroupCenterWeight;

                    groupRadius = Vector3.Distance(target.position, center);
                    for (int i = 0; i < echoCount; i++)
                    {
                        float distance = Vector3.Distance(
                            director.GetEchoPlayback(i).transform.position, center);
                        if (distance > groupRadius) groupRadius = distance;
                    }
                }
            }

            if (activeSection != null)
            {
                focus = settings.ClampFocus(
                    activeSection.SectionNumber, activeSection.transform.position, focus);
            }

            desiredFieldOfView = echoCount >= 2
                ? settings.TwoEchoFieldOfView
                : settings.BaseFieldOfView;
            if (groupRadius > 4f)
            {
                desiredFieldOfView = Mathf.Min(
                    settings.MaximumFieldOfView,
                    desiredFieldOfView + (groupRadius - 4f) * 0.8f);
            }
            return focus;
        }
    }
}
